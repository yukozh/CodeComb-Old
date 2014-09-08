using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace CodeComb.Entity
{
    [Table("join_logs")]
    public class JoinLog
    {
        [Column("id")]
        public int ID { get; set; }

        [Column("user_id")]
        public int UserID { get; set; }

        public virtual User User { get; set; }

        [Column("contest_id")]
        public int ContestID { get; set; }

        public virtual Contest Contest { get; set; }
    }
}
