using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using PushSharp;
using PushSharp.Apple;
using PushSharp.Core;

namespace CodeComb.Web.SignalR
{
    public class MobileHub : Hub
    {
        public static List<MobileUser> Users = new List<MobileUser>();
        public Database.DB DbContext = new Database.DB();
        public static Microsoft.AspNet.SignalR.IHubContext context = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MobileHub>();

        #region Static Methods
        private Entity.User GetUser()
        {
            var user = (from dt in DbContext.DeviceTokens
                        where dt.Token == Context.ConnectionId
                        select dt.User).FirstOrDefault();
            return user;
        }
        private Entity.UserRole? CheckRole()
        {
            var user = GetUser();
            if (user == null) return null;
            return user.Role;
        }
        public static void PushTo(int UserID, string Message)
        {
            Database.DB DbContext = new Database.DB();
            var devicetokens =(from dt in DbContext.DeviceTokens
                                   where dt.UserID == UserID
                                   && dt.TypeAsInt == (int)Entity.DeviceType.iOS
                                   select dt).ToList();
            foreach(var dt in devicetokens)
                iOS_PushTo(dt.Token, Message);
        }
        public static void PushToAll(string Message)
        {
            Database.DB DbContext = new Database.DB();
            var devicetokens = (from dt in DbContext.DeviceTokens
                                where dt.TypeAsInt == (int)Entity.DeviceType.iOS
                                select dt).ToList();
            foreach (var dt in devicetokens)
                iOS_PushTo(dt.Token, Message);
        }
        public static void iOS_PushTo(string device, string msg)
        {
            //Helpers.Push.push.QueueNotification(
            //    new AppleNotification()
            //        .ForDeviceToken(device)
            //        .WithAlert(msg)
            //        .WithBadge(1)
            //        .WithSound("default")
            //);
        }
        #endregion
        public CodeComb.Models.WebAPI.Base RegisterSignalR(string token)
        {
            var user = (from d in DbContext.DeviceTokens
                        where d.Token == token
                        select d.User).SingleOrDefault();
            if (user == null)
                return new CodeComb.Models.WebAPI.Base
                {
                    Code = 500,
                    IsSuccess = false,
                    Info = "AccessToken不正确"
                };
            Groups.Add(Context.ConnectionId, user.Username);
            return new CodeComb.Models.WebAPI.Base
            {
                Code = 0,
                IsSuccess = true,
                Info = ""
            };
        }
    }
}