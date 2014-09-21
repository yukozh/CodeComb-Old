using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeComb.Models.WebAPI
{
    public class GroupApplications : Base
    {
        public List<GroupApplication> List { get; set; }
        public int PageCount { get; set; }
    }
    public class GroupApplication
    {
        public int ID { get; set; }
        public int GroupID { get; set; }
        public int UserID { get; set; }
        public string AvatarURL { get; set; }
        public string Nickname { get; set; }
        public string Message { get; set; }
        public string Response { get; set; }
        public int StatusAsInt { get; set; }
        public string Status { get; set; }
    }
}
