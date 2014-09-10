using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CodeComb.Web.Controllers
{
    public class PrivateMessageController : BaseController
    {
        //
        // GET: /PrivateMessage/
        [HttpGet]
        [Authorize]
        public ActionResult GetContacts(string ids)
        {
            var jss = new System.Web.Script.Serialization.JavaScriptSerializer();
            var _ids = jss.Deserialize<List<int>>(ids);
            var contacts = new List<Models.View.Contact>();
            var unread = (from m in DbContext.Messages
                          where m.Receiver.Username == User.Identity.Name
                          && !m.Read
                          select m.SenderID).ToList();
            _ids = _ids.Union(unread).ToList();
            foreach (var id in _ids.Distinct())
                try
                {
                    contacts.Add(new Models.View.Contact(DbContext.Users.Find(id), (int)ViewBag.CurrentUser.ID));
                }
                catch { }
            contacts = contacts.OrderByDescending(x => x.MessageCount).Take(50).ToList();
            return Json(contacts, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Authorize]
        public ActionResult GetChatRecords(int sender_id)
        {
            var receiver_id = (int)ViewBag.CurrentUser.ID;
            var messages = (from m in DbContext.Messages
                            where (m.SenderID == sender_id
                            && m.ReceiverID == receiver_id)
                            || (m.SenderID == receiver_id
                            && m.ReceiverID == sender_id)
                            orderby m.Time descending
                            select m).Take(50).ToList().OrderBy(x => x.Time);
            var chatrecords = new List<Models.View.ChatRecord>();
            foreach (var message in messages)
                chatrecords.Add(new Models.View.ChatRecord(message));
            foreach (var message in messages.Where(x => x.ReceiverID == receiver_id && !x.Read))
                message.Read = true;
            DbContext.SaveChanges();
            return Json(chatrecords, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Authorize]
        [ValidateInput(false)]
        public ActionResult PostMessage(int receiver_id, string content)
        {
            var message = new Entity.Message 
            { 
                Content = content,
                Read = false,
                ReceiverID = receiver_id,
                Time = DateTime.Now,
                SenderID = ViewBag.CurrentUser.ID
            };
            DbContext.Messages.Add(message);
            DbContext.SaveChanges();
            var name1 = DbContext.Users.Find(message.ReceiverID).Username;
            var name2 = DbContext.Users.Find(message.SenderID).Username;
            SignalR.CodeCombHub.context.Clients.Group(name1).onMessageReceived(message.ID);
            SignalR.CodeCombHub.context.Clients.Group(name2).onMessageReceived(message.ID);
            SignalR.MobileHub.context.Clients.Group(name1).onMessageReceived(new CodeComb.Models.WebAPI.Message 
            { 
                Content = message.Content,
                Time = message.Time,
                SenderID = message.SenderID,
                ReceiverID = message.ReceiverID
            });
            SignalR.MobileHub.context.Clients.Group(name2).onMessageReceived(new CodeComb.Models.WebAPI.Message
            {
                Content = message.Content,
                Time = message.Time,
                SenderID = message.SenderID,
                ReceiverID = message.ReceiverID
            });
            SignalR.MobileHub.PushTo(message.ReceiverID, message.Content);
            return Content("OK");
        }
	}
}