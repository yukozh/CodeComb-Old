using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CodeComb.Web.Models.View
{
    public class ProblemPager
    {
        public ProblemPager() { }
        public ProblemPager(int page) 
        {
            Display = page * 100 + "";
            LessThan = page * 100;
            MoreThan = (page - 1) * 100 + 1;
        }
        public string Display { get; set; }
        public int LessThan { get; set; }
        public int MoreThan { get; set; }
    }
}