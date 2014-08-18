using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CodeComb.Entity;
using CodeComb.Database;

namespace CodeComb.Web.Controllers
{
    public class SolutionController : BaseController
    {
        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            base.Initialize(requestContext);
            ViewBag.IsMaster = false;
            ViewBag.IsCreator = false;
            ViewBag.IsAccepted = false;
            if (!User.Identity.IsAuthenticated) return;
            var id = requestContext.RouteData.Values["id"] == null ? null : (int?)Convert.ToInt32(requestContext.RouteData.Values["id"]);
            if (id == null) return;
            var problem = DbContext.Problems.Find(id);
            var contest = problem.Contest;
            var user = (from u in DbContext.Users where u.Username == User.Identity.Name select u).Single();
            if (problem.Statuses.Where(x => x.UserID == user.ID && x.ResultAsInt == (int)Entity.JudgeResult.Accepted).Count() > 0)
                ViewBag.IsAccepted = true;
            if (user.Role >= UserRole.Master)
            {
                ViewBag.IsMaster = true;
                ViewBag.IsCreator = true;
                ViewBag.IsAccepted = true;
            }
            else
            {
                var contestmanager = (from cm in contest.Managers where cm.UserID == user.ID select cm).SingleOrDefault();
                if (contestmanager != null)
                {
                    ViewBag.IsMaster = true;
                    ViewBag.IsAccepted = true;
                    if (contestmanager.IsCreator)
                        ViewBag.IsCreator = true;
                }
            }
        }

        //
        // GET: /Solution/
        public ActionResult Index(int id)
        {
            var problem = DbContext.Problems.Find(id);
            var user = ViewBag.CurrentUser == null ? new Entity.User() : (Entity.User)ViewBag.CurrentUser;

            if (DateTime.Now < problem.Contest.End && user.Role < Entity.UserRole.Master && !(from m in problem.Contest.Managers select m.ID).Contains(user.ID))
                return RedirectToAction("Message", "Shared", new { msg = "目前还不能使用结题报告系统！" });
            var solutions = (from s in DbContext.Solutions
                             where s.ProblemID == id
                             orderby s.ID descending
                             select s).ToList();
            return View();
        }

        public ActionResult Show(int id)
        {
            var solution = DbContext.Solutions.Find(id);
            var problem = solution.Problem;
            var user = ViewBag.CurrentUser == null ? new Entity.User() : (Entity.User)ViewBag.CurrentUser;
            if (DateTime.Now < problem.Contest.End && user.Role < Entity.UserRole.Master && !(from m in problem.Contest.Managers select m.ID).Contains(user.ID))
                return RedirectToAction("Message", "Shared", new { msg = "目前还不能使用结题报告系统！" });
            var tags = "";
            var i = 0;
            foreach (var tag in solution.SolutionTags)
            {
                if (++i == solution.SolutionTags.Count)
                {
                    tags += tag.AlgorithmTag.Title;
                }
                else
                {
                    tags += (tag.AlgorithmTag.Title + ",");
                }
            }
            ViewBag.Tags = tags;
            return View(solution);
        }

        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Delete(int id)
        {
            var solution = DbContext.Solutions.Find(id);
            var problem = solution.Problem;
            var user = ViewBag.CurrentUser == null ? new Entity.User() : (Entity.User)ViewBag.CurrentUser;
            if (DateTime.Now < problem.Contest.End && user.Role < Entity.UserRole.Master && !(from m in problem.Contest.Managers select m.ID).Contains(user.ID) && solution.UserID != user.ID)
                return RedirectToAction("Message", "Shared", new { msg = "您无权删除这份解题报告！" });
            DbContext.Solutions.Remove(solution);
            DbContext.SaveChanges();
            return RedirectToAction("Index", "Solution", new { id = problem.ID });
        }
    }
}