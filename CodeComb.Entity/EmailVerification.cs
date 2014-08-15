using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace CodeComb.Entity
{
    [Table("email_verifications")]
    public class EmailVerification
    {
        [Column("id")]
        public int ID { get; set; }

        [Column("email")]
        public string Email { get; set; }

        [Column("token")]
        public string Token { get; set; }

        [Column("time")]
        public DateTime Time { get; set; }

        public override bool Equals(object obj)
        {
            var data = obj as EmailVerification;
            if (data.ID == this.ID) return true;
            return false;
        }

        public override int GetHashCode()
        {
            return this.ID;
        }
    }
}
