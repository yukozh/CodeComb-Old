using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CodeComb.Web.Models.View
{
    public class Clar
    {
        public Clar() { }
        public Clar(Entity.Clarification clar) 
        {
            ID = clar.ID;
            NoProblemRelation = false;
            if (clar.ProblemID == null)
            {
                ProblemRelation = "General";
                NoProblemRelation = true;
            }
            else
                ProblemRelation = HttpUtility.HtmlEncode(clar.Problem.Title);
            Question = HttpUtility.HtmlEncode(clar.Question);
            Answer = HttpUtility.HtmlEncode(clar.Answer);
            Time = clar.Time.ToString("yyyy-MM-dd HH:mm:ss");
        }
        public int ID { get; set; }
        public string ProblemRelation { get; set; }
        public bool NoProblemRelation { get; set; }
        public string Question { get; set; }
        public string Answer { get; set; }
        public string Time { get; set; }
    }
}