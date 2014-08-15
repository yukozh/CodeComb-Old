using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CodeComb.Web.Models.View
{
    public class Contact
    {
        public Contact() { }
        public Contact(Entity.User user, int myid)
        {
            MessageCount = user.PMSent.Where(x => x.ReceiverID == myid && x.Read == false).Count();
            ID = user.ID;
            Gravatar = "<img class='post-face' src='" + Helpers.Gravatar.GetAvatarURL(user.Gravatar, 180) + "' />";
            Nickname = Helpers.ColorName.GetNicknameHtml(user.Nickname, user.Ratings.Sum(x => x.Credit) + 1500);
            Online = user.Online;
        }
        public int ID { get; set; }
        public int MessageCount { get; set; }
        public string Gravatar { get; set; }
        public string Nickname { get; set; }
        public bool Online { get; set; }
    }
}