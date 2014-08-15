using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace CodeComb.Entity
{
    [Table("forums")]
    public class Forum
    {
        [Column("id")]
        public int ID { get; set; }

        [Column("title")]
        public string Title { get; set; }

        [Column("sort")]
        public int Sort { get; set; }

        [Column("father_id")]
        [ForeignKey("Father")]
        public int? FatherID { get; set; }

        public virtual Forum Father { get; set; }

        public virtual ICollection<Topic> Topics { get; set; }

        public virtual ICollection<Forum> Children { get; set; }

        public override bool Equals(object obj)
        {
            var data = obj as Forum;
            if (data.ID == this.ID) return true;
            return false;
        }

        public override int GetHashCode()
        {
            return this.ID;
        }
    }
}
