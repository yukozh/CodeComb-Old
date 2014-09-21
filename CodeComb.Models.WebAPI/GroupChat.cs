using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeComb.Models.WebAPI
{
    public class GroupChats : Base
    {
        public List<GroupChat> List { get; set; }
        public int PageCount { get; set; }
    }
    public class GroupChat
    {
        public int ID { get; set; }
        public DateTime Time { get; set; }
        public int UserID { get; set; }
        public string Nickname { get; set; }
        public string AvatarURL { get; set; }
        public string Message { get; set; }
        public int GroupID { get; set; }
    }
}
