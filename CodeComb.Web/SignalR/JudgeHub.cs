using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace CodeComb.Web.SignalR
{
    public class JudgeHub : Hub
    {
        public void Hello()
        {
            Clients.All.hello();
        }
    }
}