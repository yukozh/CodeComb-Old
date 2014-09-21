using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace CodeComb.Entity
{
    [Table("group_homework_problems")]
    public class GroupHomeworkProblem
    {
        [Column("id")]
        public int ID { get; set; }

        [Column("problem_id")]
        [ForeignKey("Problem")]
        public int ProblemID { get; set; }

        public virtual Problem Problem { get; set; }

        [Column("group_homework_id")]
        [ForeignKey("GroupHomework")]
        public int GroupHomeworkID { get; set; }

        public virtual GroupHomework GroupHomework { get; set; }

        [Column("priority")]
        public int Priority { get; set; }

        public override bool Equals(object obj)
        {
            var data = obj as GroupHomeworkProblem;
            if (data.ID == this.ID) return true;
            return false;
        }

        public override int GetHashCode()
        {
            return this.ID;
        }
    }
}
