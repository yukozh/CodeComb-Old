using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CodeComb.Entity;
using CodeComb.Web.Models.View;

namespace CodeComb.Web.Controllers
{
    public class TopicController : BaseController
    {
        //
        // GET: /Topic/
        public ActionResult Index(int id)
        {
            var topic = DbContext.Topics.Find(id);
            return View(topic);
        }

        [HttpGet]
        public ActionResult GetTopics(int page, int? id)
        {
            List<CodeComb.Entity.Topic> _topics;
            if (id == null)
            {
                _topics = (from t in DbContext.Topics
                          orderby t.LastReply descending
                          select t).Skip(page * 10).Take(10).ToList();
            }
            else
            {
                _topics = (from t in DbContext.Topics
                          where t.ForumID == id
                          orderby t.LastReply descending
                          select t).Skip(page * 10).Take(10).ToList();
            }
            List<Models.View.Topic> topics = new List<Models.View.Topic>();
            foreach (var topic in _topics)
                topics.Add(new Models.View.Topic(topic));
            return Json(topics, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public ActionResult Create(int id)
        {
            if ((from f in DbContext.Forums where f.ID == id && f.FatherID != null select f).Count() == 0)
                return RedirectToAction("Message", "Shared", new { msg = "没有找到这个论坛版块！" });
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Post model)
        {
            if ((from f in DbContext.Forums where f.ID == model.ForumID && f.FatherID != null select f).Count() == 0)
                return RedirectToAction("Message", "Shared", new { msg = "没有找到这个论坛版块！" });
            if(string.IsNullOrEmpty(model.Content))
                return RedirectToAction("Message", "Shared", new { msg = "内容不能为空！" });
            var topic = new CodeComb.Entity.Topic
            {
                ForumID = model.ForumID,
                Title = model.Title,
                Content = model.Content,
                UserID = ViewBag.CurrentUser.ID,
                Time = DateTime.Now,
                Top = false,
                LastReply = DateTime.Now
            };
            DbContext.Topics.Add(topic);
            DbContext.SaveChanges();
            return RedirectToAction("Index", "Topic", new { id = topic.ID });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        { 
            var topic = DbContext.Topics.Find(id);
            if (topic.UserID == ViewBag.CurrentUser.ID || ((CodeComb.Entity.User)ViewBag.CurrentUser).Role >= UserRole.Master)
            {
                var forum_id = topic.ForumID;
                DbContext.Topics.Remove(topic);
                DbContext.SaveChanges();
                return RedirectToAction("Index", "Forum", new { id = forum_id });
            }
            else
            {
                return RedirectToAction("Message", "Shared", new { msg = "您无权删除这个主题！" });
            }
        }

        [Authorize]
        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, string content)
        { 
            var topic = DbContext.Topics.Find(id);
            if (topic.UserID == ViewBag.CurrentUser.ID || ((CodeComb.Entity.User)ViewBag.CurrentUser).Role >= UserRole.Master)
            {
                topic.Content = content;
                DbContext.SaveChanges();
                return Content("OK");
            }
            else
            {
                return Content("NO");
            }
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UnSetTop(int id)
        {
            if (((Entity.User)ViewBag.CurrentUser).Role >= UserRole.Master)
            {
                var topic = DbContext.Topics.Find(id);
                topic.Top = false;
                DbContext.SaveChanges();
                return Content("OK");
            }
            else
            {
                return Content("NO");
            }
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SetTop(int id)
        {
            if (((Entity.User)ViewBag.CurrentUser).Role >= UserRole.Master)
            {
                var topic = DbContext.Topics.Find(id);
                topic.Top = true;
                DbContext.SaveChanges();
                return Content("OK");
            }
            else
            {
                return Content("NO");
            }
        }
	}
}