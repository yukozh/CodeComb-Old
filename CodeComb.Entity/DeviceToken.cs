using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace CodeComb.Entity
{
    [Table("device_tokens")]
    public class DeviceToken
    {
        [Column("id")]
        public int ID { get; set; }

        [Column("user_id")]
        [ForeignKey("User")]
        public int UserID { get; set; }

        public virtual User User { get; set; }

        [Column("token")]
        public string Token { get; set; }

        [Column("type")]
        public int TypeAsInt { get; set; }

        [NotMapped]
        public DeviceType Type
        {
            get { return (DeviceType)TypeAsInt; }
            set { TypeAsInt = (int)value; }
        }
    }
    public enum DeviceType { SignalR, iOS, Android, WindowsPhone, Windows, System };
}
