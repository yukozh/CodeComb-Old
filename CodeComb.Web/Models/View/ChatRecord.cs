using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CodeComb.Web.Models.View
{
    public class ChatRecord
    {
        public ChatRecord() { }
        public ChatRecord(Entity.Message message) 
        {
            ID = message.ID;
            Time = message.Time.ToString("yyyy-MM-dd HH:mm:ss");
            Content = HttpUtility.HtmlEncode(message.Content);
            SenderID = message.SenderID;
            ReceiverID = message.ReceiverID;
            SenderNickname = Helpers.ColorName.GetNicknameHtml(message.Sender.Nickname, message.Sender.Ratings.Sum(x => x.Credit) + 1500);
            ReceiverNickname = Helpers.ColorName.GetNicknameHtml(message.Receiver.Nickname, message.Receiver.Ratings.Sum(x => x.Credit) + 1500);
            Gravatar = Gravatar = "<img class='post-face' src='" + Helpers.Gravatar.GetAvatarURL(message.Sender.Gravatar, 180) + "' />";
        }
        public int ID { get; set; }
        public string Time { get; set; }
        public string Gravatar { get; set; }
        public string Content { get; set; }
        public int SenderID { get; set; }
        public int ReceiverID { get; set; }
        public string SenderNickname { get; set; }
        public string ReceiverNickname { get; set; }
    }
}