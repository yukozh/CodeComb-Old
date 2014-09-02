using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeComb.Mobile.Models
{
    public class Clarification
    {
        public int ID { get; set; }
        public int ContestID { get; set; }
        public string Question { get; set; }
        public int Status { get; set; }
        public string Answer { get; set; }
        public string Problem { get; set; }
        public DateTime Time { get; set; }
    }
}
