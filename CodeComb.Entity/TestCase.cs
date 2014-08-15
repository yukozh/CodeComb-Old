using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace CodeComb.Entity
{
    [Table("test_cases")]
    public class TestCase
    {
        [Column("id")]
        public int ID { get; set; }

        [Column("problem_id")]
        [ForeignKey("Problem")]
        public int ProblemID { get; set; }

        public virtual Problem Problem { get; set; }

        [Column("type")]
        public int TypeAsInt { get; set; }

        [NotMapped]
        public TestCaseType Type
        {
            get { return (TestCaseType)TypeAsInt; }
            set { TypeAsInt = (int)value; }
        }

        [Column("input")]
        public string Input { get; set; }

        [Column("output")]
        public string Output { get; set; }

        [Column("hash")]
        public string Hash { get; set; }

        public override bool Equals(object obj)
        {
            var data = obj as TestCase;
            if (data.ID == this.ID) return true;
            return false;
        }

        public override int GetHashCode()
        {
            return this.ID;
        }
    }
    public enum TestCaseType { Sample, Overall, Unilateralism, Custom };
}
