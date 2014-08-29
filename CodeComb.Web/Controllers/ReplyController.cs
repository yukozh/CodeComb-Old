using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CodeComb.Entity;

namespace CodeComb.Web.Controllers
{
    public class ReplyController : BaseController
    {
        //
        // GET: /Reply/
        public ActionResult GetReplies(int id, int page)
        {
            var replies = DbContext.Topics.Find(id).Replies.Where(x => x.FatherID == null).OrderByDescending(x => x.Time).Skip(10 * page).Take(10).ToList();
            return View(replies);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Create(int id, string content, int? father_id)
        {
            if (string.IsNullOrEmpty(content))
                return Content("NO");
            var reply = new Reply 
            { 
                FatherID = father_id,
                Time = DateTime.Now,
                Content = content,
                TopicID = id,
                UserID = ViewBag.CurrentUser.ID
            };
            DbContext.Replies.Add(reply);
            DbContext.SaveChanges();
            var topic = DbContext.Topics.Find(id);
            topic.LastReply = reply.Time;
            DbContext.SaveChanges();
            return Content("OK");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Delete(int id)
        {
            var reply = DbContext.Replies.Find(id);
            int? father_id = reply.FatherID;
            int topic_id = reply.TopicID;
            if (reply.UserID == ViewBag.CurrentUser.ID || (reply.FatherID != null && reply.Father.UserID == ViewBag.CurrentUser.ID) || ((CodeComb.Entity.User)ViewBag.CurrentUser).Role >= UserRole.Master)
            {
                DbContext.Replies.Remove(reply);
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