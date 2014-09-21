using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeComb.Models.WebAPI
{
    public class GroupHomeworkStandings : Base
    {
        public List<GroupHomeworkStandingsItem> List { get; set; }
    }
    public class GroupHomeworkStandingsItem
    {
        public int UserID { get; set; }
        public int GroupID { get; set; }
        public string AvatarURL { get; set; }
        public string Nickname { get; set; }
        public int TotalPoints { get; set; }
        public List<GroupHomeworkProblem> Problems { get; set; }
    }
}
