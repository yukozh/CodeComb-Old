using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeComb.Models.WebAPI
{
    public class Profile:Base
    {
        public int UserID { get; set; }
        public string Nickname { get; set; }
        public int Rating { get; set; }
        public string Motto { get; set; }
        public string AvatarURL { get; set; }
    }
}
