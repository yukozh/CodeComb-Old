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
            var HistroyTime = Convert.ToDateTime("2014-8-1 0:00");
            var contests = (from c in DbContext.Contests
                            where c.End >=HistroyTime
                            orderby c.End descending
                            select c).Skip(10 * Page.Value).Take(10).ToList();
            var ret = new Contests { Code = 0, IsSuccess = true, Info = "", PageCount = DbContext.Contests.Count() / 10 + 1 , List = new List<Contest>() };
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
            var contest = clar.Contest;
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
            IEnumerable<Entity.User> users = (from pm in DbContext.Messages
                         where pm.ReceiverID == user.ID
                         select pm.Sender).ToList();
            users.Union((from pm in DbContext.Messages
                         where pm.SenderID == user.ID
                         select pm.Receiver).ToList());
           users = users.Distinct();
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
                                              && m.SenderID == u.ID
                                              && m.Read == false
                                              select m).Count(),
                    Motto = u.Motto
                });
            }
            return Json(ret);
        }

        [HttpPost]
        public ActionResult FindContacts(string Token, string Nickname)
        {
            var user = CheckUser(Token);
            if (user == null)
                return Json(new Base
                {
                    Code = 500,
                    IsSuccess = false,
                    Info = "AccessToken不正确"
                });
            var users = (from u in DbContext.Users
                                              where u.Nickname.Contains(Nickname) || Nickname.Contains(u.Nickname)
                                              select u).ToList();
            var ret = new Contacts() { IsSuccess = true, Code = 0, Info = "", List = new List<Contact>() };
            foreach (var u in users)
            {
                ret.List.Add(new Contact
                {
                    UserID = u.ID,
                    AvatarURL = Helpers.Gravatar.GetAvatarURL(u.Gravatar, 180),
                    Nickname = u.Nickname,
                    UnreadMessageCount = (from m in DbContext.Messages
                                          where m.ReceiverID == user.ID
                                          && m.SenderID == u.ID
                                          && m.Read == false
                                          select m).Count(),
                    Motto = u.Motto
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
                            where (m.ReceiverID == user.ID && m.SenderID == UserID)
                            || (m.SenderID == UserID && m.ReceiverID == user.ID)
                            orderby m.Time descending
                            select m).Take(50).ToList();
            messages.Reverse(0, messages.Count);
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
            SignalR.MobileHub.PushTo(msg.ReceiverID, msg.Sender.Nickname + ":" + msg.Content);
            return Json(new Base
            {
                Code = 0,
                IsSuccess = true,
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

        [HttpPost]
        public ActionResult GetGroups(string Token, int? Page)
        {
            var user = CheckUser(Token);
            if (user == null)
                return Json(new Groups
                {
                    List = null,
                    Code = 500,
                    IsSuccess = false,
                    Info = "AccessToken不正确"
                });
            if(Page == null) Page = 0;
            IEnumerable<Entity.Group> groups = (from g in DbContext.Groups
                                                where (from m in g.GroupMembers
                                                       where m.UserID == user.ID
                                                       select m).Count() > 0
                                                orderby g.ID ascending
                                                select g);
            var ret = new CodeComb.Models.WebAPI.Groups { IsSuccess = true, Code = 0, Info = "", List = new List<Group>(), PageCount = groups.Count() / 10 + 1 };
            groups = groups.Skip(Page.Value * 10).Take(10).ToList();
            foreach (var group in groups)
            {
                ret.List.Add(new CodeComb.Models.WebAPI.Group 
                { 
                    ID = group.ID,
                    Description = group.Description,
                    Icon = group.Icon,
                    MemberCount = group.GroupMembers.Count,
                    Title = group.Title
                });
            }
            return Json(ret);
        }

        [HttpPost]
        public ActionResult CreateGroup(string Token, string Title, string Description, int JoinMethod)
        {
            var user = CheckUser(Token);
            if (user == null)
                return Json(new Base
                {
                    Code = 500,
                    IsSuccess = false,
                    Info = "AccessToken不正确"
                });
            var group = new Entity.Group 
            { 
                Title = Title,
                Description = Description,
                Icon = null,
                JoinMethodAsInt = JoinMethod
            };
            DbContext.Groups.Add(group);
            DbContext.GroupMembers.Add(new Entity.GroupMember
            {
                GroupID = group.ID,
                JoinTime = DateTime.Now,
                Role = Entity.GroupRole.Root,
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
        public ActionResult ModifyGroup(string Token, int GroupID, string Title, string Description, int JoinMethod)
        {
            var user = CheckUser(Token);
            if (user == null)
                return Json(new Base
                {
                    Code = 500,
                    IsSuccess = false,
                    Info = "AccessToken不正确"
                });
            var group = DbContext.Groups.Find(GroupID);
            if ((from gm in @group.GroupMembers where gm.UserID == user.ID && gm.RoleAsInt >= (int)Entity.GroupRole.Master select gm).Count() == 0)
                return Json(new Base
                {
                    Code = 802,
                    IsSuccess = false,
                    Info = "您不是该群的管理员"
                });
            group.Title = Title;
            group.Description = Description;
            group.JoinMethodAsInt = JoinMethod;
            DbContext.SaveChanges();
            return Json(new Base
            {
                Code = 0,
                IsSuccess = true,
                Info = ""
            });
        }

        [HttpPost]
        public ActionResult KickGroupMember(string Token, int GroupID, int UserID)
        {
            var user = CheckUser(Token);
            if (user == null)
                return Json(new Base
                {
                    Code = 500,
                    IsSuccess = false,
                    Info = "AccessToken不正确"
                });
            var group = DbContext.Groups.Find(GroupID);
            var me = (from gm in @group.GroupMembers where gm.UserID == user.ID && gm.RoleAsInt >= (int)Entity.GroupRole.Master select gm).FirstOrDefault();
            if (me == null)
                return Json(new Base
                {
                    Code = 802,
                    IsSuccess = false,
                    Info = "您不是该群的管理员"
                });
            var group_member = (from gm in DbContext.GroupMembers
                                where gm.GroupID == GroupID
                                && gm.UserID == UserID
                                select gm).FirstOrDefault();
            if (group_member.Role >= me.Role)
                return Json(new Base
                {
                    Code = 807,
                    IsSuccess = false,
                    Info = "您的权限不能踢出该用户"
                });
            if(group_member == null)
                return Json(new Base
                {
                    Code = 803,
                    IsSuccess = false,
                    Info = "该用户不在群中"
                });
            DbContext.GroupMembers.Remove(group_member);
            DbContext.SaveChanges();
            return Json(new Base
            {
                Code = 0,
                IsSuccess = true,
                Info = ""
            });
        }

        [HttpPost]
        public ActionResult JoinGroup(string Token, int GroupID, string Message)
        {
            var user = CheckUser(Token);
            if (user == null)
                return Json(new Base
                {
                    Code = 500,
                    IsSuccess = false,
                    Info = "AccessToken不正确"
                });
            var group = DbContext.Groups.Find(GroupID);
            if((from gm in @group.GroupMembers where gm.UserID == user.ID && gm.GroupID == GroupID select gm).Count() > 0)
                return Json(new Base
                {
                    Code = 804,
                    IsSuccess = false,
                    Info = "已经在群中"
                });
            if(group.JoinMethod == Entity.JoinMethod.Forbidden)
                return Json(new Base
                {
                    Code = 805,
                    IsSuccess = false,
                    Info = "该群不允许任何人加入"
                });
            if (group.JoinMethod == Entity.JoinMethod.Message)
            { 
                if((from ga in @group.GroupJoinApplications where ga.UserID == user.ID && ga.GroupID == GroupID select ga).Count() > 0)
                    return Json(new Base
                    {
                        Code = 806,
                        IsSuccess = false,
                        Info = "您已经提交过入群申请，请耐心等待审核"
                    });
                DbContext.GroupJoinApplications.Add(new Entity.GroupJoinApplication 
                { 
                    GroupID = GroupID,
                    Status = Entity.GroupJoinStatus.Pending,
                    UserID = user.ID,
                    Message = Message,
                    Response = ""
                });
                DbContext.SaveChanges();
                return Json(new Base
                {
                    Code = 1,
                    IsSuccess = true,
                    Info = "入群申请已提交，请等待管理员审核"
                });
            }
            DbContext.GroupMembers.Add(new Entity.GroupMember 
            { 
                Role = Entity.GroupRole.Member,
                GroupID = GroupID,
                UserID = user.ID,
                JoinTime = DateTime.Now
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
        public ActionResult GetGroupApplications(string Token, int GroupID, int? Page)
        {
            var user = CheckUser(Token);
            if (user == null)
                return Json(new GroupApplications
                {
                    Code = 500,
                    IsSuccess = false,
                    Info = "AccessToken不正确"
                });
            if (Page == null) Page = 0;
            var group = DbContext.Groups.Find(GroupID);
            if ((from gm in @group.GroupMembers where gm.UserID == user.ID && gm.RoleAsInt >= (int)Entity.GroupRole.Master select gm).Count() == 0)
                return Json(new GroupApplications
                {
                    Code = 802,
                    IsSuccess = false,
                    Info = "您不是该群的管理员"
                });
            var group_applications = group.GroupJoinApplications.OrderByDescending(x=>x.Time).Skip(Page.Value * 10).Take(10).ToList();
            var ret = new CodeComb.Models.WebAPI.GroupApplications { Code = 0, IsSuccess = true, List = new List<GroupApplication>(), Info = "", PageCount = group.GroupJoinApplications.Count / 10 + 1 };
            foreach (var ga in group_applications)
            {
                ret.List.Add(new CodeComb.Models.WebAPI.GroupApplication 
                { 
                    ID = ga.ID,
                    GroupID = ga.GroupID,
                    Message = ga.Message,
                    UserID = ga.UserID,
                    StatusAsInt = ga.StatusAsInt,
                    Status = ga.Status.ToString(),
                    Nickname = ga.User.Nickname,
                    AvatarURL = Helpers.Gravatar.GetAvatarURL(ga.User.Gravatar, 180),
                    Response = ga.Response
                });
            }
            return Json(ret);
        }

        [HttpPost]
        public ActionResult ResponseGroupApplication(string Token, int ApplicationID, int Status, string Response)
        {
            var user = CheckUser(Token);
            if (user == null)
                return Json(new Base
                {
                    Code = 500,
                    IsSuccess = false,
                    Info = "AccessToken不正确"
                });
            var application = DbContext.GroupJoinApplications.Find(ApplicationID);
            var group = application.Group;
            if ((from gm in @group.GroupMembers where gm.UserID == user.ID && gm.RoleAsInt >= (int)Entity.GroupRole.Master select gm).Count() == 0)
                return Json(new Base
                {
                    Code = 802,
                    IsSuccess = false,
                    Info = "您不是该群的管理员"
                });
            application.Response = Response;
            application.StatusAsInt = Status;
            DbContext.SaveChanges();
            switch (application.Status)
            { 
                case Entity.GroupJoinStatus.Accepted:
                    SignalR.MobileHub.PushTo(application.UserID, group.Title + " 的群管理员接受了您的入群申请");
                    break;
                case Entity.GroupJoinStatus.Rejected:
                    SignalR.MobileHub.PushTo(application.UserID, group.Title + " 的群管理员拒绝了您的入群申请");
                    break;
                default: break;
            }
            SignalR.MobileHub.context.Clients.Client(application.User.Username).onGroupApplicationResponsed(new CodeComb.Models.WebAPI.GroupApplication 
            {
                ID = application.ID,
                GroupID = application.GroupID,
                AvatarURL = Helpers.Gravatar.GetAvatarURL(application.User.Gravatar, 180),
                StatusAsInt = application.StatusAsInt,
                Status = application.Status.ToString(),
                Message = application.Message,
                Response = application.Response,
                Nickname = application.User.Nickname,
                UserID = application.UserID
            });
            return Json(new Base
            {
                Code = 0,
                IsSuccess = true,
                Info = ""
            });
        }

        [HttpPost]
        public ActionResult GetGroupChat(string Token, int GroupID, int? Page)
        {
            var user = CheckUser(Token);
            if (user == null)
                return Json(new GroupChats
                {
                    Code = 500,
                    IsSuccess = false,
                    Info = "AccessToken不正确"
                });
            var group = DbContext.Groups.Find(GroupID);
            if ((from gm in @group.GroupMembers where gm.UserID == user.ID select gm).Count() == 0)
                return Json(new Base
                {
                    Code = 808,
                    IsSuccess = false,
                    Info = "您不是该群的成员"
                });
            if (Page == null) Page = 0;
            var ret = new GroupChats { Code = 0, IsSuccess = true, Info = "", List = new List<GroupChat>(), PageCount = group.GroupChats.Count / 50 + 1 };
            var chats = group.GroupChats.OrderByDescending(x => x.Time).Skip(Page.Value * 50).Take(50).Reverse();
            foreach (var chat in chats)
            {
                ret.List.Add(new GroupChat 
                { 
                    ID = chat.ID,
                    AvatarURL = Helpers.Gravatar.GetAvatarURL(chat.User.Gravatar, 180),
                    GroupID = chat.GroupID,
                    Message = chat.Message,
                    Nickname = chat.User.Nickname,
                    Time = chat.Time,
                    UserID = chat.UserID
                });
            }
            return Json(ret);
        }

        [HttpPost]
        public ActionResult SendGroupMessage(string Token, int GroupID, string Message)
        {
            var user = CheckUser(Token);
            if (user == null)
                return Json(new Base
                {
                    Code = 500,
                    IsSuccess = false,
                    Info = "AccessToken不正确"
                });
            var group = DbContext.Groups.Find(GroupID);
            if ((from gm in @group.GroupMembers where gm.UserID == user.ID select gm).Count() == 0)
                return Json(new Base
                {
                    Code = 808,
                    IsSuccess = false,
                    Info = "您不是该群的成员"
                });
            var groupchat = new Entity.GroupChat 
            { 
                GroupID = GroupID,
                Message = Message,
                Time = DateTime.Now,
                UserID = user.ID
            };
            DbContext.GroupChats.Add(groupchat);
            DbContext.SaveChanges();
            var group_users = (from u in DbContext.Users
                               where (from g in DbContext.GroupMembers
                                      where g.ID == GroupID
                                      && g.UserID == u.ID
                                      select g).Count() > 0
                               select u).ToList();
            foreach (var u in group_users)
            {
                SignalR.MobileHub.context.Clients.Client(u.Username).onGroupMessage(new GroupChat 
                { 
                    ID = groupchat.ID,
                    AvatarURL = Helpers.Gravatar.GetAvatarURL(groupchat.User.Gravatar, 180),
                    Message = groupchat.Message,
                    Time = groupchat.Time,
                    GroupID = groupchat.GroupID,
                    Nickname = groupchat.User.Nickname,
                    UserID = groupchat.UserID
                });
                //Todo: Push to web
                SignalR.MobileHub.PushTo(u.ID, groupchat.Group.Title + ": " + Message);
            }
            return Json(new Base
            {
                Code = 0,
                IsSuccess = true,
                Info = ""
            });
        }

        [HttpPost]
        public ActionResult QuitGroup(string Token, int GroupID)
        {
            var user = CheckUser(Token);
            if (user == null)
                return Json(new Base
                {
                    Code = 500,
                    IsSuccess = false,
                    Info = "AccessToken不正确"
                });
            var group = DbContext.Groups.Find(GroupID);
            var groupmember = (from gm in @group.GroupMembers where gm.UserID == user.ID select gm).FirstOrDefault();
            if (groupmember == null)
                return Json(new Base
                {
                    Code = 808,
                    IsSuccess = false,
                    Info = "您不是该群的成员"
                });
            if(groupmember.Role == Entity.GroupRole.Root)
                return Json(new Base
                {
                    Code = 809,
                    IsSuccess = false,
                    Info = "您是群主，不能退出群"
                });
            DbContext.GroupMembers.Remove(groupmember);
            DbContext.SaveChanges();
            return Json(new Base
            {
                Code = 0,
                IsSuccess = true,
                Info = ""
            });
        }

        [HttpPost]
        public ActionResult GetGroupHomeworks(string Token, int GroupID, int? Page)
        {
            var user = CheckUser(Token);
            if (user == null)
                return Json(new Base
                {
                    Code = 500,
                    IsSuccess = false,
                    Info = "AccessToken不正确"
                });
            var group = DbContext.Groups.Find(GroupID);
            var groupmember = (from gm in @group.GroupMembers where gm.UserID == user.ID select gm).FirstOrDefault();
            if (groupmember == null)
                return Json(new Base
                {
                    Code = 808,
                    IsSuccess = false,
                    Info = "您不是该群的成员"
                });
            if (Page == null) Page = 0;
            var ret = new GroupHomeworks { Code = 0, Info = "", IsSuccess = true, List = new List<GroupHomework>(), PageCount = group.GroupHomeworks.Count / 10 + 1 };
            var homeworks = group.GroupHomeworks.OrderByDescending(x => x.Begin).Skip(Page.Value * 10).Take(10).ToList();
            foreach (var h in homeworks)
            {
                var homework = new GroupHomework 
                { 
                    Begin = h.Begin,
                    End = h.End,
                    Description = h.Description,
                    Title = h.Title,
                    GroupID = h.GroupID,
                    Problems = new List<GroupHomeworkProblem>()
                };
                var problems = h.GroupHomeworkProblems.OrderBy(x => x.Priority);
                foreach (var p in problems)
                {
                    var problem = new GroupHomeworkProblem 
                    { 
                        ProblemID = p.ID,
                        Title = p.Problem.Title,
                        Code = ""
                    };
                    var status = (from s in DbContext.Statuses
                                  where h.Begin <= s.Time
                                  && s.Time < h.End
                                  && s.UserID == user.ID
                                  orderby s.Time descending
                                  select s).FirstOrDefault();
                    if (status == null)
                        problem.Status = "未完成";
                    else
                    {
                        problem.Points = status.JudgeTasks.Where(x => x.Result == Entity.JudgeResult.Accepted).Count() * 100 / status.JudgeTasks.Count;
                        problem.Status = problem + "分";
                        problem.Code = status.Code;
                    }
                    homework.Problems.Add(problem);
                }
                ret.List.Add(homework);
            }
            return Json(ret);
        }

        [HttpPost]
        public ActionResult GetGroupHomeworkStandings(string Token, int GroupHomeworkID, int? Page)
        {
            var user = CheckUser(Token);
            if (user == null)
                return Json(new Base
                {
                    Code = 500,
                    IsSuccess = false,
                    Info = "AccessToken不正确"
                });
            var homework = DbContext.GroupHomeworks.Find(GroupHomeworkID);
            var group = homework.Group;
            var groupmember = (from gm in @group.GroupMembers where gm.UserID == user.ID select gm).FirstOrDefault();
            if (groupmember == null)
                return Json(new GroupHomeworkStandings
                {
                    Code = 808,
                    IsSuccess = false,
                    Info = "您不是该群的成员"
                });
            if (Page == null) Page = 0;
            var groupmembers = (from u in @group.GroupMembers
                                select groupmember.User).ToList();
            var groupmember_ids = (from u in groupmembers
                                   select u.ID).ToList();
            var statuses = (from s in DbContext.Statuses
                            where homework.Begin <= s.Time
                            && s.Time < homework.End
                            && groupmember_ids.Contains(s.UserID)
                            select s).ToList();
            var ret = new GroupHomeworkStandings { Code = 0, IsSuccess = true, Info = "", List = new List<GroupHomeworkStandingsItem>() };
            var problems = homework.GroupHomeworkProblems.OrderBy(x => x.Priority).ToList();
            foreach (var u in groupmembers)
            {
                var s = new GroupHomeworkStandingsItem 
                { 
                    UserID = u.ID,
                    AvatarURL = Helpers.Gravatar.GetAvatarURL(u.Gravatar, 180),
                    GroupID = group.ID,
                    Nickname = u.Nickname,
                    Problems = new List<GroupHomeworkProblem>(),
                    TotalPoints = 0
                };
                foreach (var problem in problems)
                {
                    var p = new GroupHomeworkProblem 
                    { 
                        Code = null,
                        ProblemID = problem.ProblemID,
                        Title = problem.Problem.Title,
                        Status = "未完成",
                        Points = 0
                    };
                    var status = statuses
                        .Where(x => x.UserID == u.ID
                        && x.ProblemID == problem.ProblemID)
                        .OrderByDescending(x => x.Time)
                        .FirstOrDefault();
                    if (status != null)
                    {
                        p.Points = status.JudgeTasks.Where(x => x.Result == Entity.JudgeResult.Accepted).Count() * 100 / status.JudgeTasks.Count;
                        p.Status = problem + "分";
                        if (groupmember.Role >= Entity.GroupRole.Master)
                            p.Code = status.Code;
                    }
                    s.Problems.Add(p);
                }
                s.TotalPoints = s.Problems.Sum(x => x.Points);
                ret.List.Add(s);
            }
            return Json(ret);
        }
    }
}