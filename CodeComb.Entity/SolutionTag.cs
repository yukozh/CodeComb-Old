using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace CodeComb.Entity
{
    [Table("solution_tags")]
    public class SolutionTag
    {
        [Column("id")]
        public int ID { get; set; }

        [Column("solution_id")]
        [ForeignKey("Solution")]
        public int SolutionID { get; set; }

        public virtual Solution Solution { get; set; }

        [Column("algorithm_id")]
        [ForeignKey("AlgorithmTag")]
        public int AlgorithmTagID { get; set; }

        public virtual AlgorithmTag AlgorithmTag { get; set; }

        public override bool Equals(object obj)
        {
            var data = obj as SolutionTag;
            if (data.ID == this.ID) return true;
            return false;
        }

        public override int GetHashCode()
        {
            return this.ID;
        }
    }
}
