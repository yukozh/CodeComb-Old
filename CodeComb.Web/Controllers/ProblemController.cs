using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CodeComb.Web.Controllers
{
    public class ProblemController : ContestBaseController
    {
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
            return View(problem);
        }
	}   
}