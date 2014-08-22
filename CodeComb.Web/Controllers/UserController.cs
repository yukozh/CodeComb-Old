using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.Mvc;
using CodeComb.Entity;
using CodeComb.Web.Models.View;

namespace CodeComb.Web.Controllers
{
    public class UserController : BaseController
    {
        //
        // GET: /User/
        #region 登录登出
        public ActionResult Login(string ReturnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                if (ReturnUrl != null)
                {
                    return Redirect(ReturnUrl);
                }
                else
                {
                    return Redirect("/");
                }
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(Login model, string ReturnUrl)
        {
            var user = (from u in DbContext.Users
                        where u.Username == model.Username
                        select u).SingleOrDefault();
            if (user == null)
            {
                ViewBag.UsernameInfo = "不存在这个用户！";
                return View();
            }
            else if (user.Password != Helpers.Security.SHA1(model.Password))
            {
                ViewBag.PasswordInfo = "密码错误！";
                return View();
            }
            else
            {
                FormsAuthentication.SetAuthCookie(model.Username, model.Remember);
                user.Online = true;
                DbContext.SaveChanges();
                if (ReturnUrl != null)
                {
                    return Redirect(ReturnUrl);
                }
                else
                {
                    return Redirect("/");
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            var user = (from u in DbContext.Users
                        where u.Username == User.Identity.Name
                        select u).Single();
            user.Online = false;
            DbContext.SaveChanges();
            FormsAuthentication.SignOut();
            return Redirect(Request.UrlReferrer.ToString());
        }
        #endregion

        #region 注册
        public ActionResult Register() 
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterStep1 model)
        {
            if ((from u in DbContext.Users where u.Email == model.Email select u).Count() > 0)
                return RedirectToAction("Message", "Shared", new { msg = "这个电子邮箱已经被注册过了，请返回更换电子邮箱后重新尝试注册！" });
            if (!Helpers.Regexes.Email.IsMatch(model.Email))
                return RedirectToAction("Message", "Shared", new { msg = "您输入的电子邮箱不合法，请返回更换电子邮箱后重新尝试注册！" });

            EmailVerification email_verification = (from ev in DbContext.EmailVerifications
                                                    where ev.Email == model.Email
                                                    select ev).SingleOrDefault();
            if (email_verification == null)
            {
                email_verification = new EmailVerification
                {
                    Email = model.Email,
                    Time = DateTime.Now.AddHours(2),
                    Token = Helpers.String.RandomString(16)
                };
                DbContext.EmailVerifications.Add(email_verification);
            }
            else
            { 
                email_verification.Time = DateTime.Now.AddHours(2);
                email_verification.Token = Helpers.String.RandomString(16);
            }
            DbContext.SaveChanges();

            Helpers.SMTP.Send(email_verification.Email, "Code Comb 用户注册邮箱验证", "验证码：" + email_verification.Token);
            return RedirectToAction("Message", "Shared", new { msg = "我们已经向" + email_verification.Email + "中发送了一封包含验证链接的电子邮件，请根据电子邮件中的提示进行下一步的注册。" });
        }

        public ActionResult RegisterDetail()
        {
            if(Session["Email"]==null)
                return RedirectToAction("Message", "Shared", new { msg = "非法访问。" });
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RegisterDetail(RegisterStep2 model)
        {
            if (Session["Email"] == null)
                return RedirectToAction("Message", "Shared", new { msg = "非法访问。" });
            var user = (from u in DbContext.Users
                        where u.Username == model.Username
                        select u).SingleOrDefault();
            var email = Session["Email"].ToString();
            if (user != null) return RedirectToAction("Message", "Shared", new { msg = "用户名已经存在，请返回更换用户名再试！" });
            DbContext.Users.Add(new User 
            {
                Username = model.Username,
                Password = Helpers.Security.SHA1(model.Password),
                Email = email,
                Nickname = model.Nickname,
                LastLoginTime = DateTime.Now,
                RegisterTime = DateTime.Now,
                Online = false,
                Role = UserRole.Member,
                Motto = "",
                CommonLanguage = Language.C,
                Gravatar = email
            });
            DbContext.SaveChanges();
            var email_verification = (from ev in DbContext.EmailVerifications
                                      where ev.Email == email
                                      select ev).Single();
            DbContext.EmailVerifications.Remove(email_verification);
            DbContext.SaveChanges();
            return RedirectToAction("Message", "Shared", new { msg = "感谢您注册成为Code Comb会员，您可以通过右上方登录按钮进行登录操作。" });
        }

        [HttpPost]
        public ActionResult CheckName(string Username)
        {
            var user = (from u in DbContext.Users
                        where u.Username == Username
                        select u).SingleOrDefault();
            if (user == null) return Content("OK");
            else return Content("NO");
        }
        #endregion

        #region 显示用户
        public ActionResult Index(int id)
        {
            var user = DbContext.Users.Find(id);
            return View(user);
        }
        #endregion

        #region 修改个人资料
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SetPassword(int id, string OldPassword, string NewPassword, string RepeatPassword)
        {
            var user = DbContext.Users.Find(id);
            if (user.ID != ViewBag.CurrentUser.ID && ViewBag.CurrentUser.Role < Entity.UserRole.Master)
                return RedirectToAction("Message", "Shared", new { msg = "您没有权限这样做！" });
            if(Helpers.Security.SHA1(OldPassword) != user.Password && ViewBag.CurrentUser.Role < Entity.UserRole.Master)
                return RedirectToAction("Message", "Shared", new { msg = "旧密码输入不正确！" });
            if(NewPassword !=RepeatPassword)
                return RedirectToAction("Message", "Shared", new { msg = "两次密码输入不一致！" });
            user.Password = Helpers.Security.SHA1(RepeatPassword);
            DbContext.SaveChanges();
            return RedirectToAction("Settings", "User", new { id = user.ID });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SetProfile(int id, string Nickname, string Gravatar, string Motto, int CommonLanguage)
        {
            var user = DbContext.Users.Find(id);
            if (user.ID != ViewBag.CurrentUser.ID && ViewBag.CurrentUser.Role < Entity.UserRole.Master)
                return RedirectToAction("Message", "Shared", new { msg = "您没有权限这样做！" });
            user.Nickname = Nickname;
            user.Gravatar = Gravatar;
            user.Motto = Motto;
            user.CommonLanguageAsInt = CommonLanguage;
            DbContext.SaveChanges();
            return RedirectToAction("Settings", "User", new { id = user.ID });
        }
        #endregion
    }
}