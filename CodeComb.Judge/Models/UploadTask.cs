using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeComb.Judge.Models
{
    public class UploadTask
    {
        public string Token { get; set; }
        public int ID { get; set; }
        public string Input { get; set; }
        public string Output { get; set; }
        public bool HasOutput { get; set; }
        public string Hash { get; set; }
    }
}
