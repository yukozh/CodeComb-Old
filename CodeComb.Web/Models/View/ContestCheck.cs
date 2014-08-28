using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CodeComb.Web.Models.View
{
    public class ContestCheck
    {
        public string Problem { get; set; }
        public string Result { get; set; }
        public string Css 
        {
            get 
            {
                switch (Status)
                {
                    case CheckStatus.Error:
                        return "status-text-wa";
                    case CheckStatus.Warning:
                        return "status-text-ce";
                    case CheckStatus.Info:
                        return "status-text-running";
                    default: return "";
                }
            }
        }
        public CheckStatus Status { get; set; }
    }
    public enum CheckStatus { Info, Warning, Error };
}