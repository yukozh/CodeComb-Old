using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace CodeComb.Entity
{
    [Table("glances")]
    public class Glance
    {
        [Column("id")]
        public int ID { get; set; }

        [Column("problem_id")]
        [ForeignKey("Problem")]
        public int ProblemID { get; set; }

        public virtual Problem Problem { get; set; }

        [Column("user_id")]
        public int UserID { get; set; }

        public virtual User User { get; set; }

        [Column("time")]
        public DateTime Time { get; set; }

        public override bool Equals(object obj)
        {
            var data = obj as Glance;
            if (data.ID == this.ID) return true;
            return false;
        }

        public override int GetHashCode()
        {
            return this.ID;
        }
    }
}
