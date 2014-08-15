using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CodeComb.Web.Models.View
{
    public class HomeNews
    {
        public HomeNews() { }
        public HomeNews(Entity.Topic topic)
        {
            ID = topic.ID;
            Title = topic.Title;
            Time = topic.Time;
        }
        public int ID { get; set; }
        public string Title { get; set; }
        public DateTime Time { get; set; }
    }
}