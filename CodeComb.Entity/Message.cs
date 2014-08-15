using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace CodeComb.Entity
{
    [Table("messages")]
    public class Message
    {
        [Column("id")]
        public int ID { get; set; }

        [Column("sender_id")]
        [ForeignKey("Sender")]
        public int SenderID { get; set; }

        public virtual User Sender { get; set; }

        [Column("receiver_id")]
        [ForeignKey("Receiver")]
        public int ReceiverID { get; set; }

        public virtual User Receiver { get; set; }

        [Column("time")]
        public DateTime Time { get; set; }

        [Column("content")]
        public string Content { get; set; }

        [Column("read")]
        public bool Read { get; set; }
    }
}
