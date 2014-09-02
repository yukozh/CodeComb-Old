using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations.Schema;

namespace CodeComb.Entity
{
    [Table("users")]
    public class User
    {
        [Column("id")]
        public int ID { get; set; }

        [Column("username")]
        public string Username { get; set; }

        [Column("nickname")]
        public string Nickname { get; set; }

        [Column("password")]
        public string Password { get; set; }

        [Column("gravatar")]
        public string Gravatar { get; set; }

        [Column("email")]
        public string Email { get; set; }

        [Column("role")]
        public int RoleAsInt { get; set; }

        [NotMapped]
        public UserRole Role 
        {
            get { return (UserRole)RoleAsInt; }
            set { RoleAsInt = (int)value; }
        }

        [Column("motto")]
        public string Motto { get; set; }

        [Column("register_time")]
        public DateTime RegisterTime { get; set; }

        [Column("last_login_time")]
        public DateTime LastLoginTime { get; set; }

        [Column("online")]
        public bool Online { get; set; }

        [Column("common_language")]
        public int CommonLanguageAsInt { get; set; }

        [NotMapped]
        public Language CommonLanguage 
        {
            get { return (Language)CommonLanguageAsInt; }
            set { CommonLanguageAsInt = (int)value; }
        }

        public virtual ICollection<Rating> Ratings { get; set; }

        public virtual ICollection<Token> Tokens { get; set; }

        public virtual ICollection<App> Apps { get; set; }

        public virtual ICollection<Solution> Solutions { get; set; }

        public virtual ICollection<Message> PMSent { get; set; }

        public virtual ICollection<Message> PMReceived { get; set; }
        public virtual ICollection<DeviceToken> DeviceTokens { get; set; }

        public override bool Equals(object obj)
        {
            var data = obj as User;
            if (data.ID == this.ID) return true;
            return false;
        }

        public override int GetHashCode()
        {
            return this.ID;
        }
    }
    public enum UserRole { Temporary, Member, VIP, Master, Root };
}
