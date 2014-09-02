using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using PushSharp;
using PushSharp.Apple;
using PushSharp.Core;

namespace CodeComb.Web.SignalR
{
    public class MobileHub : Hub
    {
        public static List<MobileUser> Users = new List<MobileUser>();
        public Database.DB DbContext = new Database.DB();
        public static Microsoft.AspNet.SignalR.IHubContext context = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MobileHub>();

        #region Static Methods
        private Entity.User GetUser()
        {
            var user = (from dt in DbContext.DeviceTokens
                        where dt.Token == Context.ConnectionId
                        select dt.User).FirstOrDefault();
            return user;
        }
        private Entity.UserRole? CheckRole()
        {
            var user = GetUser();
            if (user == null) return null;
            return user.Role;
        }
        public static void PushTo(int UserID, string Message)
        {
            Database.DB DbContext = new Database.DB();
            var devicetokens =(from dt in DbContext.DeviceTokens
                                   where dt.UserID == UserID
                                   && dt.TypeAsInt == (int)Entity.DeviceType.iOS
                                   select dt).ToList();
            foreach(var dt in devicetokens)
                iOS_PushTo(dt.Token, Message);
        }
        public static void PushToAll(string Message)
        {
            Database.DB DbContext = new Database.DB();
            var devicetokens = (from dt in DbContext.DeviceTokens
                                where dt.TypeAsInt == (int)Entity.DeviceType.iOS
                                select dt).ToList();
            foreach (var dt in devicetokens)
                iOS_PushTo(dt.Token, Message);
        }
        public static void iOS_PushTo(string device, string msg)
        {
            Helpers.Push.push.QueueNotification(
                new AppleNotification()
                    .ForDeviceToken(device)
                    .WithAlert(msg)
                    .WithBadge(1)
                    .WithSound("default")
            );
        }
        #endregion
        public bool Authorize(string Username, string Password)
        { 
            var pwd = Helpers.Security.SHA1(Password);
            var user = (from u in DbContext.Users
                        where u.Username == Username
                         && u.Password == pwd
                        select u).SingleOrDefault();
            if (user == null)
                return false;
            var devicetokens = (from dt in DbContext.DeviceTokens
                               where dt.Token == Context.ConnectionId
                               select dt).ToList();
            foreach (var dt in devicetokens)
                DbContext.DeviceTokens.Remove(dt);
            DbContext.SaveChanges();
            DbContext.DeviceTokens.Add(new Entity.DeviceToken 
            { 
                Token = Context.ConnectionId,
                Type = Entity.DeviceType.SignalR,
                UserID = user.ID
            });
            DbContext.SaveChanges();
            Groups.Add(Context.ConnectionId, user.Username);
            return true;
        }
        public bool RegisteriOSPushNotification(string DeviceToken)
        {
            var user = GetUser();
            if (user == null) return false;
            DbContext.DeviceTokens.Add(new Entity.DeviceToken 
            { 
                Token = DeviceToken,
                Type = Entity.DeviceType.iOS,
                UserID = user.ID
            });
            DbContext.SaveChanges();
            return true;
        }
        public List<Mobile.Models.Contest> GetContests(int page)
        {
            var user = GetUser();
            if (user == null) return null;
            IEnumerable<Entity.Contest> _contests = (from c in DbContext.Contests
                                                    select c);
            if (CheckRole() < Entity.UserRole.Master)
            {
                _contests.Where(x => (from cm in x.Managers select x.ID).Contains(user.ID));
            }
            _contests.OrderByDescending(x => x.ID).Skip(20).Take(20).ToList();
            var contests = new List<Mobile.Models.Contest>();
            foreach (var c in _contests)
                contests.Add(new Mobile.Models.Contest 
                { 
                    ID = c.ID,
                    Begin = c.Begin,
                    End = c.End,
                    Format = c.Format.ToString(),
                    RestBegin = c.RestBegin,
                    RestEnd = c.RestEnd,
                    Title = c.Title
                });
            return contests;
        }
        public List<Mobile.Models.Clarification> GetClarifications(int ContestID)
        {
            var user = GetUser();
            if (user == null) return null;
            var contest = DbContext.Contests.Find(ContestID);
            if (contest == null) return null;
            if (CheckRole() < Entity.UserRole.Master && !(from cm in contest.Managers select cm.UserID).Contains(user.ID))
                return null;
            var clars = (from c in DbContext.Clarifications
                         where c.ContestID == ContestID
                         orderby c.Time descending
                         select c).ToList();
            var clarifications = new List<Mobile.Models.Clarification>();
            foreach (var c in clars)
                clarifications.Add(new Mobile.Models.Clarification 
                { 
                    ID = c.ID,
                    Answer = c.Answer,
                    Problem = c.ProblemID == null ? "General" : c.Problem.Title,
                    Time = c.Time,
                    Question = c.Question,
                    ContestID = c.ContestID,
                    Status = c.StatusAsInt
                });
            return clarifications;
        }
        public bool ResponseClarification(int ClarID, string Answer, bool BroadCast)
        {
            var user = GetUser();
            if (user == null) return false;
            var clar = DbContext.Clarifications.Find(ClarID);
            if (clar == null) return false;
            var contest = DbContext.Contests.Find(clar.ContestID);
            if (contest == null) return false;
            if (CheckRole() < Entity.UserRole.Master && !(from cm in contest.Managers select cm.UserID).Contains(user.ID))
                return false;
            clar.Answer = Answer;
            if (BroadCast)
            {
                clar.Status = Entity.ClarificationStatus.BroadCast;
                SignalR.CodeCombHub.context.Clients.All.onClarificationsResponsed(new Models.View.Clar(clar));
                SignalR.MobileHub.context.Clients.All.onClarificationsResponsed(new Mobile.Models.Clarification 
                {
                    ID = clar.ID,
                    Answer = clar.Answer,
                    Problem = clar.ProblemID == null ? "General" : clar.Problem.Title,
                    Time = clar.Time,
                    Question = clar.Question,
                    ContestID = clar.ContestID,
                    Status = clar.StatusAsInt
                });
            }
            else 
            { 
                clar.Status = Entity.ClarificationStatus.Private;
                SignalR.CodeCombHub.context.Clients.Group(clar.User.Username).onClarificationsResponsed(new Models.View.Clar(clar));
                SignalR.MobileHub.context.Clients.Group(clar.User.Username).onClarificationsResponsed(new Mobile.Models.Clarification
                {
                    ID = clar.ID,
                    Answer = clar.Answer,
                    Problem = clar.ProblemID == null ? "General" : clar.Problem.Title,
                    Time = clar.Time,
                    Question = clar.Question,
                    ContestID = clar.ContestID,
                    Status = clar.StatusAsInt
                });
            }
            return true;
        }
    }
}