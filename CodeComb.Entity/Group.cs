using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace CodeComb.Entity
{
    [Table("groups")]
    public class Group
    {
        [Column("id")]
        public int ID { get; set; }

        [Column("title")]
        public string Title { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Column("join_method")]
        public int JoinMethodAsInt { get; set; }

        [Column("icon")]
        public byte[] Icon { get; set; }

        [NotMapped]
        public JoinMethod JoinMethod 
        {
            get { return (JoinMethod)JoinMethodAsInt; }
            set { JoinMethodAsInt = (int)value; }
        }

        public virtual ICollection<GroupMember> GroupMembers { get; set; }

        public virtual ICollection<GroupHomework> GroupHomeworks { get; set; }

        public virtual ICollection<GroupChat> GroupChats { get; set; }

        public virtual ICollection<GroupJoinApplication> GroupJoinApplications { get; set; }
        public override bool Equals(object obj)
        {
            var data = obj as Group;
            if (data.ID == this.ID) return true;
            return false;
        }

        public override int GetHashCode()
        {
            return this.ID;
        }
    }
    public enum JoinMethod { Everyone, Message, Forbidden };
}
