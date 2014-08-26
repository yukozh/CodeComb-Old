using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeComb.Node
{
    public class Identity
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public System.Security.SecureString secPassword
        {
            get
            {
                System.Security.SecureString pwd = new System.Security.SecureString();
                foreach (char c in Password)
                {
                    pwd.AppendChar(c);
                }
                return pwd;
            }
        }
    }
}
