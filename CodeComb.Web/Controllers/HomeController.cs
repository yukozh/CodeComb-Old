using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CodeComb.Entity;
using CodeComb.Database;
using CodeComb.Web.Models.View;

namespace CodeComb.Web.Controllers
{
    public class HomeController : BaseController
    {
        //
        // GET: /Home/
        public ActionResult Index()
        {
            var contests = new List<HomeContest>();
            var _contests = (from c in DbContext.Contests
                                    where c.End >= DateTime.Now
                                    && string.IsNullOrEmpty(c.Password)
                                    && c.Ready
                                    && c.RatingBegin != null
                                    && c.RatingEnd != null
                                    orderby c.Begin ascending
                                    select c).ToList();
            foreach (var contest in _contests)
                contests.Add(new HomeContest(contest));
            ViewBag.HomeContests = contests;
            var news = new List<HomeNews>();
            var _news = (from t in DbContext.Topics
                                where t.Top
                                orderby t.Time descending
                                select t).ToList();
            foreach(var __news in _news)
                news.Add(new HomeNews(__news));
            ViewBag.HomeNews = news;
            return View();
        }
	}
}