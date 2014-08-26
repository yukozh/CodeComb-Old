using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CodeComb.Web.Models.View
{
    public class HackCode
    {
        public int ID { get; set; }
        public bool Available { get; set; }
        public string Code { get; set; }
        public string Error { get; set; }
    }
}