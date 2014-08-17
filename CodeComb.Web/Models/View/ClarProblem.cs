using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CodeComb.Web.Models.View
{
    public class ClarProblem
    {
        public ClarProblem() { }
        public ClarProblem(Entity.Problem problem)
        {
            ID = problem.ID;
            Title = problem.Title;
        }
        public int ID { get; set; }
        public string Title { get; set; }
    }
}