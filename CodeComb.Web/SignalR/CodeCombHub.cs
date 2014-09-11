using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace CodeComb.Web.SignalR
{
    public class CodeCombHub : Hub
    {
        public static Hashtable BarCode = new Hashtable();
        public static Hashtable LoginTokens = new Hashtable();
        public static Microsoft.AspNet.SignalR.IHubContext context = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<CodeCombHub>();
        public override System.Threading.Tasks.Task OnConnected()
        {
            string token = "";
            bool existed = false;
            do
            {
                token = Helpers.String.RandomString(32).ToLower();
                if (BarCode[token] == null)
                {
                    BarCode[token] = Context.ConnectionId;
                    existed = false;
                }
                else
                {
                    existed = true;
                }
            }
            while (existed);
            Clients.Client(Context.ConnectionId).onBarCodeToken(token);
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
            foreach (DictionaryEntry de in BarCode)
            {
                if (de.Value.ToString() == Context.ConnectionId)
                    BarCode.Remove(de.Key);
            }
            return base.OnDisconnected(stopCalled);
        }
        public override System.Threading.Tasks.Task OnReconnected()
        {
            string token = "";
            bool existed = false;
            do
            {
                token = Helpers.String.RandomString(32).ToLower();
                if (BarCode[token] == null)
                {
                    BarCode[token] = Context.ConnectionId;
                    existed = false;
                }
                else
                {
                    existed = true;
                }
            }
            while (existed);
            Clients.Client(Context.ConnectionId).onBarCodeToken(token);
            return base.OnReconnected();
        }
        public void JoinJudgeList()
        {
            Groups.Add(Context.ConnectionId, "System Judge");
        }
    }
}