using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CodeComb.Models.WebAPI;

namespace CodeComb.Web.Controllers
{
    public class APIController : BaseController
    {
        //
        // GET: /API/
        public ActionResult Index()
        {
            return View();
        }
        public Entity.User CheckUser(string token)
        {
            var user = (from d in DbContext.DeviceTokens
                        where d.Token == token
                        select d.User).SingleOrDefault();
            return user;
        }
        public ActionResult Auth(string Username, string Password)
        {
            var pwd = Helpers.Security.SHA1(Password);
            var user = (from u in DbContext.Users
                        where u.Username == Username
                        && u.Password == pwd
                        select u).SingleOrDefault();
            if (user == null)
            {
                return Json(new Auth { AccessToken = null, Code = 400, IsSuccess = false, Info = "用户名或密码错误！" });
            }
            else 
            {
                var _t = (from t in DbContext.DeviceTokens where t.UserID == user.ID && t.TypeAsInt == (int)Entity.DeviceType.System select t).FirstOrDefault();
                if (_t != null)
                {
                    return Json(new Auth { AccessToken = _t.Token, Code = 0, IsSuccess = true, Info = "" });
                }
                try
                {
                    string token;
                    bool existed;
                    do
                    {
                        token = Helpers.String.RandomString(64);
                        existed = (from t in DbContext.DeviceTokens
                                   where t.Token == token
                                   select t).Count() > 0;
                    }
                    while (existed);
                    DbContext.DeviceTokens.Add(new Entity.DeviceToken
                    {
                        Token = token,
                        Type = Entity.DeviceType.System,
                        UserID = user.ID
                    });
                    DbContext.SaveChanges();
                    return Json(new Auth { AccessToken = token, Code = 0, IsSuccess = true, Info = "" });
                }
                catch (Exception e)
                {
                    return Json(new Auth { AccessToken = null, Code = 300, IsSuccess = false, Info = e.ToString() });
                }
            }
        }

        [HttpPost]
        public ActionResult RegisterPushService(string Token, int DeviceType, string DeviceToken)
        {
            var user = CheckUser(Token);
            if (user == null)
                return Json(new Base
                {
                    Code = 500,
                    IsSuccess = false,
                    Info = "AccessToken不正确"
                });
            var _dt = (from dt in DbContext.DeviceTokens
                      where dt.Token == DeviceToken
                      && dt.TypeAsInt == (int)Entity.DeviceType.iOS
                      select dt).FirstOrDefault();
            if (_dt != null)
            {
                _dt.UserID = user.ID;
                DbContext.SaveChanges();
                return Json(new Base
                {
                    Code = 0,
                    IsSuccess = true,
                    Info = ""
                });
            }
            DbContext.DeviceTokens.Add(new Entity.DeviceToken
            {
                Token = DeviceToken,
                Type = Entity.DeviceType.iOS,
                UserID = user.ID
            });
            DbContext.SaveChanges();
            return Json(new Base
                {
                    Code = 0,
                    IsSuccess = true,
                    Info = ""
                });
        }

        [HttpPost]
        public ActionResult GetProfile(string Token)
        {
            var user = CheckUser(Token);
            if (user != null)
                return Json(new Profile { 
                    UserID = user.ID, 
                    AvatarURL = Helpers.Gravatar.GetAvatarURL(user.Email,180),
                    Motto = user.Motto,
                    Rating = user.Ratings.Sum(x=>x.Credit)+1500,
                    Nickname = user.Nickname,
                    Code = 0,
                    IsSuccess = true,
                    Info = ""
                });
            else
                return Json(new Profile
                {
                    UserID = -1,
                    AvatarURL = "",
                    Motto = null,
                    Rating = -1,
                    Nickname = "",
                    Code = 500,
                    IsSuccess = false,
                    Info = "AccessToken不正确"
                });
        }

        [HttpPost]
        public ActionResult GetContests(string Token, int? Page)
        {
            var user = CheckUser(Token);
            if(user == null)
                return Json(new Contests
                {
                    List = null,
                    Code = 500,
                    IsSuccess = false,
                    Info = "AccessToken不正确"
                });
            if (Page == null) Page = 0;
            var contests = (from c in DbContext.Contests
                            orderby c.End descending
                            select c).Skip(10 * Page.Value).Take(10).ToList();
            var ret = new Contests() { Code = 0, IsSuccess = true, Info = "", PageCount = Convert.ToInt32(Math.Ceiling(DbContext.Contests.Count() / 10f)), List=new List<Contest>() };
            foreach (var contest in contests)
            {
                ret.List.Add(new Contest 
                { 
                    ContestID = contest.ID,
                    Begin= contest.Begin,
                    End=contest.End,
                    Format=contest.Format.ToString(),
                    FormatAsInt = contest.FormatAsInt,
                    RestBegin = contest.RestBegin,
                    RestEnd = contest.RestEnd,
                    Title = contest.Title
                });
            }
            return Json(ret);
        }

        [HttpPost]
        public ActionResult GetManagedContests(string Token, int? Page)
        {
            var user = CheckUser(Token);
            if (user == null)
                return Json(new Contests
                {
                    List = null,
                    Code = 500,
                    IsSuccess = false,
                    PageCount = -1,
                    Info = "AccessToken不正确"
                });
            if (Page == null) Page = 0;
            IQueryable<Entity.Contest> contests = (from c in DbContext.Contests
                                                   orderby c.End descending
                                                   select c);
            if (user.Role < Entity.UserRole.Master)
                contests.Where(x=> (from cm in x.Managers select cm.UserID).Contains(user.ID));
            contests.Skip(10 * Page.Value).Take(10).ToList();
            var ret = new Contests() { Code = 0, IsSuccess = true, Info = "", PageCount = Convert.ToInt32(Math.Ceiling(DbContext.Contests.Count() / 10f)), List=new List<Contest>() };
            foreach (var contest in contests)
            {
                ret.List.Add(new Contest
                {
                    ContestID = contest.ID,
                    Begin = contest.Begin,
                    End = contest.End,
                    Format = contest.Format.ToString(),
                    FormatAsInt = contest.FormatAsInt,
                    RestBegin = contest.RestBegin,
                    RestEnd = contest.RestEnd,
                    Title = contest.Title
                });
            }
            return Json(ret);
        }

        [HttpPost]
        public ActionResult GetClarifications(string Token, int ContestID)
        {
            var user = CheckUser(Token);
            if (user == null)
                return Json(new Clarifications
                {
                    List = null,
                    Code = 500,
                    IsSuccess = false,
                    Info = "AccessToken不正确"
                });
            var contest = DbContext.Contests.Find(ContestID);
            IQueryable<Entity.Clarification> clarifications = (from c in DbContext.Clarifications
                                                         where c.ContestID == ContestID
                                                         orderby c.Time descending
                                                         select c);
            if (user.Role < Entity.UserRole.Master && !(from cm in contest.Managers select cm.ID).Contains(user.ID))
                clarifications.Where(x => x.UserID == user.ID || x.Status == Entity.ClarificationStatus.BroadCast);
            clarifications.ToList();
            var ret = new Clarifications() { Code = 0, IsSuccess = true, Info = "", List=new List<Clarification>() };
            foreach (var clarification in clarifications)
            {
                ret.List.Add(new Clarification
                {
                    ClarID = clarification.ID,
                    Answer = clarification.Answer,
                    Question = clarification.Question,
                    Status = clarification.Status.ToString(),
                    StatusAsInt = clarification.StatusAsInt,
                    Category = clarification.ProblemID == null ? "General" : clarification.Problem.Title,
                    Time = clarification.Time,
                    ContestID = clarification.ContestID
                });
            }
            return Json(ret);
        }

        [HttpPost]
        public ActionResult ResponseClarification(string Token, int ClarID, string Answer, int Status)
        {
            var user = CheckUser(Token);
            if (user == null)
                return Json(new Base
                {
                    Code = 500,
                    IsSuccess = false,
                    Info = "AccessToken不正确"
                });
            var clar = DbContext.Clarifications.Find(ClarID);
            var contest = clar.Problem.Contest;
            if (user.Role < Entity.UserRole.Master && !(from cm in contest.Managers select cm.UserID).Contains(user.ID))
                return Json(new Base { Code = 1001, Info = "权限不足", IsSuccess = false });
            clar.Answer = Answer;
            clar.StatusAsInt = Status;
            DbContext.SaveChanges();
            if (clar.Status == Entity.ClarificationStatus.BroadCast)
            {
                clar.Status = Entity.ClarificationStatus.BroadCast;
                SignalR.CodeCombHub.context.Clients.All.onClarificationsResponsed(new Models.View.Clar(clar));
                SignalR.MobileHub.context.Clients.All.onClarificationsResponsed(new Clarification
                {
                    ClarID = clar.ID,
                    Answer = clar.Answer,
                    Category = clar.ProblemID == null ? "General" : clar.Problem.Title,
                    Time = clar.Time,
                    Question = clar.Question,
                    ContestID = clar.ContestID,
                    StatusAsInt = clar.StatusAsInt,
                    Status = clar.Status.ToString()
                });
            }
            else
            {
                clar.Status = Entity.ClarificationStatus.Private;
                SignalR.CodeCombHub.context.Clients.Group(clar.User.Username).onClarificationsResponsed(new Models.View.Clar(clar));
                SignalR.MobileHub.context.Clients.Group(clar.User.Username).onClarificationsResponsed(new Clarification
                {
                    ClarID = clar.ID,
                    Answer = clar.Answer,
                    Category = clar.ProblemID == null ? "General" : clar.Problem.Title,
                    Time = clar.Time,
                    Question = clar.Question,
                    ContestID = clar.ContestID,
                    StatusAsInt = clar.StatusAsInt,
                    Status = clar.Status.ToString()
                });
            }
            return Json(new Base { Code = 0, Info = "", IsSuccess = true });
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult BroadCast(string Token, string Message)
        {
            var user = CheckUser(Token);
            if (user == null)
                return Json(new Base
                {
                    Code = 500,
                    IsSuccess = false,
                    Info = "AccessToken不正确"
                });
            if(user.Role < Entity.UserRole.Master)
                return Json(new Base { Code = 1001, Info = "权限不足", IsSuccess = false });
            SignalR.CodeCombHub.context.Clients.All.onBroadCast(Helpers.HtmlFilter.Instance.SanitizeHtml(Message));
            SignalR.MobileHub.PushToAll(Helpers.String.CleanHTML(Message));
            SignalR.MobileHub.context.Clients.All.onBroadCast(Helpers.String.CleanHTML(Message));
            return Json(new Base { Code = 0, Info = "", IsSuccess = true });
        }

        [HttpPost]
        public ActionResult GetContacts(string Token)
        {
            var user = CheckUser(Token);
            if (user == null)
                return Json(new Base
                {
                    Code = 500,
                    IsSuccess = false,
                    Info = "AccessToken不正确"
                });
            var users = (from pm in DbContext.Messages
                         where pm.ReceiverID == user.ID
                         select pm.Sender).ToList();
            users.Union((from pm in DbContext.Messages
                         where pm.SenderID == user.ID
                         select pm.Receiver).ToList());
            users.Distinct();
            var ret = new Contacts() { IsSuccess = true, Code = 0, Info = "", List=new List<Contact>()};
            foreach (var u in users)
            {
                ret.List.Add(new Contact 
                {
                    UserID = u.ID,
                    AvatarURL = Helpers.Gravatar.GetAvatarURL(u.Gravatar, 180),
                    Nickname = u.Nickname,
                    UnreadMessageCount = (from m in DbContext.Messages
                                              where m.ReceiverID == user.ID
                                              && m.Read == false
                                              select m).Count()
                });
            }
            return Json(ret);
        }

        [HttpPost]
        public ActionResult GetChatRecords(string Token, int UserID)
        {
            var user = CheckUser(Token);
            if (user == null)
                return Json(new Messages
                {
                    List = null,
                    Code = 500,
                    IsSuccess = false,
                    Info = "AccessToken不正确"
                });
            var messages = (from m in DbContext.Messages
                                                   where m.ReceiverID == user.ID
                                                   && m.SenderID == UserID
                                                   orderby m.Time descending
                                                   select m).Take(50).ToList();
            var ret = new Messages { Code = 0, Info = "", IsSuccess = true, List = new List<Message>() };
            foreach (var message in messages)
            {
                message.Read = true;
                ret.List.Add(new Message 
                { 
                    Content = message.Content,
                    SenderID = message.SenderID,
                    ReceiverID = message.ReceiverID,
                    Time = message.Time
                });
            }
            DbContext.SaveChanges();
            return Json(ret);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult SendMessage(string Token, int UserID, string Content)
        {
            var user = CheckUser(Token);
            if (user == null)
                return Json(new Base
                {
                    Code = 500,
                    IsSuccess = false,
                    Info = "AccessToken不正确"
                });
            var msg = new Entity.Message 
            { 
                Content = Content,
                SenderID = user.ID,
                ReceiverID = UserID,
                Time = DateTime.Now,
                Read = false
            };
            DbContext.Messages.Add(msg);
            DbContext.SaveChanges();
            var name1 = DbContext.Users.Find(msg.ReceiverID).Username;
            var name2 = DbContext.Users.Find(msg.SenderID).Username;
            SignalR.CodeCombHub.context.Clients.Group(name1).onMessageReceived(msg.ID);
            SignalR.CodeCombHub.context.Clients.Group(name2).onMessageReceived(msg.ID);
            SignalR.MobileHub.context.Clients.Group(name1).onMessageReceived(new CodeComb.Models.WebAPI.Message
            {
                Content = msg.Content,
                Time = msg.Time,
                SenderID = msg.SenderID,
                ReceiverID = msg.ReceiverID
            });
            SignalR.MobileHub.context.Clients.Group(name2).onMessageReceived(new CodeComb.Models.WebAPI.Message
            {
                Content = msg.Content,
                Time = msg.Time,
                SenderID = msg.SenderID,
                ReceiverID = msg.ReceiverID
            });
            SignalR.MobileHub.PushTo(msg.ReceiverID, msg.Content);
            return Json(new Base
            {
                Code = 0,
                IsSuccess = false,
                Info = ""
            });
        }

        [HttpPost]
        public ActionResult LoginByBarCode(string Token, string BarCode)
        {
            var user = CheckUser(Token);
            if (user == null)
                return Json(new Base
                {
                    Code = 500,
                    IsSuccess = false,
                    Info = "AccessToken不正确"
                });
            if (SignalR.CodeCombHub.BarCode[BarCode] == null)
            {
                return Json(new Base
                {
                    Code = 700,
                    IsSuccess = false,
                    Info = "二维码不正确"
                });
            }
            var conn_id = (string)SignalR.CodeCombHub.BarCode[BarCode];
            string token;
            bool existed;
            do
            {
                token = Helpers.String.RandomString(32);
                if (SignalR.CodeCombHub.LoginTokens[token] == null)
                {
                    SignalR.CodeCombHub.LoginTokens[token] = user.ID;
                    existed = false;
                }
                else
                    existed = true;
            }
            while(existed);
            SignalR.CodeCombHub.context.Clients.Client(conn_id).onLoginToken(token);
            return Json(new Base
                {
                    Code = 0,
                    IsSuccess = true,
                    Info = ""
                });
        }
    }
}