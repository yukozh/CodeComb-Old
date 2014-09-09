using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CodeComb.Web.Models.View
{
    public class Hack
    {
        public Hack() {}
        public Hack(CodeComb.Entity.Hack hack) 
        {
            ID = hack.ID;
            Problem = hack.Status.Problem.Title;
            StatusID = hack.StatusID;
            Nickname = Helpers.ColorName.GetNicknameHtml(hack.Hacker.Nickname, hack.Hacker.Ratings.Sum(x => x.Credit) + 1500);
            Defender = Helpers.ColorName.GetNicknameHtml(hack.Defender.Nickname, hack.Defender.Ratings.Sum(x => x.Credit) + 1500);
            Gravatar = Helpers.Gravatar.GetAvatarURL(hack.Hacker.Gravatar, 180);
            Time = Helpers.Time.ToTimeTip(hack.Time);
            Result = hack.Result.ToString();
            switch (hack.Result)
            { 
                case Entity.HackResult.Pending:
                case Entity.HackResult.Running:
                    Css = "status-text-running";
                    break;
                case Entity.HackResult.SystemError:
                case Entity.HackResult.BadData:
                    Css = "status-text-ce";
                    break;
                case Entity.HackResult.Success:
                    Css = "status-text-ac";
                    break;
                case Entity.HackResult.Failure:
                    Css = "status-text-wa";
                    break;
                default: break;
            }
        }
        public int ID { get; set; }
        public string Problem { get; set; }
        public int StatusID { get; set; }
        public string Nickname { get; set; }
        public string Defender { get; set; }
        public string Result { get; set; }
        public string Css { get; set; }
        public string Gravatar { get; set; }
        public string Time { get; set; }
    }
}