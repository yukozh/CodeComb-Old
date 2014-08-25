using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeComb.Judge.Models
{
    public class Result
    {
        public int TimeUsage { get; set; }
        public int PagedSize { get; set; }
        public int WorkingSet { get; set; }
        public long ExitCode { get; set; }
    }
}
