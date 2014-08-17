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
            return View(status);
        }

        [HttpGet]
        public ActionResult GetStatuses(int page, string nickname, int? result,int? contest_id, int? problem_id)
        {
            var _statuses = from s in DbContext.Statuses select s;
            if (!string.IsNullOrEmpty(nickname))
                _statuses = _statuses.Where(x => x.User.Nickname.Contains(nickname));
            if (result != null) 
            {
                if (result == 0)
                    _statuses = _statuses.Where(x => x.ResultAsInt == 0);
                else
                    _statuses = _statuses.Where(x => (from jt in x.JudgeTasks select jt.ResultAsInt).ToList().Contains(result.Value));
            }
            if (contest_id != null)
                _statuses = _statuses.Where(x => x.Problem.ContestID == contest_id && x.Time >= x.Problem.Contest.Begin && x.Time < x.Problem.Contest.End);
            if (problem_id != null)
                _statuses = _statuses.Where(x => x.ProblemID == problem_id);
            _statuses = _statuses.OrderByDescending(x => x.Time);
            var statuses = new List<Models.View.Status>();
            foreach (var status in _statuses.Skip(10 * page).Take(10).ToList())
                statuses.Add(new Models.View.Status(status));
            return Json(statuses, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetStatus(int id)
        {
            var status = DbContext.Statuses.Find(id);
            return Json(new Models.View.Status(status), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetStatusDetails(int id)
        {
            //TODO: 针对不同权限不同赛制提供有限的内容
            var judgetasks = (from jt in DbContext.JudgeTasks
                               where jt.StatusID == id
                               select jt).ToList();
            var statusdetails = new List<Models.View.StatusDetail>();
            int index = 0;
            foreach (var jt in judgetasks)
                statusdetails.Add(new Models.View.StatusDetail(jt, index++));
            return Json(statusdetails, JsonRequestBehavior.AllowGet);
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
            var testcase_ids = (from tc in problem.TestCases
                                where tc.Type == Entity.TestCaseType.Overall
                                select tc.ID).ToList();
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
            SignalR.CodeCombHub.context.Clients.All.onStatusCreated(new Models.View.Status(status));//推送新状态
            return Content(status.ID.ToString());
        }
	}
}