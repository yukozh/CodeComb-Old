using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CodeComb.Web.Helpers
{
    public static class Standings
    {
        public static List<Models.View.Standing> Build(int id)//contest id
        {
            Database.DB db = new Database.DB();
            List<Models.View.Standing> standings = new List<Models.View.Standing>();
            var contest = db.Contests.Find(id);
            var users = (from s in db.Statuses
                         let pid = (from p in db.Problems
                                    where p.ContestID == contest.ID
                                    select p.ID).ToList()
                         where pid.Contains(s.ProblemID)
                         && s.Time >= contest.Begin
                         && s.Time < contest.End
                         select s.User).Distinct().ToList();
            foreach (var user in users)
                standings.Add(new Models.View.Standing(user, contest));
            switch (contest.Format)
            { 
                case Entity.ContestFormat.OI:
                    standings = standings.OrderByDescending(x => x.Key1).ThenBy(x => x.Key2).ThenBy(x => x.Key3).ToList();
                    break;
                case Entity.ContestFormat.OPJOI:
                    standings = standings.OrderByDescending(x => x.Key1).ThenBy(x => x.Key2).ThenBy(x => x.Key3).ToList();
                    break;
                case Entity.ContestFormat.Codeforces:
                    standings = standings.OrderByDescending(x => x.Key1).ThenByDescending(x => x.Key2).ThenBy(x => x.Key3).ToList();
                    break;
                case Entity.ContestFormat.TopCoder:
                    standings = standings.OrderByDescending(x => x.Key1).ThenByDescending(x => x.Key2).ThenBy(x => x.Key3).ToList();
                    break;
                case Entity.ContestFormat.ACM:
                    standings = standings.OrderByDescending(x => x.Key1).ThenBy(x => x.Key2).ThenBy(x => x.Key3).ToList();
                    break;
                default: break;
            }
            return standings;
        }
    }
}