using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CodeComb.Web.Controllers
{
    public class ProblemController : BaseController
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
            var Problems = new List<Models.View.StatusSnapshot>();
            foreach (var p in problem.Contest.Problems.OrderBy(x=>x.Credit))
                Problems.Add(new Models.View.StatusSnapshot(p.Contest, ViewBag.CurrentUser));
            ViewBag.Problems = Problems;
            return View(problem);
        }
	}   
}