using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using Microsoft.AspNet.SignalR;
using CodeComb.Judge.Models;

namespace CodeComb.Web.SignalR
{
    public class JudgeHub : Hub
    {
        public Database.DB DbContext = new Database.DB();
        public static List<Client> Online = new List<Client>();
        public static Microsoft.AspNet.SignalR.IHubContext context = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<JudgeHub>();
        public override System.Threading.Tasks.Task OnDisconnected(bool stopCalled)
        {
            var index = Online.FindIndex(x=>x.Token == Context.ConnectionId);
            if (index >= 0)
            {
                Groups.Remove(Context.ConnectionId, Online[index].Username);
                Online.RemoveAt(index);
            }
            return base.OnDisconnected(stopCalled);
        }
        public UploadTask GetTestCase(int ID)
        {
            if (Online.FindIndex(x => x.Token == Context.ConnectionId) < 0)
                return null;
            var upload = (from tc in DbContext.TestCases
                          where tc.ID == ID
                          let hasoutput = tc.Output.Length == 0 ? false : true
                          select new UploadTask
                          {
                              ID = ID,
                              Hash = tc.Hash,
                              HasOutput = hasoutput,
                              Input = tc.Input,
                              Output = tc.Output
                          }).Single();
            return upload;
        }
        public string GetTestCaseHash(int ID)
        {
            if (Online.FindIndex(x => x.Token == Context.ConnectionId) < 0)
                return null;
            return DbContext.TestCases.Find(ID).Hash;
        }
        public void Auth(string Username, string Password)
        {
            var pwd = Helpers.Security.SHA1(Password);
            var user = (from u in DbContext.Users
                        where u.Username == Username
                        && u.Password == pwd
                        && u.RoleAsInt == (int)Entity.UserRole.Root
                        select u).SingleOrDefault();
            if (user != null)
            {
                user.Online = true;
                DbContext.SaveChanges();
                if (Online.FindIndex(x => x.Username == user.Username) >= 0)
                {
                    Clients.Group(user.Username).onMessage(String.Format("{0}已经在线，无法接受您的连接！", user.Username));
                }
                Online.Add(new Client { Token = Context.ConnectionId, Username = user.Username });
                Online = Online.Distinct().ToList();
                Groups.Add(Context.ConnectionId, user.Username);
                Clients.Group(user.Username).onMessage(String.Format("{0}，欢迎您。您已经成功注册成为Code Comb评测机！", user.Username));
                var time = DateTime.Now.AddMinutes(-5);
                var err_statuses = (from s in DbContext.Statuses
                           where s.ResultAsInt == (int)Entity.JudgeResult.Pending
                           || (s.ResultAsInt == (int)Entity.JudgeResult.Running && s.Time< time)
                           select s).ToList();
                foreach (var status in err_statuses)
                {
                    foreach (var jt in status.JudgeTasks.ToList())
                    {
                        jt.Result = Entity.JudgeResult.Running;
                        DbContext.SaveChanges();
                        var judgetask = new Judge.Models.JudgeTask(jt);
                        System.Threading.Tasks.Task.Factory.StartNew(() =>
                        {
                            SignalR.JudgeHub.context.Clients.Group(user.Username).Judge(judgetask);
                        });
                    }
                    status.Result = Entity.JudgeResult.Running;
                    DbContext.SaveChanges();
                    SignalR.CodeCombHub.context.Clients.All.onStatusCreated(new Models.View.Status(status));//推送新状态
                }
            }
        }
        public void JudgeFeedBack(JudgeFeedback jfb)
        {
            if (Online.FindIndex(x => x.Token == Context.ConnectionId) < 0)
                return;
            var jt = DbContext.JudgeTasks.Find(jfb.ID);
            jt.Hint = jfb.Hint;
            jt.MemoryUsage = jfb.MemoryUsage;
            jt.TimeUsage = jfb.TimeUsage;
            jt.Result = jfb.Result;
            DbContext.SaveChanges();
            if (jt.Status.JudgeTasks.Where(x => x.ResultAsInt == (int)Entity.JudgeResult.Running || x.ResultAsInt == (int)Entity.JudgeResult.Pending).Count() == 0)
            {
                jt.Status.ResultAsInt = jt.Status.JudgeTasks.Max(x => x.ResultAsInt);
                DbContext.SaveChanges();
                var contest = jt.Status.Problem.Contest;
                if (DateTime.Now >= contest.Begin && DateTime.Now < contest.End)
                {
                    SignalR.CodeCombHub.context.Clients.All.onStandingsChanged(contest.ID, new Models.View.Standing(jt.Status.User, contest));
                }
            }
            SignalR.CodeCombHub.context.Clients.All.onStatusChanged(new Models.View.Status(jt.Status));//推送新状态
        }
        public void HackFeedBack(HackFeedback hfb)
        {
            if (Online.FindIndex(x => x.Token == Context.ConnectionId) < 0)
                return;
            var hack = DbContext.Hacks.Find(hfb.HackID);
            hack.Result = hfb.Result;
            hack.Hint = hfb.Hint;
            DbContext.SaveChanges();
            if (hack.Result == Entity.HackResult.Success)
            {
                var hash = Helpers.Security.SHA1(hack.InputData);
                var tc = (from t in DbContext.TestCases where t.Hash == hash select t).FirstOrDefault();
                if (tc == null)
                { 
                    tc = new Entity.TestCase
                    {
                        Input = hack.InputData,
                        Hash = Helpers.Security.SHA1(hack.InputData),
                        Type = Entity.TestCaseType.Custom,
                        ProblemID = hack.Status.ProblemID,
                        Output = hfb.Output
                    };
                    DbContext.TestCases.Add(tc);
                }
                DbContext.JudgeTasks.Add(new Entity.JudgeTask
                {
                    StatusID = hack.StatusID,
                    Result = hfb.JudgeResult,
                    TimeUsage = hfb.TimeUsage,
                    MemoryUsage = hfb.MemoryUsage,
                    Hint = hfb.Hint,
                    TestCaseID = tc.ID
                });
                hack.Status.Result = Entity.JudgeResult.Hacked;
                DbContext.SaveChanges();
                SignalR.CodeCombHub.context.Clients.Group(hack.Defender.Username).onHacked(new Models.View.HackResult(hack));
                SignalR.CodeCombHub.context.Clients.Group(hack.Hacker.Username).onHackFinished(new Models.View.HackResult(hack));

                SignalR.CodeCombHub.context.Clients.All.onStatusChanged(new Models.View.Status(hack.Status));
                if (DateTime.Now >= hack.Status.Problem.Contest.Begin && DateTime.Now < hack.Status.Problem.Contest.End)
                {
                    SignalR.CodeCombHub.context.Clients.All.onStandingsChanged(hack.Status.Problem.Contest.ID, new Models.View.Standing(hack.Defender, hack.Status.Problem.Contest));
                    SignalR.CodeCombHub.context.Clients.All.onStandingsChanged(hack.Status.Problem.Contest.ID, new Models.View.Standing(hack.Hacker, hack.Status.Problem.Contest));
                }
            }
            else
            {
                SignalR.CodeCombHub.context.Clients.Group(hack.Hacker.Username).onHackFinished(new Models.View.HackResult(hack));
                if (DateTime.Now >= hack.Status.Problem.Contest.Begin && DateTime.Now < hack.Status.Problem.Contest.End)
                {
                    SignalR.CodeCombHub.context.Clients.All.onStandingsChanged(hack.Status.Problem.Contest.ID, new Models.View.Standing(hack.Hacker, hack.Status.Problem.Contest));
                }
            }
        }
    }
}