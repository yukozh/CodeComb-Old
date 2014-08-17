using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CodeComb.Web.Models.View
{
    public class StatusSnapshot
    {
        public StatusSnapshot() { }
        public StatusSnapshot(Entity.Contest contest, Entity.User user) 
        {
            var problems = contest.Problems.OrderBy(x => x.Credit).ToList();
            foreach (var problem in problems)
            {
                ProblemID = problem.ID;
                ProblemTitle = problem.Title;
                if (user == null)
                {
                    Result = "N/A";
                    Css = "nul";
                    return;
                }
                var statuses = problem.GetContestStatuses().Where(x => x.UserID == user.ID).OrderBy(x => x.Time);
                if (statuses.Count() == 0)
                {
                    Result = "N/A";
                    Css = "nul";
                    return;
                }
                if (contest.Format == Entity.ContestFormat.OI)
                {
                    Result = "PND";
                    Css = "ac";
                    return;
                }
                else if (contest.Format == Entity.ContestFormat.OPJOI)
                {
                    var count_ac = statuses.Last().JudgeTasks.Where(x => x.Result == Entity.JudgeResult.Accepted).Count();
                    var count_all = statuses.Last().JudgeTasks.Count;
                    var points = Convert.ToInt32(count_ac * 100 / count_all);
                    Result = points.ToString();
                    if (count_ac == count_all)
                        Css = "ac";
                    else
                        Css = "tle";
                    return;
                }
                else if (contest.Format == Entity.ContestFormat.ACM)
                {
                    var status = statuses.Where(x => x.Result == Entity.JudgeResult.Accepted).FirstOrDefault();
                    if (status == null)
                    {
                        Result = "NoA";
                        Css = "tle";
                    }
                    else
                    {
                        Result = "AC";
                        Css = "ac";
                    }
                    return;
                }
                else
                {
                    var status = statuses.Last();
                    if (status.Result == Entity.JudgeResult.Accepted)
                    {
                        Result = "AC";
                        Css = "ac";
                    }
                    else if (status.Result == Entity.JudgeResult.Hacked)
                    {
                        Result = "HKD";
                        Css = "wa";
                    }
                    else
                    {
                        Result = "NoA";
                        Css = "tle";
                    }
                    return;
                }
            }
        }
        public int ProblemID { get; set; }
        public string ProblemTitle { get; set; }
        public string Result { get; set; }
        public string Css { get; set; }
    }
}