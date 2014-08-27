using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CodeComb.Web.SignalR
{
    public class Client
    {
        public string Username { get; set; }
        public string Token { get; set; }
        public int MaxThreads { get; set; }
        public int CurrentThreads { get; set; }
        public int FreeThreads { get { return MaxThreads - CurrentThreads; } }
        public int Ratio 
        {
            get 
            {
                if (MaxThreads == 0) return 0;
                return CurrentThreads * 100 / MaxThreads;
            }
        }
        public override bool Equals(object obj)
        {
            var c = obj as Client;
            if (c.Username == this.Username) return true;
            return false;
        }
    }
}