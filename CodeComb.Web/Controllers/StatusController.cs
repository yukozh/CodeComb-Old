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
            var judgetasks = (from jt in DbContext.JudgeTasks
                               where jt.StatusID == id
                               select jt).ToList();
            var statusdetails = new List<Models.View.StatusDetail>();
            int index = 0;
            foreach (var jt in judgetasks)
                statusdetails.Add(new Models.View.StatusDetail(jt, index++));
            return Json(statusdetails, JsonRequestBehavior.AllowGet);
        }
	}
}