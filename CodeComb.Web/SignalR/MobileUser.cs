using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CodeComb.Web.SignalR
{
    public class MobileUser
    {
        public int ID { get; set; }
        public string Username { get; set; }
        public string iOSToken { get; set; }
        public string ConnectionID { get; set; }
        public string ClientInfo { get; set; }
        public ClientType Type { get; set; }
        public Entity.UserRole Role { get; set; }
        public override int GetHashCode()
        {
            return this.ID;
        }
        public override bool Equals(object obj)
        {
            var u = obj as MobileUser;
            if (u.ID == this.ID) return true;
            else return false;
        }
    }
    public enum ClientType { iOS, Android, WP8, Win8 };
}