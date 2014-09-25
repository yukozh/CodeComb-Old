using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeComb.Models.WebAPI
{
    public class GroupMembers : Base
    {
        public List<GroupMember> List { get; set; }
    }
    public class GroupMember
    {
        public int ID { get; set; }
        public string Nickname { get; set; }
        public string AvatarURL { get; set; }
        public int RoleAsInt { get; set; }
        public string Role { get; set; }
    }
}
