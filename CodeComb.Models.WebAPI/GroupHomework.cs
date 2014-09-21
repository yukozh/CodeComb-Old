using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeComb.Models.WebAPI
{
    public class GroupHomeworks : Base 
    {
        public List<GroupHomework> List { get; set; }
        public int PageCount { get; set; }
    }
    public class GroupHomework
    {
        public int ID { get; set; }
        public int GroupID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public List<GroupHomeworkProblem> Problems { get; set; }
        public DateTime Begin { get; set; }
        public DateTime End { get; set; }
    }
    public class GroupHomeworkProblem 
    {
        public int ProblemID { get; set; }
        public string Title { get; set; }
        public string Status { get; set; }
        public string Code { get; set; }
        public int Points { get; set; }
    }
}
