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
                          let hasoutput = tc.Output.Length == 0 ? true : false
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
            }
            SignalR.CodeCombHub.context.Clients.All.onStatusCreated(new Models.View.Status(jt.Status));//推送新状态
        }
    }
}