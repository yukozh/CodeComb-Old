using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace CodeComb.Entity
{
    [Table("algorithm_tags")]
    public class AlgorithmTag
    {
        [Column("id")]
        public int ID { get; set; }

        [Column("title")]
        public string Title { get; set; }

        [Column("father_id")]
        public int? FatherID { get; set; }

        public virtual AlgorithmTag Father { get; set; }

        public virtual ICollection<AlgorithmTag> Children { get; set; }

        public override bool Equals(object obj)
        {
            var data = obj as AlgorithmTag;
            if (data.ID == this.ID) return true;
            return false;
        }

        public override int GetHashCode()
        {
            return this.ID;
        }
    }
}
