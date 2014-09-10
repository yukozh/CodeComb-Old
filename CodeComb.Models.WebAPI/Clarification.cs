using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeComb.Models.WebAPI
{
    public class Clarifications : Base
    {
        public List<Clarification> List { get; set; }
    }
    public class Clarification
    {
        public int ClarID { get; set; }
        public string Question { get; set; }
        public string Answer { get; set; }
        public string Status { get; set; }
        public int StatusAsInt { get; set; }
        public string Category { get; set; }
        public DateTime Time { get; set; }
        public int ContestID { get; set; }
    }
}
