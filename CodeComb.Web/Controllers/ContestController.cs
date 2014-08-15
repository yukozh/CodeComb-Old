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
            foreach (var problem in contest.Problems)
                lstProblems.Add(new Models.View.ProblemInContest(problem, user));
            ViewBag.Problems = lstProblems;
            return View(contest);
        }
	}
}