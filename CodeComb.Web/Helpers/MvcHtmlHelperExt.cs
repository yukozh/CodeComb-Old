using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CodeComb.Web.Helpers
{
    public static class MvcHtmlHelperExt
    {
        public static MvcHtmlString ToContestStatus<TModel>(this HtmlHelper<TModel> self, DateTime begin, DateTime? rest_begin, DateTime?rest_end,DateTime end)
        {
            return new MvcHtmlString(Time.ToContestStatus(begin, rest_begin, rest_end, end));
        }

        public static MvcHtmlString ToTimeTip<TModel>(this HtmlHelper<TModel> self, DateTime time)
        {
            return new MvcHtmlString(Time.ToTimeTip(time));
        }

        public static MvcHtmlString ToTimeLength<TModel>(this HtmlHelper<TModel> self, DateTime time1, DateTime time2)
        {
            return new MvcHtmlString(Time.ToTimeLength(time1,time2));
        }

        public static MvcHtmlString ToVagueTimeLength<TModel>(this HtmlHelper<TModel> self, DateTime time1, DateTime time2)
        {
            return new MvcHtmlString(Time.ToVagueTimeLength(time1, time2));
        }

        public static MvcHtmlString Gravatar<TModel>(this HtmlHelper<TModel> self, string email, int size)
        {
            string url = Helpers.Gravatar.GetAvatarURL(email, size);
            string tag = string.Format("<img src='{0}' style='width:{1}px;height:{1}px;' />", HttpUtility.HtmlAttributeEncode(url), size);
            return new MvcHtmlString(tag);
        }

        public static MvcHtmlString Gravatar<TModel>(this HtmlHelper<TModel> self, string email, int size, string @class)
        {
            string url = Helpers.Gravatar.GetAvatarURL(email, size);
            string tag = string.Format("<img src='{0}' class='{1}' />", HttpUtility.HtmlAttributeEncode(url), @class);
            return new MvcHtmlString(tag);
        }

        public static MvcHtmlString Nickname<TModel>(this HtmlHelper<TModel> self, string nickname, int ratings)
        {
            return new MvcHtmlString(ColorName.GetNicknameHtml(nickname, ratings));
        }

        public static MvcHtmlString Nickname<TModel>(this HtmlHelper<TModel> self, string nickname, int ratings, string @class)
        {
            return new MvcHtmlString(ColorName.GetNicknameHtml(nickname, ratings, @class));
        }

        public static MvcHtmlString Markdown<TModel>(this HtmlHelper<TModel> self, string mdtxt)
        {
            return new MvcHtmlString(Helpers.Markdown.ToHtml(mdtxt));
        }

        public static MvcHtmlString Sanitized<TModel>(this HtmlHelper<TModel> self, string html)
        {
            if (html == null) return new MvcHtmlString("");
            return new MvcHtmlString(HtmlFilter.Instance.SanitizeHtml(html));
        }

        public static MvcHtmlString ToTimeStamp<TModel>(this HtmlHelper<TModel> self, DateTime time)
        {
            return new MvcHtmlString(Helpers.Time.ToTimeStamp(time).ToString());
        }

        public static MvcHtmlString ToLevelStr<TModel>(this HtmlHelper<TModel> self, int credits)
        {
            return new MvcHtmlString(Helpers.ColorName.GetLevel(credits));
        }
    }
}