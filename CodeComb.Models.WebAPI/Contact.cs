using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeComb.Models.WebAPI
{
    public class Contacts : Base
    {
        public List<Contact> List { get; set; }
    }
    public class Contact
    {
        public int UserID { get; set; }
        public string Nickname { get; set; }
        public int UnreadMessageCount { get; set; }
        public string AvatarURL { get; set; }
        public string Motto { get; set; }
    }
}
