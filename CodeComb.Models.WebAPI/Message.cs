using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeComb.Models.WebAPI
{
    public class Messages : Base
    {
        public List<Message> List { get; set; }
    }
    public class Message
    {
        public int SenderID { get; set; }
        public int ReceiverID { get; set; }
        public DateTime Time { get; set; }
        public string Content { get; set; }
    }
}
