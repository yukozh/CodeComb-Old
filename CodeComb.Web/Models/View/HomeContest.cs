using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CodeComb.Web.Models.View
{
    public class HomeContest
    {
        public HomeContest() { }
        public HomeContest(Entity.Contest contest) 
        {
            ID = contest.ID;
            Title = contest.Title;
            Begin = contest.Begin;
            End = contest.End;
            RestBegin = contest.RestBegin;
            RestEnd = contest.RestEnd;
        }
        public int ID { get; set; }
        public string Title { get; set; }
        public DateTime Begin { get; set; }
        public DateTime? RestBegin { get; set; }
        public DateTime? RestEnd { get; set; }
        public DateTime End { get; set; }
    }
}