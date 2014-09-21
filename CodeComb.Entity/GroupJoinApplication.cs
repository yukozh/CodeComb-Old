using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace CodeComb.Entity
{
    [Table("group_join_applications")]
    public class GroupJoinApplication
    {
        [Column("id")]
        public int ID { get; set; }

        [Column("group_id")]
        [ForeignKey("Group")]
        public int GroupID { get; set; }

        public virtual Group Group { get; set; }

        [Column("user_id")]
        [ForeignKey("User")]
        public int UserID { get; set; }

        public virtual User User { get; set; }

        [Column("status")]
        public int StatusAsInt { get; set; }

        [NotMapped]
        public GroupJoinStatus Status 
        {
            get { return (GroupJoinStatus)StatusAsInt; }
            set { StatusAsInt = (int)value; }
        }
        
        [Column("response")]
        public string Response { get; set; }

        [Column("message")]
        public string Message { get; set; }

        [Column("time")]
        public DateTime Time { get; set; }

        public override bool Equals(object obj)
        {
            var data = obj as GroupJoinApplication;
            if (data.ID == this.ID) return true;
            return false;
        }

        public override int GetHashCode()
        {
            return this.ID;
        }
    }
    public enum GroupJoinStatus { Pending, Accepted, Rejected };
}
