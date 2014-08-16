using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace CodeComb.Entity
{
    [Table("statuses")]
    public class Status
    {
        public static JudgeResult[] FreeResults = { JudgeResult.CompileError, JudgeResult.SystemError };
        [Column("id")]
        public int ID { get; set; }

        [Column("user_id")]
        [ForeignKey("User")]
        public int UserID { get; set; }

        public virtual User User { get; set; }

        [Column("time")]
        public DateTime Time { get; set; }

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

        [Column("status")]
        public int ResultAsInt { get; set; }

        [Column("public")]
        public bool Public { get; set; }

        [NotMapped]
        public JudgeResult Result 
        {
            get { return (JudgeResult)ResultAsInt; }
            set { ResultAsInt = (int)value; }
        }

        [Column("problem_id")]
        [ForeignKey("Problem")]
        public int ProblemID { get; set; }

        public virtual Problem Problem { get; set; }

        public string GetStatusAsString()
        {
            return CommonEnums.JudgeResultDisplay[ResultAsInt];
        }

        public virtual ICollection<JudgeTask> JudgeTasks { get; set; }

        public virtual ICollection<Hack> Hacks { get; set; }

        public override bool Equals(object obj)
        {
            var data = obj as Status;
            if (data.ID == this.ID) return true;
            return false;
        }

        public override int GetHashCode()
        {
            return this.ID;
        }
    }
}
