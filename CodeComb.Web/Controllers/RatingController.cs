using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CodeComb.Web.Models.View;

namespace CodeComb.Web.Controllers
{
    public class RatingController : BaseController
    {
        //
        // GET: /Rating/
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult GetRatings(int page)
        {
            var users = (from u in DbContext.Users
                         //where u.Ratings.Count() > 0
                         orderby u.Ratings.Sum(x => x.Credit) descending
                         select u).Skip(12 * page).Take(12).ToList();
            List<Rating> ratings = new List<Rating>();
            for (int i = 0; i < users.Count(); i++)
                ratings.Add(new Rating(users[i], page * 12 + i + 1));
            return Json(ratings, JsonRequestBehavior.AllowGet);
        }
	}
}