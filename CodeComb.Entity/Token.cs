using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace CodeComb.Entity
{
    [Table("tokens")]
    public class Token
    {
        [Column("id")]
        public int ID { get; set; }

        [Column("user_id")]
        [ForeignKey("User")]
        public int UserID { get; set; }

        public virtual User User { get; set; }

        [Column("expire")]
        public DateTime Expire { get; set; }

        [Column("app_id")]
        [ForeignKey("App")]
        public int AppID { get; set; }

        public virtual App App { get; set; }

        [Column("access_token")]
        public string AccessToken { get; set; }

        [Column("request_token")]
        public string RequestToken { get; set; }

        public override bool Equals(object obj)
        {
            var data = obj as Token;
            if (data.ID == this.ID) return true;
            return false;
        }

        public override int GetHashCode()
        {
            return this.ID;
        }
    }
}
