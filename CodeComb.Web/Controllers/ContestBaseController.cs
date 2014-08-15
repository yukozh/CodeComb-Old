using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CodeComb.Entity;
using CodeComb.Database;

namespace CodeComb.Web.Controllers
{
    public class ContestBaseController : BaseController
    {
        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            base.Initialize(requestContext);
            ViewBag.IsMaster = false;
            ViewBag.IsCreator = false;
            if (!User.Identity.IsAuthenticated) return;
            var id = requestContext.RouteData.Values["id"] == null ? null : (int?)Convert.ToInt32(requestContext.RouteData.Values["id"]);
            if (id == null) return;
            var contest = DbContext.Contests.Find(id);
            var user = (from u in DbContext.Users where u.Username == User.Identity.Name select u).Single();
            if (user.Role >= UserRole.Master)
            {
                ViewBag.IsMaster = true;
                ViewBag.IsCreator = true;
            }
            else
            {
                var contestmanager = (from cm in contest.Managers where cm.UserID == user.ID select cm).SingleOrDefault();
                if (contestmanager != null)
                {
                    ViewBag.IsMaster = true;
                    if (contestmanager.IsCreator)
                        ViewBag.IsCreator = true;
                }
            }
        }
    }
}