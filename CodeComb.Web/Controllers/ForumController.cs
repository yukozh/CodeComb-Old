using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CodeComb.Web.Controllers
{
    public class ForumController : BaseController
    {
        //
        // GET: /Forum/
        public ActionResult Index(int? id)
        {
            var forum = DbContext.Forums.Find(id);
            ViewBag.ForumList = (from f in DbContext.Forums
                                 where f.FatherID == null
                                 orderby f.Sort ascending
                                 select f).ToList();
            if(id == null)
                ViewBag.HotTopics = (from t in DbContext.Topics
                                     orderby t.Replies.Count descending
                                     select t).Take(10).ToList();
            else
                ViewBag.HotTopics = (from t in DbContext.Topics
                                     where t.ForumID == id
                                     orderby t.Replies.Count descending
                                     select t).Take(10).ToList();
            return View(forum);
        }
	}
}