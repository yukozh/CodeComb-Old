using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace CodeComb.Web.SignalR
{
    public class CodeCombHub : Hub
    {
        public static Microsoft.AspNet.SignalR.IHubContext context = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<CodeCombHub>();
        public override System.Threading.Tasks.Task OnConnected()
        {
            if (Context.User.Identity.IsAuthenticated)
            {
                return Groups.Add(Context.ConnectionId, Context.User.Identity.Name);
            }
            return base.OnConnected();
        }
        public override System.Threading.Tasks.Task OnDisconnected(bool stopCalled)
        {
            if (Context.User.Identity.IsAuthenticated)
            {
                return Groups.Remove(Context.ConnectionId, Context.User.Identity.Name);
            }
            return base.OnDisconnected(stopCalled);
        }
        public void JoinJudgeList()
        {
            Groups.Add(Context.ConnectionId, "System Judge");
        }
    }
}