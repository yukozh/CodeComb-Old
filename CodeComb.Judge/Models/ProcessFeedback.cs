using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeComb.Judge.Models
{
    public class ProcessFeedback
    {
        public int ExitCode { get; set; }
        public string StdOut { get; set; }
        public string StdErr { get; set; }
        public int PeakPageSize { get; set; }
        public int PeakWorkingSetSize { get; set; }
        public int UserTime { get; set; }
        public int RealTime { get; set; }
    }
}
