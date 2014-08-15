using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CodeComb.Web.Controllers
{
    public class VerifyController : BaseController
    {
        //
        // GET: /Verify/
        public ActionResult Register(int id, string token)
        {
            var email_verification = DbContext.EmailVerifications.Find(id);
            if (email_verification == null)
                return RedirectToAction("Message", "Shared", new { msg = "对不起，您的验证链接不合法，无法继续进行注册！" });
            if (DateTime.Now > email_verification.Time)
                return RedirectToAction("Message", "Shared", new { msg = "对不起，您的验证信已经过期，请重新注册以验证电子邮箱！" });
            if (email_verification.Token != token)
                return RedirectToAction("Message", "Shared", new { msg = "对不起，您的验证码不正确，请返回邮箱重新打开验证链接或重新注册！" });
            Session["Email"] = email_verification.Email;
            return RedirectToAction("RegisterDetail", "User", null);
        }
	}
}