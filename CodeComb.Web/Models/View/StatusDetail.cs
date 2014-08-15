using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CodeComb.Web.Models.View
{
    public class StatusDetail
    {
        public StatusDetail() { }
        public StatusDetail(Entity.JudgeTask judgetask, int index)
        {
            ID = index;
            MemoryUsage = judgetask.MemoryUsage;
            TimeUsage = judgetask.TimeUsage;
            Result = judgetask.ResultAsInt;
            Hint = Helpers.HtmlFilter.Instance.SanitizeHtml(judgetask.Hint);
        }
        public int ID { get; set; }
        public int Result { get; set; }
        public int TimeUsage { get; set; }
        public int MemoryUsage { get; set; }
        public string Hint { get; set; }
    }
}