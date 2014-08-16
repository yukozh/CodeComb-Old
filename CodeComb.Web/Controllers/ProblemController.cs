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
            return View(problem);
        }
	}   
}