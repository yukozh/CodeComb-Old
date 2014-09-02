using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CodeComb.Web.Models.View
{
    public class Contest
    {
        public Contest() { }
        public Contest(Entity.Contest contest)
        {
            ID = contest.ID;
            Format = contest.Format.ToString();
            ProblemCount = contest.Problems.Count;
            Time = contest.Begin.ToString("yyyy-MM-dd HH:mm");
            TimeLength = Helpers.Time.ToVagueTimeLength(contest.Begin, contest.End);
            Title = HttpUtility.HtmlEncode(contest.Title);
            Database.DB DbContext = new Database.DB();
            UserCount =  (from s in DbContext.Statuses
                         let pid = (from p in DbContext.Problems
                                    where p.ContestID == contest.ID
                                    select p.ID).ToList()
                         where pid.Contains(s.ProblemID)
                         && s.Time >= contest.Begin
                         && s.Time < contest.End
                         select s.UserID).Distinct().Count();
            Managers = "";
            foreach (var m in contest.Managers)
            {
                Managers += String.Format("<a href='/User/{0}'>{1}</a> ", m.UserID, Helpers.ColorName.GetNicknameHtml(m.User.Nickname, m.User.Ratings.Sum(x => x.Credit) + 1500));
            }
            Rating = "";
            if (contest.Ready)
            {
                Rating = string.Format("<span class='label blue'>评级赛({0}~{1})</span> ", contest.RatingBegin, contest.RatingEnd);
            }
            if (string.IsNullOrEmpty(contest.Password))
            {
                Private = false;
            }
            else
            {
                Private = true;
            }
        }
        public int ID { get; set; }
        public string TimeLength { get; set; }
        public string Time { get; set; }
        public string Format { get; set; }
        public string Title { get; set; }
        public int ProblemCount { get; set; }
        public int UserCount { get; set; }
        public string Managers { get; set; }
        public string Rating { get; set; }
        public bool Private { get; set; }
    }
}