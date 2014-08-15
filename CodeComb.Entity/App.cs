using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace CodeComb.Entity
{
    [Table("apps")]
    public class App
    {
        [Column("id")]
        public int ID { get; set; }

        [Column("user_id")]
        [ForeignKey("User")]
        public int UserID { get; set; }

        public User User { get; set; }

        [Column("title")]
        public string Title { get; set; }

        [Column("InterfaceAddress")]
        public string InterfaceAddress { get; set; }

        [Column("status")]
        public int StatusAsInt { get; set; }

        [NotMapped]
        public AppStatus Status
        {
            get { return (AppStatus)StatusAsInt; }
            set { StatusAsInt = (int)value; }
        }

        [Column("expire")]
        public DateTime Expire { get; set; }

        [Column("client_secret")]
        public string ClientSecret { get; set; }

        [Column("access_token")]
        public string AccessToken { get; set; }

        public override bool Equals(object obj)
        {
            var data = obj as App;
            if (data.ID == this.ID) return true;
            return false;
        }

        public override int GetHashCode()
        {
            return this.ID;
        }
    }
    public enum AppStatus { Verifying, Normal, Expired };
}
