using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeComb.Mobile.Models
{
    public class Contest
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public DateTime Begin { get; set; }
        public DateTime End { get; set; }
        public DateTime? RestBegin { get; set; }
        public DateTime? RestEnd { get; set; }
        public string Format { get; set; }
    }
}
