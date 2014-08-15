using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace CodeComb.Entity
{
    [Table("judge_tasks")]
    public class JudgeTask
    {
        [Column("id")]
        public int ID { get; set; }

        [Column("test_case_id")]
        [ForeignKey("TestCase")]
        public int TestCaseID { get; set; }

        public virtual TestCase TestCase { get; set; }

        [Column("time_usage")]
        public int TimeUsage { get; set; }

        [Column("memory_usage")]
        public int MemoryUsage { get; set; }

        [Column("status_id")]
        [ForeignKey("Status")]
        public int StatusID { get; set; }

        public virtual Status Status { get; set; }

        [Column("result")]
        public int ResultAsInt { get; set; }

        [NotMapped]
        public JudgeResult Result 
        {
            get { return (JudgeResult)ResultAsInt; }
            set { ResultAsInt = (int)value; }
        }

        [Column("hint")]
        public string Hint { get; set; }

        public override bool Equals(object obj)
        {
            var data = obj as JudgeTask;
            if (data.ID == this.ID) return true;
            return false;
        }

        public override int GetHashCode()
        {
            return this.ID;
        }
    }
}
