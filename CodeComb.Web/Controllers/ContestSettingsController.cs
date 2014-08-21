using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CodeComb.Web.Controllers
{
    public class ContestSettingsController : ContestBaseController
    {
        //
        // GET: /ContestSettings/
        [Authorize]
        public ActionResult General(int id)//
        {
            if (!ViewBag.IsMaster)
                return RedirectToAction("Message", "Shared", new { msg = "您没有权限对本场比赛进行任何修改！" });
            ViewBag.ContestFormats = Enum.GetNames(typeof(Entity.ContestFormat));
            var contest = DbContext.Contests.Find(id);
            return View(contest);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        [ValidateInput(false)]
        public ActionResult General(int id, Entity.Contest model)
        {
            if (!ViewBag.IsMaster)
                return RedirectToAction("Message", "Shared", new { msg = "您没有权限对本场比赛进行任何修改！" });
            var contest = DbContext.Contests.Find(id);
            contest.Begin = model.Begin;
            contest.RestBegin = model.RestBegin;
            contest.RestEnd = model.RestEnd;
            contest.End = model.End;
            contest.Title = model.Title;
            contest.Password = model.Password;
            contest.Content = model.Content;
            contest.Format = model.Format;
            contest.RatingBegin = model.RatingBegin;
            contest.RatingEnd = model.RatingEnd;
            contest.AllowClarification = model.AllowClarification;
            contest.AllowPrintRequest = model.AllowPrintRequest;
            if (model.RatingBegin > model.RatingEnd)
            {
                return RedirectToAction("Message", "Shared", new { msg = "能力值不合法" });
            }
            if (((Entity.User)ViewBag.CurrentUser).Role >= Entity.UserRole.Master)
            {
                contest.Ready = model.Ready;//这里只有系统管理员才可以设置这个选项，比赛创建者、管理员均无权
            }
            if (contest.Format == Entity.ContestFormat.TopCoder)
            {
                if (!(contest.Begin < contest.RestBegin && contest.RestBegin <= contest.RestEnd && contest.RestEnd < contest.End))
                {
                    return RedirectToAction("Message", "Shared", new { msg = "比赛时间或休息时间不合法" });
                }
            }
            else
            {
                if (!(contest.Begin < contest.End))
                {
                    return RedirectToAction("Message", "Shared", new { msg = "比赛时间设定不合法" });
                }
                contest.RestBegin = null;
                contest.RestEnd = null;
            }
            if (string.IsNullOrEmpty(contest.Password))
                contest.Password = null;
            DbContext.SaveChanges();
            return RedirectToAction("General", "ContestSettings", new { id = id });
        }

        [Authorize]
        public ActionResult Problems(int id)
        {
            if (!ViewBag.IsMaster)
                return RedirectToAction("Message", "Shared", new { msg = "您没有权限对本场比赛进行任何修改！" });
            ViewBag.ContestFormats = Enum.GetNames(typeof(Entity.ContestFormat));
            var contest = DbContext.Contests.Find(id);
            return View(contest);
        }

        [Authorize]
        public ActionResult Manager(int id)
        {
            if (!ViewBag.IsCreator)
                return RedirectToAction("Message", "Shared", new { msg = "您无权执行本操作！" });
            ViewBag.ContestFormats = Enum.GetNames(typeof(Entity.ContestFormat));
            var contest = DbContext.Contests.Find(id);
            return View(contest);
        }

        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult AddManager(int id, int user_id)
        {
            if (!ViewBag.IsCreator)
                return RedirectToAction("Message", "Shared", new { msg = "您无权执行本操作！" });
            ViewBag.ContestFormats = Enum.GetNames(typeof(Entity.ContestFormat));
            DbContext.ContestManagers.Add(new Entity.ContestManager 
            { 
                ContestID = id,
                IsCreator = false,
                UserID = user_id
            });
            DbContext.SaveChanges();
            return RedirectToAction("Manager", "ContestSettings", new { id = id });
        }

        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult DeleteManager(int id, int manager_id)
        {
            if (!ViewBag.IsCreator)
                return RedirectToAction("Message", "Shared", new { msg = "您无权执行本操作！" });
            ViewBag.ContestFormats = Enum.GetNames(typeof(Entity.ContestFormat));
            var manager = DbContext.ContestManagers.Find(manager_id);
            if (manager.IsCreator)
                return RedirectToAction("Message", "Shared", new { msg = "您无权执行本操作！" });
            DbContext.ContestManagers.Remove(manager);
            DbContext.SaveChanges();
            return RedirectToAction("Manager", "ContestSettings", new { id = id });
        }
	}
}