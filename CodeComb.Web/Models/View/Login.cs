using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CodeComb.Web.Models.View
{
    public class Login
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public bool Remember { get; set; }
    }
}