using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace CodeComb.Entity
{
    [Table("solutions")]
    public class Solution
    {
        [Column("id")]
        public int ID { get; set; }

        [Column("problem_id")]
        [ForeignKey("Problem")]
        public int ProblemID { get; set; }

        public virtual Problem Problem { get; set; }

        [Column("user_id")]
        [ForeignKey("User")]
        public int UserID { get; set; }

        public virtual User User { get; set; }

        [Column("title")]
        public string Title { get; set; }

        [Column("content")]
        public string Content { get; set; }

        [Column("code")]
        public string Code { get; set; }

        [Column("language")]
        public int LanguageAsInt { get; set; }

        [NotMapped]
        public Language Language
        {
            get { return (Language)LanguageAsInt; }
            set { LanguageAsInt = (int)value; }
        }

        public virtual ICollection<SolutionTag> SolutionTags { get; set; }

        public override bool Equals(object obj)
        {
            var data = obj as Solution;
            if (data.ID == this.ID) return true;
            return false;
        }

        public override int GetHashCode()
        {
            return this.ID;
        }
    }
}
