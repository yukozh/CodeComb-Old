using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CodeComb.Entity;
using CodeComb.Database;

namespace CodeComb.Web.Controllers
{
    public class BaseController : Controller
    {
        public readonly DB DbContext = new DB();
        public BaseController()
        {
            
        }
        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            base.Initialize(requestContext);
            if (requestContext.HttpContext.User != null && requestContext.HttpContext.User.Identity.IsAuthenticated)
            {
                ViewBag.CurrentUser = (from u in DbContext.Users
                                       where u.Username == requestContext.HttpContext.User.Identity.Name
                                       select u).Single();
            }
            else
            {
                ViewBag.CurrentUser = null;
            }
        }
	}
}