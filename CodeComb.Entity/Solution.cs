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

        [Column("source")]
        public string Source { get; set; }

        [Column("language")]
        public int LanguageAsInt { get; set; }

        [Column("alogorithm_tags")]
        public string AlgorithmTagsAsString { get; set; }

        [NotMapped]
        public List<int> AlgorithmTags 
        {
            get 
            {
                var ids = AlgorithmTagsAsString.Split('|');
                var ret = new List<int>();
                foreach (var id in ids)
                {
                    ret.Add(Convert.ToInt32(id));
                }
                return ret;
            }
            set 
            {
                string dest = "";
                foreach (var id in value)
                {
                    dest += id.ToString() + "|";
                }
                dest = dest.TrimEnd('|');
                AlgorithmTagsAsString = dest;
            }
        }

        [NotMapped]
        public Language Language
        {
            get { return (Language)LanguageAsInt; }
            set { LanguageAsInt = (int)value; }
        }

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
