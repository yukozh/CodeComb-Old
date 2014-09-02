using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CodeComb.Web.Controllers
{
    public class SharedController : BaseController
    {
        //
        // GET: /Shared/
        public ActionResult Message(string msg)
        {
            return View("Message", (object)msg);
        }

        [HttpPost]
        [Authorize]
        public ActionResult Markdown(string content)
        {
            return Content(Helpers.Markdown.ToHtml(content));
        }

        public ActionResult Judger()
        {
            var judgers = new List<Models.View.Judger>();
            foreach (var client in SignalR.JudgeHub.Online)
            {
                var user = (from u in DbContext.Users where u.Username == client.Username select u).Single();
                judgers.Add(new Models.View.Judger(user, client));
            }
            return View(judgers);
        }

        public ActionResult About()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult BroadCast(string content)
        {
            var user = (Entity.User)ViewBag.CurrentUser;
            if (user.Role < Entity.UserRole.Master)
                return Content("NO");
            SignalR.CodeCombHub.context.Clients.All.onBroadCast(Helpers.HtmlFilter.Instance.SanitizeHtml(content));
            SignalR.MobileHub.context.Clients.All.onBroadCast(HttpUtility.HtmlDecode(content));
            SignalR.MobileHub.PushToAll(HttpUtility.HtmlDecode(content));
            return Content("OK");
        }
	}
}