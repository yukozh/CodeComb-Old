using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CodeComb.Web.Models.View
{
    public class Topic
    {
        public Topic() { }
        public Topic(Entity.Topic topic)
        {
            ID = topic.ID;
            ForumID = topic.ForumID;
            ForumTitle = topic.Forum.Title;
            Gravatar = Helpers.Gravatar.GetAvatarURL(topic.User.Gravatar, 180);
            Nickname = Helpers.ColorName.GetNicknameHtml(topic.User.Nickname, topic.User.Ratings.Sum(x => x.Credit) + 1500);
            RepliesCount = topic.Replies.Count;
            Time = Helpers.Time.ToTimeTip(topic.Time);
            Title = HttpUtility.HtmlEncode(topic.Title);
            Top = topic.Top;
            UserID = topic.UserID;
            HasReply = topic.Replies.Count == 0 ? false : true;
            LastReplyNickname = topic.Replies.Count == 0 ? null : (Helpers.ColorName.GetNicknameHtml(topic.Replies.OrderBy(x => x.Time).Last().User.Nickname, topic.Replies.OrderBy(x => x.Time).Last().User.Ratings.Sum(x => x.Credit) + 1500));
            LastReplyTime = topic.Replies.Count == 0 ? null : (Helpers.Time.ToTimeTip(topic.Replies.OrderBy(x => x.Time).Last().Time));
            LastReplyUserID = topic.Replies.Count == 0 ? null : (int?)(topic.Replies.OrderBy(x => x.Time).Last().UserID);
        }
        public int ID { get; set; }
        public string Nickname { get; set; }
        public int RepliesCount { get; set; }
        public string Time { get; set; }
        public int ForumID { get; set; }
        public string ForumTitle { get; set; }
        public bool Top { get; set; }
        public string Title { get; set; }
        public int UserID { get; set;}
        public string Gravatar { get; set; }
        public bool HasReply { get; set; }
        public string LastReplyTime { get; set; }
        public string LastReplyNickname { get; set; }
        public int? LastReplyUserID { get; set; }
    }
}