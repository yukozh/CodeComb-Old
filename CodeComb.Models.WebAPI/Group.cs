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
        public byte[] Icon { get; set; }
        public int MemberCount { get; set; }
    }
    public class Groups : Base 
    {
        public List<Group> List { get; set; }
        public int PageCount { get; set; }
    }
}
