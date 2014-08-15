using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace CodeComb.Entity
{
    [Table("hacks")]
    public class Hack
    {
        [Column("id")]
        public int ID { get; set; }

        [Column("status_id")]
        public int StatusID { get; set; }

        public virtual Status Status { get; set; }

        [Column("hacker_id")]
        [ForeignKey("Hacker")]
        public int HackerID { get; set; }

        public virtual User Hacker { get; set; }

        [Column("defender_id")]
        [ForeignKey("Defender")]
        public int DefenderID { get; set; }

        public virtual User Defender { get; set; }

        [Column("status")]
        public int ResultAsInt { get; set; }

        [NotMapped]
        public HackResult Result 
        {
            get { return (HackResult)ResultAsInt; }
            set { ResultAsInt = (int)value; }
        }

        public override bool Equals(object obj)
        {
            var data = obj as Hack;
            if (data.ID == this.ID) return true;
            return false;
        }

        public override int GetHashCode()
        {
            return this.ID;
        }
    }
}
