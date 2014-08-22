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
            Sort(contest.Format, ref standings);
            return standings;
        }
        public static void Sort(Entity.ContestFormat format,ref List<Models.View.Standing> standings)
        {
            switch (format)
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
        }
        public static void Update(int user_id,int contest_id, ref List<Models.View.Standing> standings)
        {
            Database.DB db = new Database.DB();
            var user = db.Users.Find(user_id);
            var contest = db.Contests.Find(contest_id);
            var index = standings.FindIndex(x => x.UserID == user_id);
            var new_standing = new Models.View.Standing(user, contest);
            if (index < 0)
            {
                standings.Add(new_standing);
            }
            else
            {
                standings[index] = new_standing;
            }
            Sort(contest.Format, ref standings);

            //推送排名变化
            SignalR.CodeCombHub.context.Clients.All.onStandingChanged(contest_id, new_standing);
        }
    }
}