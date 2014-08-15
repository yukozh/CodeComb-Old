using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace CodeComb.Entity
{
    [Table("clarifications")]
    public class Clarification
    {
        [Column("id")]
        public int ID { get; set; }

        [Column("contest_id")]
        [ForeignKey("Contest")]
        public int ContestID { get; set; }

        public virtual Contest Contest { get; set; }

        [Column("problem_id")]
        [ForeignKey("Problem")]
        public int? ProblemID { get; set; }

        public virtual Problem Problem { get; set; }

        [Column("user_id")]
        [ForeignKey("User")]
        public int UserID { get; set; }

        public virtual User User { get; set; }

        [Column("time")]
        public DateTime Time { get; set; }

        [Column("question")]
        public string Question { get; set; }

        [Column("answer")]
        public string Answer { get; set; }

        [Column("status")]
        public int StatusAsInt { get; set; }

        [NotMapped]
        public ClarificationStatus Status 
        {
            get { return (ClarificationStatus)StatusAsInt; }
            set { StatusAsInt = (int)value; }
        }

        public override bool Equals(object obj)
        {
            var data = obj as Clarification;
            if (data.ID == this.ID) return true;
            return false;
        }

        public override int GetHashCode()
        {
            return this.ID;
        }
    }
    public enum ClarificationStatus { Pending, Private, BroadCast };
}
