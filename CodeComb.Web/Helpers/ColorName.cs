using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CodeComb.Entity;
using CodeComb.Web.Models.View;

namespace CodeComb.Web.Helpers
{
    public static class ColorName
    {
        public static string GetColor(int Ratings)
        {
            if (Ratings < 1000) return "inherit";
            if (Ratings < 1500) return "green";
            if (Ratings < 1700) return "DodgerBlue";
            if (Ratings < 2000) return "purple ";
            if (Ratings < 2400) return "orange";
            return "red";
        }
        public static string GetLevel(int Ratings)
        {
            if (Ratings < 1500) return "R";
            if (Ratings < 1700) return "L3";
            if (Ratings < 2000) return "L2";
            if (Ratings < 2400) return "L1";
            return "S";
        }

        public static string GetNicknameHtml(string Nickname, int Ratings)
        {
            return string.Format("<span style='color:{0}'>{1}</span>", GetColor(Ratings), HttpUtility.HtmlEncode(Nickname));
        }

        public static string GetNicknameHtml(string Nickname, int Ratings, string @class)
        {
            return string.Format("<span style='color:{0}' class='{1}'>{2}</span>", GetColor(Ratings), @class, HttpUtility.HtmlEncode(Nickname));
        }
    }
}