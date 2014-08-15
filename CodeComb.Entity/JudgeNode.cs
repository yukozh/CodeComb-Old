using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace CodeComb.Entity
{
    [Table("judge_nodes")]
    public class JudgeNode
    {
        [Column("id")]
        public int ID { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("interface_address")]
        public string InterfaceAddress { get; set; }

        [Column("token")]
        public string Token { get; set; }

        [Column("status")]
        public int StatusAsInt { get; set; }

        [NotMapped]
        public JudgeNodeStatus Status 
        {
            get { return (JudgeNodeStatus)StatusAsInt; }
            set { StatusAsInt = (int)value; }
        }

        public override bool Equals(object obj)
        {
            var data = obj as JudgeNode;
            if (data.ID == this.ID) return true;
            return false;
        }

        public override int GetHashCode()
        {
            return this.ID;
        }
    }
    public enum JudgeNodeStatus { Online, Offline };
}
