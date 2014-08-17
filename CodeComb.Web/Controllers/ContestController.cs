using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CodeComb.Web.Controllers
{
    public class ContestController : ContestBaseController
    {
        //
        // GET: /Contest/
        public ActionResult Index()
        {
            var time = DateTime.Now.AddDays(7);
            var contests = (from c in DbContext.Contests
                            where DateTime.Now < c.End
                            && c.Ready
                            && c.Password == null
                            && c.Begin <= time
                            orderby c.Begin ascending
                            select c).ToList();
            return View(contests);
        }

        [HttpGet]
        public ActionResult GetContests(int page, int? format)
        {
            var _contests = (from c in DbContext.Contests
                            where DateTime.Now >= c.End
                            select c);
            if (format != null)
                _contests = _contests.Where(x => x.FormatAsInt == format);
            _contests = _contests.OrderByDescending(x => x.End);
            var contests = new List<Models.View.Contest>();
            foreach (var contest in _contests.Skip(10 * page).Take(10).ToList())
                contests.Add(new Models.View.Contest(contest));
            return Json(contests, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Show(int id)
        {
            var contest = DbContext.Contests.Find(id);
            ViewBag.UserCount = (from s in DbContext.Statuses
                         let pid = (from p in DbContext.Problems
                                    where p.ContestID == contest.ID
                                    select p.ID).ToList()
                         where pid.Contains(s.ProblemID)
                         && s.Time >= contest.Begin
                         && s.Time < contest.End
                         select s.UserID).Distinct().Count();
            if (DateTime.Now < contest.Begin)
                ViewBag.InfoCss = "pending";
            else if (DateTime.Now <= contest.End)
                ViewBag.InfoCss = "inprogress";
            else
                ViewBag.InfoCss = "";
            var lstProblems = new List<Models.View.ProblemInContest>();
            Entity.User user = null;
            if(User.Identity.IsAuthenticated)
                user = (from u in DbContext.Users where u.Username == User.Identity.Name select u).Single();
            foreach (var problem in contest.Problems.OrderBy(x => x.Credit))
                lstProblems.Add(new Models.View.ProblemInContest(problem, user));
            ViewBag.Problems = lstProblems;
            return View(contest);
        }

        public ActionResult Statistics(int id)
        {
            var contest = DbContext.Contests.Find(id);
            return View(contest);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Print(int id, string content)
        {
            var contest = DbContext.Contests.Find(id);
            if (!contest.AllowPrintRequest)
                return RedirectToAction("Message", "Shared", new { msg = "这场比赛没有开放打印服务！" });
            if(DateTime.Now < contest.Begin || DateTime.Now >= contest.End)
                return RedirectToAction("Message", "Shared", new { msg = "只有在比赛进行时才可以使用打印服务！" });
            DbContext.PrintRequests.Add(new Entity.PrintRequest 
            {
                PrintFinished = false,
                Content = content,
                Time = DateTime.Now,
                ContestID = id,
                UserID = ViewBag.CurrentUser.ID
            });
            DbContext.SaveChanges();
            return RedirectToAction("Message", "Shared", new { msg = "打印材料提交成功，您当前的队列位置为：" + contest.PrintRequests.Where(x => !x.PrintFinished).Count() });
        }

        [Authorize]
        public ActionResult Print(int id)
        {
            var contest = DbContext.Contests.Find(id);
            if (!contest.AllowPrintRequest)
                return RedirectToAction("Message", "Shared", new { msg = "这场比赛没有开放打印服务！" });
            if (DateTime.Now < contest.Begin || DateTime.Now >= contest.End)
                return RedirectToAction("Message", "Shared", new { msg = "只有在比赛进行时才可以使用打印服务！" });
            return View(contest);
        }

        [HttpGet]
        public ActionResult GetClar(int id)
        {
            var uid = 0;
            if (User.Identity.IsAuthenticated)
                uid = (int)ViewBag.CurrentUser.ID;
            var clar = DbContext.Clarifications.Find(id);
            if (clar.Status != Entity.ClarificationStatus.BroadCast && clar.UserID != uid)
                return Json(null, JsonRequestBehavior.AllowGet);
            return Json(new Models.View.Clar(clar), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Clar(int id)
        {
            var clarifications = (from c in DbContext.Clarifications
                                  where c.ContestID == id
                                  && c.StatusAsInt == (int)Entity.ClarificationStatus.BroadCast
                                  select c).ToList();
            ViewBag.CurrentContest = DbContext.Contests.Find(id);
            if (User.Identity.IsAuthenticated)
            {
                var _problems = (from p in DbContext.Problems
                                 where p.ContestID == id
                                 orderby p.Credit descending
                                 select p).ToList();
                var problems = new List<Models.View.ClarProblem>();
                foreach (var p in _problems)
                    problems.Add(new Models.View.ClarProblem(p));
                ViewBag.Problems = problems;
                var uid = (int)ViewBag.CurrentUser.ID;
                clarifications = clarifications.Union((from c in DbContext.Clarifications
                                                       where c.ContestID == id
                                                       && c.UserID == uid
                                                       select c).ToList()).ToList();
            }
            clarifications = clarifications.OrderByDescending(x => x.Time).ToList();
            return View(clarifications);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        [Authorize]
        public ActionResult Clar(int id,int? problem_id, string question)
        {
            //合法性验证
            var contest = DbContext.Contests.Find(id);
            if (!contest.AllowClarification)
                return RedirectToAction("Message", "Shared", new { msg = "本场比赛没有开启答疑平台！" });
            var problem = DbContext.Problems.Find(problem_id);
            if (problem != null && problem.ContestID != contest.ID)
                return RedirectToAction("Message", "Shared", new { msg = "没有找到该题！" });

            var clar = new Entity.Clarification
            {
                ContestID = id,
                ProblemID = problem_id,
                Status = Entity.ClarificationStatus.Pending,
                Question = question,
                Answer = "",
                Time = DateTime.Now,
                UserID = ViewBag.CurrentUser.ID
            };
            DbContext.Clarifications.Add(clar);
            DbContext.SaveChanges();
            var masters = (from u in DbContext.Users
                           let master_ids = (from m in DbContext.ContestManagers
                                             where m.ContestID == contest.ID
                                             select id).ToList()
                           where u.RoleAsInt >= (int)Entity.UserRole.Master
                           || master_ids.Contains(u.ID)
                           select u.Username).ToList();
            foreach (var master in masters)
                SignalR.CodeCombHub.context.Clients.Group(master).onClarificationsRequested(clar.ID);

            //重新载入Model
            var clarifications = (from c in DbContext.Clarifications
                                  where c.ContestID == id
                                  && c.StatusAsInt == (int)Entity.ClarificationStatus.BroadCast
                                  select c).ToList();
            ViewBag.CurrentContest = DbContext.Contests.Find(id);
            if (User.Identity.IsAuthenticated)
            {
                var _problems = (from p in DbContext.Problems
                                    where p.ContestID == id
                                    orderby p.Credit descending
                                    select p).ToList();
                var problems = new List<Models.View.ClarProblem>();
                foreach (var p in _problems) 
                    problems.Add(new Models.View.ClarProblem(p));
                ViewBag.Problems = problems;
                var uid = (int)ViewBag.CurrentUser.ID;
                clarifications = clarifications.Union((from c in DbContext.Clarifications
                                                       where c.ContestID == id
                                                       && c.UserID == uid
                                                       select c).ToList()).ToList();
            }
            clarifications = clarifications.OrderByDescending(x => x.Time).ToList();
            return View(clarifications);
        }

        [HttpPost]
        [ValidateInput(false)]
        [Authorize]
        public ActionResult ResponseClar(int id, string answer, bool broadcast)
        {
            if (!ViewBag.IsMaster)
            {
                return Content("No");
            }
            var clar = DbContext.Clarifications.Find(id);
            if(broadcast)
                clar.Status = Entity.ClarificationStatus.BroadCast;
            else 
                clar.Status = Entity.ClarificationStatus.Private;
            clar.Answer = answer;
            DbContext.SaveChanges();
            if (broadcast)
                SignalR.CodeCombHub.context.Clients.All.onClarificationsResponsed(new Models.View.Clar(clar));
            else
                SignalR.CodeCombHub.context.Clients.Group(clar.User.Username).onClarificationsResponsed(new Models.View.Clar(clar));
            return Content("Yes");
        }
	}
}