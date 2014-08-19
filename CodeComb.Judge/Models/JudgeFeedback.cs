using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeComb.Judge.Models
{
    public class JudgeFeedback
    {
        public bool Success { get; set; }
        public int ID { get; set; }
        public Entity.JudgeResult Result { get; set; }
        public string Hint { get; set; }
        public int TimeUsage { get; set; }
        public int MemoryUsage { get; set; }
    }
}
