using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CodeComb.Entity;
using CodeComb.Database;

namespace CodeComb.Web.Controllers
{
    public class ProblemController : BaseController
    {
        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            base.Initialize(requestContext);
            ViewBag.IsMaster = false;
            ViewBag.IsCreator = false;
            if (!User.Identity.IsAuthenticated) return;
            var id = requestContext.RouteData.Values["id"] == null ? null : (int?)Convert.ToInt32(requestContext.RouteData.Values["id"]);
            if (id == null) return;
            var contest = DbContext.Problems.Find(id).Contest;
            var user = (from u in DbContext.Users where u.Username == User.Identity.Name select u).Single();
            if (user.Role >= UserRole.Master)
            {
                ViewBag.IsMaster = true;
                ViewBag.IsCreator = true;
            }
            else
            {
                var contestmanager = (from cm in contest.Managers where cm.UserID == user.ID select cm).SingleOrDefault();
                if (contestmanager != null)
                {
                    ViewBag.IsMaster = true;
                    if (contestmanager.IsCreator)
                        ViewBag.IsCreator = true;
                }
            }
        }
        //
        // GET: /Problem/
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Show(int id)
        {
            var problem = DbContext.Problems.Find(id);
            var contest = problem.Contest;
            var user = ViewBag.CurrentUser == null ? new Entity.User() : (Entity.User)ViewBag.CurrentUser;
            if (DateTime.Now < contest.Begin && user.Role < Entity.UserRole.Master && (from m in contest.Managers select m.ID).ToList().Contains(user.ID))
                return RedirectToAction("Message", "Shared", new { msg = "您无权查看该题目！" });
            var Problems = new List<Models.View.StatusSnapshot>();
            foreach (var p in problem.Contest.Problems.OrderBy(x=>x.Credit))
                Problems.Add(new Models.View.StatusSnapshot(p.Contest, ViewBag.CurrentUser));
            ViewBag.Problems = Problems;
            var statuses = problem.GetContestStatuses().Where(x => x.User.Username == User.Identity.Name).OrderByDescending(x => x.Time).ToList();
            if (contest.Format == ContestFormat.OI)
            {
                foreach (var s in statuses)
                {
                    s.Result = JudgeResult.Hidden;
                }
            }
            ViewBag.Statuses = statuses;
            return View(problem);
        }
        
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Create(int id)
        {
            var contest = DbContext.Contests.Find(id);
            var user = ViewBag.CurrentUser == null ? new Entity.User() : (Entity.User)ViewBag.CurrentUser;
            if (DateTime.Now < contest.Begin && user.Role < Entity.UserRole.Master && (from m in contest.Managers select m.ID).ToList().Contains(user.ID))
                return RedirectToAction("Message", "Shared", new { msg = "您无权执行本操作！" });
            DbContext.Problems.Add(new Problem 
            { 
                Difficulty = 0,
                Background = null,
                Input = null,
                Output =null,
                Description = null,
                SpecialJudge="",
                SpecialJudgeLanguage = Language.C,
                StandardSource = "",
                StandardSourceLanguage = Language.C,
                RangeChecker = "",
                RangeCheckerLanguage = Language.C,
                MemoryLimit = 65536,
                TimeLimit = 1000,
                Title = "新建题目",
                Hint = null,
                ContestID = id,
                Credit = 0
            });
            DbContext.SaveChanges();
            return RedirectToAction("Problems", "ContestSettings", new { id = id });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var problem = DbContext.Problems.Find(id);
            var contest = problem.Contest;
            var user = ViewBag.CurrentUser == null ? new Entity.User() : (Entity.User)ViewBag.CurrentUser;
            if (DateTime.Now < contest.Begin && user.Role < Entity.UserRole.Master && (from m in contest.Managers select m.ID).ToList().Contains(user.ID))
                return RedirectToAction("Message", "Shared", new { msg = "您无权执行本操作！" });
            DbContext.Problems.Remove(problem);
            DbContext.SaveChanges();
            return RedirectToAction("Problems", "ContestSettings", new { id = id });
        }
	}   
}