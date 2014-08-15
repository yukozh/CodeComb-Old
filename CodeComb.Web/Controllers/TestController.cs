using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CodeComb.Web.Controllers
{
    public class TestController : BaseController
    {
        //
        // GET: /Test/
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult StatusPush(int id)
        {
            var _status = DbContext.Statuses.Find(id);
            var statistics = new int[Entity.CommonEnums.JudgeResultDisplay.Count()];
            var _statistics = from jt in _status.JudgeTasks
                              group jt.ResultAsInt by jt.ResultAsInt into g
                              select new KeyValuePair<int, int>(g.Key, g.Count());
            foreach (var item in _statistics)
                statistics[item.Key] = item.Value;
            var _result = "";
            if (_status.Result == Entity.JudgeResult.Pending)
                _result = "Pending";
            else if (_status.Result == Entity.JudgeResult.Running)
                _result = "Running";
            else if (_status.Result == Entity.JudgeResult.Accepted)
                _result = "Accepted";
            else
                _result = "Not Accepted";
            var status = new Models.View.Status
            {
                ID = _status.ID,
                Nickname = Helpers.ColorName.GetNicknameHtml(_status.User.Nickname, _status.User.Ratings.Sum(x => x.Credit) + 1500),
                TimeTip = Helpers.Time.ToTimeTip(_status.Time),
                Result = _result,
                PointCount = _status.JudgeTasks.Count,
                ProblemTitle = _status.Problem.Title,
                ProblemID = _status.ProblemID,
                Gravatar = "<img src='" + Helpers.Gravatar.GetAvatarURL(_status.User.Gravatar, 180) + "' />",
                Statistics = statistics
            };
            SignalR.CodeCombHub.context.Clients.All.onStatusChanged(status);
            return Content("Done");
        }
	}
}