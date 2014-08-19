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

        [Authorize]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        [HttpPost]
        public ActionResult Edit(int id, Solution model)
        {
            var solution = DbContext.Solutions.Find(id);
            if (!ViewBag.IsMaster && solution.UserID != ViewBag.CurrentUser.ID)
                return RedirectToAction("Message", "Shared", new { msg="权限不足！"});
            solution.Language = model.Language;
            solution.Title = model.Title;
            solution.Code = model.Code;
            solution.Content = model.Content;
            DbContext.SaveChanges();
            return RedirectToAction("EditTags", "Solution", new { id = solution.ID });
        }

        [Authorize]
        public ActionResult Edit(int id)
        {
            var solution = DbContext.Solutions.Find(id);
            if (!ViewBag.IsMaster && solution.UserID != ViewBag.CurrentUser.ID)
                return RedirectToAction("Message", "Shared", new { msg = "权限不足！" });
            return View(solution);
        }

        [Authorize]
        public ActionResult EditTags(int id)
        {
            var solution = DbContext.Solutions.Find(id);
            if (!ViewBag.IsMaster && solution.UserID != ViewBag.CurrentUser.ID)
                return RedirectToAction("Message", "Shared", new { msg = "权限不足！" });
            ViewBag.Tags = (from at in DbContext.AlgorithmTags
                            where at.FatherID == null
                            orderby at.ID ascending
                            select at).ToList();
            return View(solution);
        }

        [HttpGet]
        [Authorize]
        public ActionResult GetTags(int id)
        {
            var solution = DbContext.Solutions.Find(id);
            if (!ViewBag.IsMaster && solution.UserID != ViewBag.CurrentUser.ID)
                return Json(null, JsonRequestBehavior.AllowGet);
            var tags = (from t in solution.SolutionTags
                        select t.AlgorithmTagID).ToList();
            return Json(tags, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Authorize]
        public ActionResult SetTag(int id, int tid)
        {
            var solution = DbContext.Solutions.Find(id);
            if (!ViewBag.IsMaster && solution.UserID != ViewBag.CurrentUser.ID)
                return Content("Failed");
            var tag = solution.SolutionTags.Where(x => x.AlgorithmTagID == tid).SingleOrDefault();
            if (tag == null)
            {
                tag = new SolutionTag
                {
                    AlgorithmTagID = tid,
                    SolutionID = id
                };
                DbContext.SolutionTags.Add(tag);
                DbContext.SaveChanges();
                return Content("Added");
            }
            else
            {
                DbContext.SolutionTags.Remove(tag);
                DbContext.SaveChanges();
                return Content("Deleted");
            }
        }

        [Authorize]
        public ActionResult Create(int id)
        {
            var problem = DbContext.Problems.Find(id);
            var user = ViewBag.CurrentUser == null ? new Entity.User() : (Entity.User)ViewBag.CurrentUser;
            if (DateTime.Now < problem.Contest.End && user.Role < Entity.UserRole.Master && !(from m in problem.Contest.Managers select m.ID).Contains(user.ID))
                return RedirectToAction("Message", "Shared", new { msg = "现在不允许编写解题报告！" });
            if (problem.Statuses.Where(x => x.ResultAsInt == (int)Entity.JudgeResult.Accepted && x.UserID == user.ID).Count() == 0)
                return RedirectToAction("Message", "Shared", new { msg = "只有通过了本题才可以编写解题报告！" });
            return View();
        }

        [Authorize]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        [HttpPost]
        public ActionResult Create(int id, Solution model)
        {
            var problem = DbContext.Problems.Find(id);
            var user = ViewBag.CurrentUser == null ? new Entity.User() : (Entity.User)ViewBag.CurrentUser;
            if (DateTime.Now < problem.Contest.End && user.Role < Entity.UserRole.Master && !(from m in problem.Contest.Managers select m.ID).Contains(user.ID))
                return RedirectToAction("Message", "Shared", new { msg = "现在不允许编写解题报告！" });
            if (problem.Statuses.Where(x => x.ResultAsInt == (int)Entity.JudgeResult.Accepted && x.UserID == user.ID).Count() == 0)
                return RedirectToAction("Message", "Shared", new { msg = "只有通过了本题才可以编写解题报告！" });
            var solution = new Solution 
            { 
                Content = model.Content,
                Language = model.Language,
                Title = model.Title,
                Code = model.Code,
                UserID = user.ID,
                ProblemID = id
            };
            DbContext.Solutions.Add(solution);
            DbContext.SaveChanges();
            return RedirectToAction("EditTags", "Solution", new { id = solution.ID });
        }
    }
}