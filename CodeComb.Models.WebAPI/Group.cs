using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeComb.Models.WebAPI
{
    public class Group
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public int MemberCount { get; set; }
    }

    public class GroupProfile : Group 
    {
        public int Code { get; set; }
        public string Info { get; set; }
        public bool IsSuccess { get; set; }
    }

    public class Groups : Base 
    {
        public List<Group> List { get; set; }
        public int PageCount { get; set; }
    }
}
