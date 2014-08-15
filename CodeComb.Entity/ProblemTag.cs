using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace CodeComb.Entity
{
    [Table("problem_tags")]
    public class ProblemTag
    {
        [Column("id")]
        public int ID { get; set; }

        [Column("problem_id")]
        [ForeignKey("Problem")]
        public int ProblemID { get; set; }

        public virtual Problem Problem { get; set; }

        [Column("algorithm_id")]
        [ForeignKey("AlgorithmTag")]
        public int AlgorithmTagID { get; set; }

        public virtual AlgorithmTag AlgorithmTag { get; set; }

        public override bool Equals(object obj)
        {
            var data = obj as ProblemTag;
            if (data.ID == this.ID) return true;
            return false;
        }

        public override int GetHashCode()
        {
            return this.ID;
        }
    }
}
