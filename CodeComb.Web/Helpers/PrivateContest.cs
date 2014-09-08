using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CodeComb.Web.Helpers
{
    public static class PrivateContest
    {
        public static bool IsUserInPrivateContest(Entity.User user, Entity.Contest contest)
        {
            if (DateTime.Now < contest.Begin || DateTime.Now >= contest.End)
                return true;
            if (string.IsNullOrEmpty(contest.Password))
                return true;
            if (user == null)
                return false;
            var joinlog = contest.JoinLogs.Where(x => x.UserID == user.ID).FirstOrDefault();
            if (joinlog == null)
            {
                if (user.Role >= Entity.UserRole.Master || (from cm in contest.Managers select cm.UserID).Contains(user.ID))
                {
                    Database.DB DbContext = new Database.DB();
                    DbContext.JoinLogs.Add(new Entity.JoinLog 
                    { 
                        ContestID = contest.ID,
                        UserID = user.ID
                    });
                    DbContext.SaveChanges();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }
    }
}