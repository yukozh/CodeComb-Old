using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CodeComb.Web.Controllers
{
    public class HackController : BaseController
    {
        //
        // GET: /Hack/
        public ActionResult Index(int id)
        {
            var hack = DbContext.Hacks.Find(id);
            var contest = hack.Status.Problem.Contest;
            if (DateTime.Now >= contest.End) return View(hack);
            if (!User.Identity.IsAuthenticated) RedirectToAction("Message", "Shared", new { msg = "您没有权限执行本操作！" });
            var user = (Entity.User)ViewBag.CurrentUser;
            if (user.Role < Entity.UserRole.Master && !(from cm in contest.Managers select cm.UserID).Contains(user.ID)) return RedirectToAction("Message", "Shared", new { msg="您没有权限执行本操作！"});
            return View(hack);
        }
        public ActionResult GetHacks(int page, string hacker, string defender, int? result, int contest_id, int? problem_id, int? user_id)
        {
            IEnumerable<Entity.Hack> hacks = (from h in DbContext.Hacks
                             where h.Status.Problem.ContestID == contest_id
                             select h);
            if (!string.IsNullOrEmpty(hacker))
                hacks.Where(x=>x.Hacker.Nickname.Contains(hacker));
            if (!string.IsNullOrEmpty(defender))
                hacks.Where(x => x.Defender.Nickname.Contains(defender));
            if (problem_id != null)
                hacks.Where(x => x.Status.ProblemID == problem_id);
            if (user_id != null)
                hacks.Where(x => x.HackerID == user_id);
            hacks = hacks.OrderByDescending(x => x.Time).Skip(page * 10).Take(10).ToList();
            List<Models.View.Hack> ret = new List<Models.View.Hack>();
            foreach (var hack in hacks)
            {
                ret.Add(new Models.View.Hack(hack));
            }
            return Json(ret, JsonRequestBehavior.AllowGet);
        }
	}
}