using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CodeComb.Web.Models.View
{
    public class ProblemInContest
    {
        public ProblemInContest() { }
        public ProblemInContest(Entity.Problem problem, Entity.User user) 
        {
            ID = problem.ID;
            Title = problem.Title;
            Status = "";
            Credits = "";
            Lock = "";
            if (problem.Contest.Format == Entity.ContestFormat.Codeforces || problem.Contest.Format == Entity.ContestFormat.TopCoder)
            {
                var max = problem.Credit;
                int current;
                if (problem.Contest.Format == Entity.ContestFormat.Codeforces)
                {
                    current = Convert.ToInt32(max * (1 - 0.004 * (DateTime.Now - problem.Contest.Begin).TotalMinutes));
                    if (current < max * 0.3) current = Convert.ToInt32(max * 0.3);
                }
                else
                { 
                    if(user == null)
                    {
                        current = max;
                    }
                    else
                    {
                        var glance = (from g in problem.Glances
                                     where g.UserID == user.ID
                                     select g).SingleOrDefault();
                        if (glance == null) current = max;
                        else current = Convert.ToInt32(max * (1 * 0.004 * (DateTime.Now - glance.Time).TotalMinutes));
                    }
                }
                Credits = String.Format(" / ({0}/{1})", current, max);
            }
            if (user == null) return;
            var statuses = (from s in problem.Statuses
                            where s.UserID == user.ID
                            && !Entity.Status.FreeResults.Contains(s.Result)
                            && s.Time >= problem.Contest.Begin
                            && s.Time < problem.Contest.End
                            orderby s.Time ascending
                            select s).ToList();
            if (statuses.Count == 0)
            {
                Status = " / <a class='status-text-nul'>Not submited</a>";
                return;
            }
            if (problem.Contest.Format == Entity.ContestFormat.OI)
            {
                Status = " / <a class='status-text-ac'>Submited</a>";
                return;
            }
            else if (problem.Contest.Format == Entity.ContestFormat.OPJOI)
            {
                var count_ac = statuses.Last().JudgeTasks.Where(x=>x.Result == Entity.JudgeResult.Accepted).Count();
                var count_all = statuses.Last().JudgeTasks.Count;
                var points = Convert.ToInt32(count_ac * 100 / count_all);
                if(count_all == count_ac)
                    Status = " / <a class='status-text-ac'>" + points + "</a>";
                else
                    Status = " / <a class='status-text-tle'>" + points + "</a>";
                return;
            }
            else if (problem.Contest.Format == Entity.ContestFormat.ACM)
            {
                var status = (from s in statuses
                              where s.Result == Entity.JudgeResult.Accepted
                              select s).FirstOrDefault();
                if (status != null)
                {
                    Status = " / <a class='status-text-ac'>Accepted</a>";
                    return;
                }
                Status = " / <a class='status-text-wa'>Not Accepted</a>";
                return;
            }
            else//CF TC
            {
                var status = statuses.Last();
                if (problem.Locks.Where(x => x.UserID == user.ID).Count() > 0)
                {
                    Lock = " / <a class='status-text-wa'>Locked</a>";
                }
                if (status.Result == Entity.JudgeResult.Accepted)
                {
                    Status = " / <a class='status-text-ac'>Accepted</a>";
                    return;
                }
                else if (status.Result == Entity.JudgeResult.Hacked)
                {
                    Status = " / <a class='status-text-wa'>Hacked</a>";
                    return;
                }
                else
                {
                    Status = " / <a class='status-text-tle'>Not Accepted</a>";
                    return;
                }
            }
        }
        public int ID { get; set; }
        public string Title { get; set; }
        public string Status { get; set; }
        public string Credits { get; set; }
        public string Lock { get; set; }
    }
}