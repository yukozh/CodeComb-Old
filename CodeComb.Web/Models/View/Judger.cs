using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CodeComb.Web.Models.View
{
    public class Judger
    {
        public Judger() { }
        public Judger(Entity.User user, SignalR.Client client) 
        {
            ID = user.ID;
            Nickname = Helpers.ColorName.GetNicknameHtml(user.Nickname, 4000);
            Gravatar = Helpers.Gravatar.GetAvatarURL(user.Gravatar, 50);
            Motto = HttpUtility.HtmlEncode(user.Motto);
            Ratio = client.Ratio;
        }
        public int ID { get; set; }
        public string Nickname { get; set; }
        public string Gravatar { get; set; }
        public string Motto { get; set; }
        public int Ratio { get; set; }
        public string Status { get { return Ratio == 0 ? "Free" : "Working"; } }
        public string Css { get { return Ratio == 0 ? "status-text-running" : "status-text-tle"; } }
    }
}