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
        public FileType Type { get; set; }
        public string Data { get; set; }
    }
    public enum FileType { Input, Output };
}
