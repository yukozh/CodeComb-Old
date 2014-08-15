using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace CodeComb.Entity
{
    [Table("contests")]
    public class Contest
    {
        [Column("id")]
        public int ID { get; set; }

        [Column("title")]
        public string Title { get; set; }

        [Column("content")]
        public string Content { get; set; }

        [Column("format")]
        public int FormatAsInt { get; set; }

        [NotMapped]
        public ContestFormat Format 
        {
            get { return (ContestFormat)FormatAsInt; }
            set { FormatAsInt = (int)value; }
        }

        [Column("allow_print_request")]
        public bool AllowPrintRequest { get; set; }

        [Column("allow_clarification")]
        public bool AllowClarification { get; set; }

        [Column("password")]
        public string Password { get; set; }

        [Column("ready")]
        public bool Ready { get; set; }

        [Column("begin")]
        public DateTime Begin { get; set; }

        [Column("rest_begin")]
        public DateTime? RestBegin { get; set; }

        [Column("rest_end")]
        public DateTime? RestEnd { get; set; }

        [Column("end")]
        public DateTime End { get; set; }

        [Column("rating_begin")]
        public int? RatingBegin { get; set; }

        [Column("rating_end")]
        public int? RatingEnd { get; set; }

        public virtual ICollection<Problem> Problems { get; set; }

        public virtual ICollection<Clarification> Clarifications { get; set; }

        public virtual ICollection<PrintRequest> PrintRequests { get; set; }

        public virtual ICollection<ContestManager> Managers { get; set; }

        public override bool Equals(object obj)
        {
            var data = obj as Contest;
            if (data.ID == this.ID) return true;
            return false;
        }

        public override int GetHashCode()
        {
            return this.ID;
        }
    }
    public enum ContestFormat { ACM, OI, Codeforces, TopCoder, OPJOI, CodeComb };
}
