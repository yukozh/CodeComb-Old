using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CodeComb.Web.Models.View
{
    public class TestCase
    {
        public TestCase() { }
        public TestCase(Entity.TestCase testcase)
        {
            ID = testcase.ID;
            Input = testcase.Input;
            Output = testcase.Output;
        }
        public int ID { get; set; }
        public string Input { get; set; }
        public string Output { get; set; }
    }
}