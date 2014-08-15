using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CodeComb.Web.Models.View
{
    public class Rating
    {
        public Rating() { }
        public Rating(Entity.User user, int rank)
        {
            UserID = user.ID;
            Nickname = Helpers.ColorName.GetNicknameHtml(user.Nickname, user.Ratings.Sum(x => x.Credit) + 1500);
            Credit = user.Ratings.Sum(x => x.Credit) + 1500;
            Rank = rank;
            Gravatar = Helpers.Gravatar.GetAvatarURL(user.Gravatar, 200);
        }
        public int UserID { get; set; }
        public string Nickname { get; set; }
        public int Credit { get; set; }
        public int Rank { get; set; }
        public string Gravatar { get; set; }
    }
}