﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.Mvc;
using System.Text.RegularExpressions;
using CodeComb.Entity;
using CodeComb.Web.Models.View;

namespace CodeComb.Web.Controllers
{
    public class UserController : BaseController
    {
        //
        // GET: /User/
        #region 登录登出
        public ActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
                return Redirect("/");
            return View();
        }

        [HttpPost]
        public ActionResult LoginByToken(string token)
        {
            if (SignalR.CodeCombHub.LoginTokens[token] == null)
                return RedirectToAction("Message", "Shared", new { msg = "登录失败" });
            var user = DbContext.Users.Find((int)SignalR.CodeCombHub.LoginTokens[token]);
            SignalR.CodeCombHub.LoginTokens[token] = null;
            FormsAuthentication.SetAuthCookie(user.Username, true);
            user.Online = true;
            DbContext.SaveChanges();
            if (Request.UrlReferrer == null)
                return Redirect("/");
            else
                return Redirect(Request.UrlReferrer.ToString());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(Login model)
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
                if (Request.UrlReferrer == null)
                    return Redirect("/");
                else
                    return Redirect(Request.UrlReferrer.ToString());
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

            string strEmail = "<!DOCTYPE HTML><html><head><meta charset=\"UTF-8\"/><title>[Title]</title><style>p{margin:5px 0px}a{color:#1D76C7;text-decoration:none}.body{margin:0px;color:#333333;font-size:14px;font-family:Tahoma,'Segoe UI',Verdana,微软雅黑,'Microsoft YaHei',宋体;padding:30px;background-color:#F2F2F2}.container{box-shadow:rgba(0,0,0,0.3)0px 0px 15px;border-top-left-radius:5px;border-top-right-radius:5px;border-bottom-left-radius:5px;border-bottom-right-radius:5px;background-color:#FFF}.header{color:#FFF;padding:10px;line-height:200%;font-size:15px;border-top-left-radius:5px;border-top-right-radius:5px;border-bottom-left-radius:0px;border-bottom-right-radius:0px;border-bottom-width:3px;border-bottom-style:solid;border-bottom-color:#85CAEB;background-color:#3AA9DE}.problem-body{padding:30px}.link{padding:5px 10px;border-left-width:10px;border-left-style:solid;border-left-color:#E2EFFA;margin:20px 20px 20px 0px}.footer{color:#444;padding:10px;font-size:12px;border-top-width:1px;border-top-style:solid;border-top-color:#DDD;border-top-left-radius:0px;border-top-right-radius:0px;border-bottom-left-radius:5px;border-bottom-right-radius:5px;background-color:#F4F4F4}</style></head><body><div class=\"body\"><div class=\"container\"><div class=\"header\">新用户激活 - CodeComb</div><div class=\"body\"><p><strong>您好，欢迎您注册CodeComb帐号，请根据下面的提示信息继续完成注册操作。</strong></p><p>请点击下面的链接完成帐号工作，激活成功后将会自动登录CodeComb系统：</p><blockquote class=\"link\"><p><a href=\"http://www.codecomb.net/Verify/Register/" + email_verification.ID + "/" + email_verification.Token + "\" target=\"_blank\">http://www.codecomb.net/Verify/Register/" + email_verification.ID + "/" + email_verification.Token + "</a></p></blockquote><p>如果这次操作不是您本人的行为，请忽略本条邮件。</p></div><div class=\"footer\"><p>这封邮件由<a href=\"http://www.codecomb.net\"target=\"_blank\">CodeComb</a>自动发送，请勿直接回复。</p></div></div></div></body></html>";

            Helpers.SMTP.Send(email_verification.Email, "Code Comb 用户注册邮箱验证", strEmail);
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
            if (!Regex.IsMatch(model.Username, @"^\w+$") || Helpers.String.StringLen(model.Username) < 4 || Helpers.String.StringLen(model.Username) > 16)
                return RedirectToAction("Message", "Shared", new { msg = "用户名不合法，用户名长度必须为4~16个字符，同时只允许使用英文字母、数字和下划线" });
            if (Helpers.String.StringLen(model.Nickname) < 4 || Helpers.String.StringLen(model.Nickname) > 16)
                return RedirectToAction("Message", "Shared", new { msg = "昵称不合法，昵称长度必须为4~12个字符，中文计2个字符" });
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
            var ac_list = (from s in DbContext.Statuses
                           where s.UserID == id
                           && s.Problem.Contest.End < DateTime.Now
                           && s.ResultAsInt == (int)JudgeResult.Accepted
                           orderby s.ProblemID ascending
                           select s.ProblemID).Distinct().ToList();
            ac_list.Sort((a, b) => { return a - b; });
            ViewBag.AcceptedList = ac_list;
            return View(user);
        }
        #endregion

        #region 修改个人资料
        [Authorize]
        public ActionResult Settings(int id)
        {
            var user = DbContext.Users.Find(id);
            if (user.ID != ViewBag.CurrentUser.ID && ViewBag.CurrentUser.Role < Entity.UserRole.Master || ViewBag.CurrentUser.Role == UserRole.Temporary)
                return RedirectToAction("Message", "Shared", new { msg = "您没有权限这样做！" });
            return View(user);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SetPassword(int id, string OldPassword, string NewPassword, string RepeatPassword)
        {
            var user = DbContext.Users.Find(id);
            if (user.ID != ViewBag.CurrentUser.ID && ViewBag.CurrentUser.Role < Entity.UserRole.Master || ViewBag.CurrentUser.Role == UserRole.Temporary)
                return RedirectToAction("Message", "Shared", new { msg = "您没有权限这样做！" });
            if (ViewBag.CurrentUser.Role < Entity.UserRole.Master)
            {
                if (string.IsNullOrEmpty(OldPassword))
                    return RedirectToAction("Message", "Shared", new { msg = "请输入旧密码！" });
                if (Helpers.Security.SHA1(OldPassword) != user.Password && ViewBag.CurrentUser.Role < Entity.UserRole.Master)
                    return RedirectToAction("Message", "Shared", new { msg = "旧密码输入不正确！" });
            }
            if (NewPassword.Length < 4)
                return RedirectToAction("Message", "Shared", new { msg = "密码长度至少为4！" });
            if (NewPassword != RepeatPassword)
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
            if (user.ID != ViewBag.CurrentUser.ID && ViewBag.CurrentUser.Role < Entity.UserRole.Master || ViewBag.CurrentUser.Role == UserRole.Temporary)
                return RedirectToAction("Message", "Shared", new { msg = "您没有权限这样做！" });
            if (Helpers.String.StringLen(Nickname) < 4 || Helpers.String.StringLen(Nickname) > 12) return RedirectToAction("Message", "Shared", new { msg = "昵称长度应为4~16个字符！" });
            if (Motto.Length > 255) return RedirectToAction("Message", "Shared", new { msg = "个性签名长度应为0~255个字符！" });
            user.Nickname = Nickname;
            user.Gravatar = Gravatar;
            user.Motto = Motto;
            user.CommonLanguageAsInt = CommonLanguage;
            DbContext.SaveChanges();
            return RedirectToAction("Settings", "User", new { id = user.ID });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SetRole(int id, int role)
        {
            if (ViewBag.CurrentUser.Role < Entity.UserRole.Master)
                return RedirectToAction("Message", "Shared", new { msg="您无权执行本操作！" });
            if(role >= (int)UserRole.Master && ViewBag.CurrentUser.Role!=UserRole.Root)
                return RedirectToAction("Message", "Shared", new { msg = "您无权执行本操作！" });
            var user = DbContext.Users.Find(id);
            user.RoleAsInt = role;
            DbContext.SaveChanges();
            return RedirectToAction("Settings", "User", new { id = id });
        }
        #endregion

        #region 创建比赛
        [Authorize]
        public ActionResult Contest(int id)
        {
            var user = DbContext.Users.Find(id);
            ViewBag.Nickname = user.Nickname;
            var contests = (from c in DbContext.Contests
                            where c.Managers.Where(x => x.UserID == user.ID).Count() > 0
                            orderby c.Begin descending
                            select c).ToList();
            return View(contests);
        }
        #endregion
    }
}