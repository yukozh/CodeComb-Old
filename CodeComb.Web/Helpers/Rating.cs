using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CodeComb.Web.Helpers
{
    public static class Rating
    {
        public static void RatingDelete(int id)
        {
            var DbContext = new Database.DB();
            var old_ratings = (from r in DbContext.Ratings
                               where r.ContestID == id
                               select r).ToList();
            foreach (var r in old_ratings)
                DbContext.Ratings.Remove(r);
            DbContext.SaveChanges();
        }

        public static void RatingCount(int id)//contest_id
        {
            RatingDelete(id);

            #region 初始化
            var DbContext = new Database.DB();
            var contest = DbContext.Contests.Find(id);
            var statuses = (from s in DbContext.Statuses
                            where s.Problem.ContestID == contest.ID
                            select s).ToList();
            var ranklist = Helpers.Standings.Build(id);
            var problems = contest.Problems.OrderBy(x => x.Credit).ToList();
            #endregion
            for (int i = 0; i < ranklist.Count; i++)
            {
                var user_credits = 0;
                #region 计算排名分 (-15 ~ 15)
                var pts = ranklist.Count / 2 - i;
                if (pts > 15) pts = 15;
                if (pts < -15) pts = -15;
                user_credits += pts - i;
                #endregion
                var num = 0;
                foreach (var detail in ranklist[i].Details)
                {
                    var problem_ratings = problems[num].Difficulty;
                    var user = DbContext.Users.Find(ranklist[i].UserID);

                    var user_ratings = user.Ratings.Sum(x => x.Credit) + 1500;
                    var extend_pts = (problem_ratings - user_ratings) / 300.0;
                    if (extend_pts > 1) extend_pts = 1;
                    if (extend_pts < -1) extend_pts = -1;

                    #region ACM赛制
                    if (contest.Format == Entity.ContestFormat.ACM)
                    {
                        var base_pts = 10;
                        base_pts -= detail.Key3;
                        if (base_pts < 0) base_pts = 0;
                        if (detail.Key1 == 1)
                        {
                            user_credits += base_pts + (int)(Math.Floor((extend_pts + 1) * 25));
                        }
                        else
                        {
                            user_credits += (int)(Math.Floor((extend_pts - 1) * 25));
                        }
                    }
                    #endregion
                    #region OI赛制
                    else if (contest.Format == Entity.ContestFormat.OI)
                    {
                        if (detail.Key1 > 50)
                        {
                            user_credits += (int)((Math.Floor((extend_pts + 1) * 25) * (100 - detail.Key1) / 50.0));
                        }
                        else
                        {
                            user_credits += (int)(Math.Floor((extend_pts - 1) * 25) * (100 - detail.Key1) / 100.0);
                        }
                    }
                    #endregion
                    #region OPJOI赛制
                    else if (contest.Format == Entity.ContestFormat.OPJOI)
                    {
                        var base_pts = 10;
                        base_pts -= detail.Key2 + 1;
                        if (base_pts < 0) base_pts = 0;
                        if (detail.Key1 > 50)
                        {
                            user_credits +=  (int)((Math.Floor((extend_pts + 1) * 25) * (100 - detail.Key1) / 50.0));
                        }
                        else
                        {
                            user_credits +=  (int)(Math.Floor((extend_pts - 1) * 25) * (100 - detail.Key1) / 100.0);
                        }
                    }
                    #endregion
                    #region CF、TC赛制
                    else if (contest.Format == Entity.ContestFormat.Codeforces || contest.Format == Entity.ContestFormat.TopCoder)
                    {
                        var base_pts = (int)Math.Floor(detail.Key2 - (detail.Key3 / 2.0));
                        if (base_pts > 10) base_pts = 10;
                        if (base_pts < -5) base_pts = -5;
                        if (base_pts < 0) base_pts = 0;
                        if (detail.Key1 > 0)
                        {
                            user_credits +=  base_pts + (int)((Math.Floor((extend_pts + 1) * 30)) * (problems[num].Credit - detail.Key1));
                        }
                        else
                        {
                            user_credits += base_pts + (int)((Math.Floor((extend_pts + 1) * 25)));
                        }
                    }
                    #endregion
                    num++;
                }
                DbContext.Ratings.Add(new Entity.Rating 
                {
                    Credit = user_credits,
                    ContestID = id,
                    Time = contest.End,
                    UserID = ranklist[i].UserID
                });
                DbContext.SaveChanges();
            }
        }
    }
}