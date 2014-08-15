using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CodeComb.Web.Models.View
{
    public class Status
    {
        public Status() { }
        public Status(Entity.Status status) 
        {
            var statistics = new int[Entity.CommonEnums.JudgeResultDisplay.Count()];
            var _statistics = from jt in status.JudgeTasks
                              group jt.ResultAsInt by jt.ResultAsInt into g
                              select new KeyValuePair<int, int>(g.Key, g.Count());
            foreach (var item in _statistics)
                statistics[item.Key] = item.Value;
            var _result = "";
            if (status.Result == Entity.JudgeResult.Pending)
                    _result = "Pending";
                else if (status.Result == Entity.JudgeResult.Running)
                    _result = "Running";
                else if (status.Result == Entity.JudgeResult.Accepted)
                    _result = "Accepted";
                else
                    _result = "Not Accepted";
            ID = status.ID;
            _Nickname = status.User.Nickname;
            Nickname = Helpers.ColorName.GetNicknameHtml(status.User.Nickname, status.User.Ratings.Sum(x => x.Credit) + 1500);
            TimeTip = Helpers.Time.ToTimeTip(status.Time);
            Result = _result;
            PointCount = status.JudgeTasks.Count;
            ProblemTitle = status.Problem.Title;
            ProblemID = status.ProblemID;
            ContestID = status.Problem.ContestID;
            Gravatar = "<img src='" + Helpers.Gravatar.GetAvatarURL(status.User.Gravatar, 180) + "' />";
            Statistics = statistics;
            UserID = status.UserID;
            MemoryUsage = status.JudgeTasks.Sum(x => x.MemoryUsage);
            TimeUsage = status.JudgeTasks.Sum(x => x.TimeUsage);
            Language = Entity.CommonEnums.LanguageDisplay[status.LanguageAsInt];
        }
        public int ID { get; set; }
        public string Result { get; set; }
        public int PointCount { get; set; }
        public int ProblemID { get; set; }
        public int ContestID { get; set; }
        public string ProblemTitle { get; set; }
        public dynamic Statistics { get; set; }
        public string Gravatar { get; set; }
        public string Nickname { get; set; }
        public string _Nickname { get; set; }
        public string TimeTip { get; set; }
        public int UserID { get; set; }
        public int MemoryUsage { get; set; }
        public int TimeUsage { get; set; }
        public string Language { get; set; }
    }
}