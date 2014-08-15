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
	}
}