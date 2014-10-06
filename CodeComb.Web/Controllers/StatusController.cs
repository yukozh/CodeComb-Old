using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CodeComb.Web.Controllers
{
    public class StatusController : BaseController
    {
        //
        // GET: /Status/
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Show(int id)
        {
            var status = DbContext.Statuses.Find(id);
            var contest = status.Problem.Contest;
            var user = ViewBag.CurrentUser == null ? new Entity.User() : (Entity.User)ViewBag.CurrentUser;
            ViewBag.IsMaster = false;
            if (user.Role >= Entity.UserRole.Master || (from cm in status.Problem.Contest.Managers select cm.UserID).Contains(user.ID))
                ViewBag.IsMaster = true;
            bool Showable = false;
            if (DateTime.Now >= contest.End || user.Role >= Entity.UserRole.Master || (from m in contest.Managers select m.ID).ToList().Contains(user.ID))
                Showable = true;
            if (contest.Format == Entity.ContestFormat.TopCoder && DateTime.Now >= contest.RestEnd)
                Showable = true;
            if (contest.Format == Entity.ContestFormat.Codeforces && (from l in status.Problem.Locks where l.UserID == user.ID select l).Count() > 0)
                Showable = true;
            if (status.UserID == user.ID)
                Showable = true;
            if (DateTime.Now >= contest.End && !status.Public)
                Showable = false;
            ViewBag.Showable = Showable;
            return View(status);
        }

        [HttpGet]
        public ActionResult GetStatus(int id)
        {
            var status = DbContext.Statuses.Find(id);
            var contest = status.Problem.Contest;
            var user = ViewBag.CurrentUser==null?new Entity.User():(Entity.User)ViewBag.CurrentUser;
            if (DateTime.Now >= contest.End || user.Role >= Entity.UserRole.Master || (from m in contest.Managers select m.ID).ToList().Contains(user.ID))
            {
                return Json(new Models.View.Status(status), JsonRequestBehavior.AllowGet);
            }
            if (contest.Format == Entity.ContestFormat.ACM)
            {
                status.JudgeTasks = new List<Entity.JudgeTask>
                    { 
                        new Entity.JudgeTask
                        {
                            Result = status.Result,
                            MemoryUsage = status.JudgeTasks.Max(x=>x.MemoryUsage),
                            TimeUsage = status.JudgeTasks.Max(x=>x.TimeUsage),
                            Hint="比赛期间不提供详细信息",
                            StatusID = status.ID
                        }
                    };
            }
            else if (contest.Format == Entity.ContestFormat.OI)
            {
                status.Result = Entity.JudgeResult.Hidden;
                status.JudgeTasks = new List<Entity.JudgeTask>
                    { 
                        new Entity.JudgeTask
                        {
                            Result = Entity.JudgeResult.Hidden,
                            MemoryUsage = status.JudgeTasks.Max(x=>x.MemoryUsage),
                            TimeUsage = status.JudgeTasks.Max(x=>x.TimeUsage),
                            Hint="比赛期间不提供详细信息",
                            StatusID = status.ID
                        }
                    };
            }
            else
            {
                foreach (var jt in status.JudgeTasks)
                    jt.Hint = "比赛期间不提供详细信息";
            }
            return Json(new Models.View.Status(status), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetStatuses(int page, string nickname, int? result,int? contest_id, int? problem_id, int? user_id)
        {
            var _statuses = from s in DbContext.Statuses select s;
            if (!string.IsNullOrEmpty(nickname))
                _statuses = _statuses.Where(x => x.User.Nickname.Contains(nickname));
            if (result != null) 
            {
                _statuses = _statuses.Where(x => x.ResultAsInt == result.Value);
                //if (result == 0)
                //    _statuses = _statuses.Where(x => x.ResultAsInt == 0);
                //else
                //    _statuses = _statuses.Where(x => (from jt in x.JudgeTasks select jt.ResultAsInt).ToList().Contains(result.Value));
            }
            if (contest_id != null)
                _statuses = _statuses.Where(x => x.Problem.ContestID == contest_id && x.Time >= x.Problem.Contest.Begin && x.Time < x.Problem.Contest.End);
            if (problem_id != null)
                _statuses = _statuses.Where(x => x.ProblemID == problem_id);
            if (user_id != null)
                _statuses = _statuses.Where(x => x.UserID == user_id);
            _statuses = _statuses.OrderByDescending(x => x.Time);
            var statuses = new List<Models.View.Status>();
            foreach (var status in _statuses.Skip(10 * page).Take(10).ToList())
            {
                var user = ViewBag.CurrentUser==null?new Entity.User():(Entity.User)ViewBag.CurrentUser;
                var contest = status.Problem.Contest;
                if (DateTime.Now >= contest.End || user.Role >= Entity.UserRole.Master || (from m in contest.Managers select m.ID).ToList().Contains(user.ID))
                {
                    statuses.Add(new Models.View.Status(status));
                    continue;
                }
                if (contest.Format == Entity.ContestFormat.ACM)
                {
                    status.JudgeTasks = new List<Entity.JudgeTask>
                    { 
                        new Entity.JudgeTask
                        {
                            Result = status.Result,
                            MemoryUsage = status.JudgeTasks.Max(x=>x.MemoryUsage),
                            TimeUsage = status.JudgeTasks.Max(x=>x.TimeUsage),
                            Hint="比赛期间不提供详细信息",
                            StatusID = status.ID
                        }
                    };
                }
                else if (contest.Format == Entity.ContestFormat.OI)
                {
                    status.Result = Entity.JudgeResult.Hidden;
                    status.JudgeTasks = new List<Entity.JudgeTask>
                    { 
                        new Entity.JudgeTask
                        {
                            Result = Entity.JudgeResult.Hidden,
                            MemoryUsage = status.JudgeTasks.Max(x=>x.MemoryUsage),
                            TimeUsage = status.JudgeTasks.Max(x=>x.TimeUsage),
                            Hint="比赛期间不提供详细信息",
                            StatusID = status.ID
                        }
                    };
                }
                else
                {
                    foreach (var jt in status.JudgeTasks)
                        jt.Hint = "比赛期间不提供详细信息";
                }
                statuses.Add(new Models.View.Status(status));
            }
            return Json(statuses, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetStatusDetails(int id)
        {
            //TODO: 针对不同权限不同赛制提供有限的内容
            var user = ViewBag.CurrentUser == null ? new Entity.User() : (Entity.User)ViewBag.CurrentUser;
            var contest = DbContext.Statuses.Find(id).Problem.Contest;
            var judgetasks = (from jt in DbContext.JudgeTasks
                               where jt.StatusID == id
                               select jt).ToList();
            var statusdetails = new List<Models.View.StatusDetail>();
            int index = 0;
            foreach (var jt in judgetasks)
                statusdetails.Add(new Models.View.StatusDetail(jt, index++));
            if (DateTime.Now >= contest.End || user.Role >= Entity.UserRole.Master || (from m in contest.Managers select m.ID).ToList().Contains(user.ID))
                return Json(statusdetails, JsonRequestBehavior.AllowGet);
            if (contest.Format == Entity.ContestFormat.ACM || contest.Format == Entity.ContestFormat.OI)
            {
                return Json(new object(), JsonRequestBehavior.AllowGet);
            }
            else
            {
                foreach (var sd in statusdetails)
                    sd.Hint = "比赛期间不提供详细信息显示";
                return Json(statusdetails, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Create(int problem_id, string code, int language_id)
        {
            var problem = DbContext.Problems.Find(problem_id);
            var user = (Entity.User)ViewBag.CurrentUser;
            if (problem == null)
                return Content("Problem not existed");
            var contest = problem.Contest;
            if (!Helpers.PrivateContest.IsUserInPrivateContest(ViewBag.CurrentUser == null ? null : (Entity.User)ViewBag.CurrentUser, contest))
                return Content("Insufficient permissions");
            Entity.ContestFormat[] SubmitAnyTime = { Entity.ContestFormat.ACM, Entity.ContestFormat.OI, Entity.ContestFormat.OPJOI };
            if (DateTime.Now < contest.Begin && user.Role < Entity.UserRole.Master && !(from m in contest.Managers select m.ID).ToList().Contains(user.ID))
            {
                return Content("Insufficient permissions");
            }
            if (contest.Format == Entity.ContestFormat.Codeforces && problem.Locks.Where(x => x.UserID == user.ID).Count() > 0 && DateTime.Now < contest.End)
            {
                return Content("Locked");
            }
            if (contest.Format == Entity.ContestFormat.TopCoder && DateTime.Now >= contest.RestBegin && DateTime.Now < contest.End)
            {
                return Content("Wrong phase");
            }
            var status = new Entity.Status
            {
                Code = code,
                LanguageAsInt = language_id,
                ProblemID = problem_id,
                Public = true,
                UserID = user.ID,
                Time = DateTime.Now,
                Result = Entity.JudgeResult.Pending
            };
            DbContext.Statuses.Add(status);
            List<int> testcase_ids;
            if (DateTime.Now < contest.Begin || DateTime.Now >= contest.End || contest.Format == Entity.ContestFormat.ACM|| contest.Format == Entity.ContestFormat.OPJOI|| contest.Format == Entity.ContestFormat.OI)
            {
                testcase_ids = (from tc in problem.TestCases
                                where tc.Type != Entity.TestCaseType.Sample
                                orderby tc.Type ascending
                                select tc.ID).ToList();
            }
            else if (contest.Format == Entity.ContestFormat.Codeforces)
            {
                testcase_ids = (from tc in problem.TestCases
                                where tc.Type == Entity.TestCaseType.Unilateralism
                                orderby tc.Type ascending
                                select tc.ID).ToList();
                var statuses = problem.GetContestStatuses().Where(x => x.UserID == user.ID).ToList();
                foreach (var s in statuses)
                {
                    if (s.JudgeTasks == null) continue;
                    foreach (var jt in s.JudgeTasks)
                    {
                        testcase_ids.Add(jt.TestCaseID);
                    }
                }
                testcase_ids = testcase_ids.Distinct().ToList();
            }
            else
            {
                if (DateTime.Now < contest.RestBegin)
                {
                    testcase_ids = (from tc in problem.TestCases
                                    where tc.Type == Entity.TestCaseType.Sample
                                    orderby tc.Type ascending
                                    select tc.ID).ToList();
                }
                else
                {
                    testcase_ids = (from tc in problem.TestCases
                                    where tc.Type != Entity.TestCaseType.Sample
                                    orderby tc.Type ascending
                                    select tc.ID).ToList();
                }
            }
            foreach (var id in testcase_ids)
            {
                DbContext.JudgeTasks.Add(new Entity.JudgeTask
                {
                    StatusID = status.ID,
                    TestCaseID = id,
                    Result = Entity.JudgeResult.Pending,
                    MemoryUsage = 0,
                    TimeUsage = 0,
                    Hint = ""
                });
            }
            DbContext.SaveChanges();
            foreach (var jt in status.JudgeTasks)
            {
                try
                {
                    var group = SignalR.JudgeHub.GetNode();
                    if (group == null) return Content("No Online Judger");
                    SignalR.JudgeHub.context.Clients.Group(group).Judge(new Judge.Models.JudgeTask(jt));
                    SignalR.JudgeHub.ThreadBusy(group);
                    jt.Result = Entity.JudgeResult.Running;
                    DbContext.SaveChanges();
                }
                catch { }
            }
            SignalR.CodeCombHub.context.Clients.All.onStatusCreated(new Models.View.Status(status));//推送新状态
            if (contest.Format == Entity.ContestFormat.OI && DateTime.Now >= contest.Begin && DateTime.Now < contest.End)
                return Content("OI");
            return Content(status.ID.ToString());
        }

        public static bool  _Rejudge(int id)
        { 
            Database.DB DbContext = new Database.DB();
            var status = DbContext.Statuses.Find(id);
            var problem = status.Problem;
            var contest = problem.Contest;
            List<int> testcase_ids;
            if (DateTime.Now < contest.Begin || DateTime.Now >= contest.End || contest.Format == Entity.ContestFormat.ACM || contest.Format == Entity.ContestFormat.OPJOI || contest.Format == Entity.ContestFormat.OI)
            {
                testcase_ids = (from tc in problem.TestCases
                                where tc.Type != Entity.TestCaseType.Sample
                                orderby tc.Type ascending
                                select tc.ID).ToList();
            }
            else if (contest.Format == Entity.ContestFormat.Codeforces)
            {
                testcase_ids = (from tc in problem.TestCases
                                where tc.Type == Entity.TestCaseType.Unilateralism
                                orderby tc.Type ascending
                                select tc.ID).ToList();
                var statuses = problem.GetContestStatuses().Where(x => x.UserID == status.UserID).ToList();
                foreach (var s in statuses)
                {
                    foreach (var jt in s.JudgeTasks)
                    {
                        testcase_ids.Add(jt.TestCaseID);
                    }
                }
                testcase_ids = testcase_ids.Distinct().ToList();
            }
            else
            {
                if (DateTime.Now < contest.RestBegin)
                {
                    testcase_ids = (from tc in problem.TestCases
                                    where tc.Type == Entity.TestCaseType.Sample
                                    orderby tc.Type ascending
                                    select tc.ID).ToList();
                }
                else
                {
                    testcase_ids = (from tc in problem.TestCases
                                    where tc.Type != Entity.TestCaseType.Sample
                                    orderby tc.Type ascending
                                    select tc.ID).ToList();
                }
            }
            var existed_ids = (from jt in status.JudgeTasks
                               select jt.TestCaseID).ToList();
            testcase_ids = testcase_ids.Except(existed_ids).ToList();
            foreach (var tid in testcase_ids)
            {
                DbContext.JudgeTasks.Add(new Entity.JudgeTask
                {
                    StatusID = status.ID,
                    TestCaseID = tid,
                    Result = Entity.JudgeResult.Pending,
                    MemoryUsage = 0,
                    TimeUsage = 0,
                    Hint = ""
                });
            }
            DbContext.SaveChanges();
            foreach (var jt in status.JudgeTasks)
            {
                try
                {
                    var group = SignalR.JudgeHub.GetNode();
                    if (group == null) return false;
                    SignalR.JudgeHub.context.Clients.Group(group).Judge(new Judge.Models.JudgeTask(jt));
                    SignalR.JudgeHub.ThreadBusy(group);
                    jt.Result = Entity.JudgeResult.Running;
                    DbContext.SaveChanges();
                }
                catch { }
            }
            SignalR.CodeCombHub.context.Clients.All.onStatusCreated(new Models.View.Status(status));//推送新状态
            return true;
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Rejudge(int id)
        {
            var status = DbContext.Statuses.Find(id);
            var problem = status.Problem;
            var contest = problem.Contest;
            var user = (Entity.User)ViewBag.CurrentUser;
            if (!(user.Role >= Entity.UserRole.Master || (from cm in contest.Managers select cm.UserID).Contains(user.ID)))
            {
                return Content("No");
            }
            if (_Rejudge(id))
                return Content("OK");
            else
                return Content("No Online Judger");
        }

        [HttpGet]
        [Authorize]
        public ActionResult GetCode(int ID)
        {
            var user = (Entity.User)ViewBag.CurrentUser;
            var status = DbContext.Statuses.Find(ID);
            var contest = status.Problem.Contest;
            var problem = status.Problem;
            var ret = new Models.View.HackCode();
            if (contest.Format != Entity.ContestFormat.Codeforces && contest.Format != Entity.ContestFormat.TopCoder)
            {
                ret.Available = false;
                ret.Error = "这场比赛不允许Hack";
                return Json (ret, JsonRequestBehavior.AllowGet);
            }
            if (contest.Format == Entity.ContestFormat.Codeforces)
            {
                var locked = (from l in DbContext.Locks
                              where l.ProblemID == problem.ID
                              && l.UserID == user.ID
                              select l).Count();
                if (locked == 0)
                {
                    ret.Available = false;
                    ret.Error = "您只有锁定该题之后才可进行Hack";
                    return Json(ret, JsonRequestBehavior.AllowGet);
                }
                ret.Available = true;
                ret.Error = "";
                ret.Code = HttpUtility.HtmlEncode(status.Code);
                return Json(ret, JsonRequestBehavior.AllowGet);
            }
            else
            {
                if (DateTime.Now < contest.RestEnd || DateTime.Now > contest.End)
                {
                    ret.Available = false;
                    ret.Error = "现在不允许Hack";
                    return Json(ret, JsonRequestBehavior.AllowGet);
                }
                ret.Available = true;
                ret.Error = "";
                ret.Code = status.Code;
                return Json(ret, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [Authorize]
        [ValidateInput(false)]
        public ActionResult Hack(int id, string data, string data_maker, int? data_maker_language)
        {
            var Available = false;
            #region 判断是否允许hack
            var user = (Entity.User)ViewBag.CurrentUser;
            var status = DbContext.Statuses.Find(id);
            var contest = status.Problem.Contest;
            if (!Helpers.PrivateContest.IsUserInPrivateContest(ViewBag.CurrentUser == null ? null : (Entity.User)ViewBag.CurrentUser, contest))
                return RedirectToAction("Private", "Contest", new { id = id });
            var problem = status.Problem;
            var ret = new Models.View.HackCode();
            if (contest.Format != Entity.ContestFormat.Codeforces && contest.Format != Entity.ContestFormat.TopCoder)
            {
                Available = false;
            }
            if (contest.Format == Entity.ContestFormat.Codeforces)
            {
                var locked = (from l in DbContext.Locks
                              where l.ProblemID == problem.ID
                              && l.UserID == user.ID
                              select l).Count();
                if (locked == 0)
                {
                    Available = false;
                }
                else
                {
                    Available = true;
                }
            }
            else
            {
                if (DateTime.Now < contest.RestEnd || DateTime.Now > contest.End)
                {
                    Available = false;
                }
                else { 
                Available = true;
                }
            }
            #endregion
            if (Available)
            {
                var hack = new Entity.Hack 
                { 
                    HackerID = ViewBag.CurrentUser.ID,
                    DefenderID = status.User.ID,
                    Result = Entity.HackResult.Pending,
                    StatusID = status.ID,
                    DataMakerCode = "",
                    DataMakerLanguage = Entity.Language.C,
                    Time = DateTime.Now,
                    Hint = ""
                };
                if (data_maker_language == null)
                {
                    hack.InputData = data;
                }
                else
                {
                    hack.DataMakerCode = data_maker;
                    hack.DataMakerLanguageAsInt = data_maker_language.Value;
                }
                DbContext.Hacks.Add(hack);
                DbContext.SaveChanges();
                try
                {
                    var group = SignalR.JudgeHub.GetNode();
                    if (group == null) return Content("err");
                    SignalR.JudgeHub.context.Clients.Group(group).Hack(new Judge.Models.HackTask(hack));
                    SignalR.JudgeHub.ThreadBusy(group);
                    hack.Result = Entity.HackResult.Running;
                    DbContext.SaveChanges();
                }
                catch { }
                return Content(hack.ID.ToString());
            }
            else
            {
                return Content("err");
            }
        }

        public static bool _Rehack(int id)
        {
            Database.DB DbContext = new Database.DB();
            var hack = DbContext.Hacks.Find(id);
            var status = hack.Status;
            var problem = status.Problem;
            var contest = problem.Contest;
            if (status.Result == Entity.JudgeResult.Hacked)
            {
                DbContext.TestCases.Remove(status.JudgeTasks.Last().TestCase);
                DbContext.SaveChanges();
                status.Result = status.JudgeTasks.Max(x => x.Result);
                DbContext.SaveChanges();
            }
            var group = SignalR.JudgeHub.GetNode();
            hack.Result = Entity.HackResult.Pending;
            DbContext.SaveChanges();
            if (group == null) return false;
            SignalR.JudgeHub.context.Clients.Group(group).Hack(new Judge.Models.HackTask(hack));
            SignalR.JudgeHub.ThreadBusy(group);
            hack.Result = Entity.HackResult.Running;
            DbContext.SaveChanges();
            return true;
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Rehack(int id)
        {
            var hack = DbContext.Hacks.Find(id);
            var status = hack.Status;
            var problem = status.Problem;
            var contest = problem.Contest;
            var user = (Entity.User)ViewBag.CurrentUser;
            if (!(user.Role >= Entity.UserRole.Master || (from cm in contest.Managers select cm.UserID).Contains(user.ID)))
            {
                return Content("No");
            }
            if (_Rehack(id))
                return Content("OK");
            else 
                return Content("Err");
        }
	}
}