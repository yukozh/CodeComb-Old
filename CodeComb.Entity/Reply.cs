using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace CodeComb.Entity
{
    [Table("replies")]
    public class Reply
    {
        [Column("id")]
        public int ID { get; set; }

        [Column("topic_id")]
        [ForeignKey("Topic")]
        public int TopicID { get; set; }

        public virtual Topic Topic { get; set; }

        [Column("user_id")]
        [ForeignKey("User")]
        public int UserID { get; set; }

        public virtual User User { get; set; }

        [Column("content")]
        public string Content { get; set; }

        [Column("time")]
        public DateTime Time { get; set; }

        [Column("father_id")]
        [ForeignKey("Father")]
        public int? FatherID { get; set; }

        public virtual Reply Father { get; set; }

        public virtual ICollection<Reply> Replies { get; set; }

        public override bool Equals(object obj)
        {
            var data = obj as Reply;
            if (data.ID == this.ID) return true;
            return false;
        }

        public override int GetHashCode()
        {
            return this.ID;
        }
    }
}
