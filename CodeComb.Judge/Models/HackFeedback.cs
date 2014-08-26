using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeComb.Judge.Models
{
    public class HackFeedback
    {
        public bool Success { get; set; }
        public int StatusID { get; set; }
        public int HackID { get; set; }
        public Entity.HackResult Result { get; set; }
        public Entity.JudgeResult JudgeResult { get; set; }
        public int MemoryUsage { get; set; }
        public int TimeUsage { get; set; }
        public string Hint { get; set; }
        public string Output { get; set; }
    }
}
