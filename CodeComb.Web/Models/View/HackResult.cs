using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CodeComb.Web.Models.View
{
    public class HackResult
    {
        public HackResult() { }
        public HackResult(Entity.Hack hack) 
        {
            ID = hack.ID;
            HackerID = hack.HackerID;
            HackerName = Helpers.ColorName.GetNicknameHtml(hack.Hacker.Nickname, hack.Hacker.Ratings.Sum(x=>x.Credit)+1500);
            HackerGravatar = Helpers.Gravatar.GetAvatarURL(hack.Hacker.Gravatar, 180);
            DefenderID = hack.DefenderID;
            DefenderName = Helpers.ColorName.GetNicknameHtml(hack.Defender.Nickname, hack.Defender.Ratings.Sum(x => x.Credit) + 1500);
            DefenderGravatar = Helpers.Gravatar.GetAvatarURL(hack.Defender.Gravatar, 180);
            Result = Entity.CommonEnums.HackResultDisplay[hack.ResultAsInt];
            if (hack.Result == Entity.HackResult.Success)
                Css = "status-text-ac";
            else if (hack.Result == Entity.HackResult.BadData || hack.Result == Entity.HackResult.DatamakerError || hack.Result == Entity.HackResult.SystemError)
                Css = "status-text-tle";
            else if (hack.Result == Entity.HackResult.Failure)
                Css = "status-text-wa";
            else
                Css = "status-text-pending";
            ProblemID = hack.Status.ProblemID;
            ProblemTitle = hack.Status.Problem.Title;
            StatusID = hack.StatusID;
        }
        public int ID { get; set; }
        public int HackerID { get; set; }
        public string HackerName { get; set; }
        public int DefenderID { get; set; }
        public string DefenderName { get; set; }
        public string Result { get; set; }
        public string Css { get; set; }
        public int StatusID { get; set; }
        public string ProblemTitle { get; set; }
        public int ProblemID { get; set; }
        public string HackerGravatar { get; set; }
        public string DefenderGravatar { get; set; }
    }
}