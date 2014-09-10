using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeComb.Models.WebAPI
{
    public class Contests : Base
    {
        public List<Contest> List { get; set; }
        public int PageCount { get; set; }
    }
    public class Contest
    {
        public int ContestID { get; set; }
        public string Title { get; set; }
        public string Format { get; set; }
        public int FormatAsInt { get; set; }
        public DateTime Begin { get; set; }
        public DateTime End { get; set; }
        public DateTime? RestBegin { get; set; }
        public DateTime? RestEnd { get; set; }
    }
}
