using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace CodeComb.Entity
{
    [Table("problems")]
    public class Problem
    {
        [Column("id")]
        public int ID { get; set; }

        [Column("contest_id")]
        [ForeignKey("Contest")]
        public int ContestID { get; set; }

        public virtual Contest Contest { get; set; }

        [Column("title")]
        public string Title { get; set; }

        [Column("background")]
        public string Background { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Column("input")]
        public string Input { get; set; }

        [Column("output")]
        public string Output { get; set; }

        [Column("hint")]
        public string Hint { get; set; }

        [Column("special_judge")]
        public string SpecialJudge { get; set; }

        [Column("special_judge_language")]
        public int SpecialJudgeLanguageAsInt { get; set; }

        [NotMapped]
        public Language SpecialJudgeLanguage
        {
            get { return (Language)SpecialJudgeLanguageAsInt; }
            set { SpecialJudgeLanguageAsInt = (int)value; }
        }

        [Column("range_checker")]
        public string RangeChecker { get; set; }

        [Column("range_checker_language")]
        public int RangeCheckerLanguageAsInt { get; set; }

        [NotMapped]
        public Language RangeCheckerLanguage
        {
            get { return (Language)RangeCheckerLanguageAsInt; }
            set { RangeCheckerLanguageAsInt = (int)value; }
        }

        [Column("standard_source")]
        public string StandardSource { get; set; }

        [Column("standard_source_language")]
        public int StandardSourceLanguageAsInt { get; set; }

        [NotMapped]
        public Language StandardSourceLanguage
        {
            get { return (Language)StandardSourceLanguageAsInt; }
            set { StandardSourceLanguageAsInt = (int)value; }
        }

        [Column("credit")]
        public int Credit { get; set; }

        [Column("difficulty")]
        public int Difficulty { get; set; }

        [Column("time_limit")]
        public int TimeLimit { get; set; }

        [Column("memory_limit")]
        public int MemoryLimit { get; set; }

        public IEnumerable<Status> GetContestStatuses()
        {
            return Statuses.Where(x => x.Time >= Contest.Begin && x.Time < Contest.End);
        }

        public int GetContestStatusesCount()
        {
            return Statuses.Where(x => x.Time >= Contest.Begin && x.Time < Contest.End).Count();
        }

        public virtual ICollection<TestCase> TestCases { get; set; }

        public virtual ICollection<Clarification> Clarifications { get; set; }

        public virtual ICollection<Status> Statuses { get; set; }

        public virtual ICollection<Solution> Solutions { get; set; }

        public virtual ICollection<Glance> Glances { get; set; }

        public virtual ICollection<Lock> Locks { get; set; }

        public override bool Equals(object obj)
        {
            var data = obj as Problem;
            if (data.ID == this.ID) return true;
            return false;
        }

        public override int GetHashCode()
        {
            return this.ID;
        }
    }
}
