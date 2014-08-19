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
        public int ID { get; set; }
        public Entity.HackResult Result { get; set; }
    }
}
