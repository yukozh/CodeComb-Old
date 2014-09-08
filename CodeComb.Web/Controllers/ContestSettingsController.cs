using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CodeComb.Web.Controllers
{
    public class ContestSettingsController : ContestBaseController
    {
        //
        // GET: /ContestSettings/
        [Authorize]
        public ActionResult General(int id)
        {
            if (!ViewBag.IsMaster)
                return RedirectToAction("Message", "Shared", new { msg = "您没有权限对本场比赛进行任何修改！" });
            ViewBag.ContestFormats = Enum.GetNames(typeof(Entity.ContestFormat));
            var contest = DbContext.Contests.Find(id);
            return View(contest);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        [ValidateInput(false)]
        public ActionResult General(int id, Entity.Contest model)
        {
            if (!ViewBag.IsMaster)
                return RedirectToAction("Message", "Shared", new { msg = "您没有权限对本场比赛进行任何修改！" });
            var contest = DbContext.Contests.Find(id);
            if (contest.Password != model.Password)
            {
                var joinlogs = (from j in DbContext.JoinLogs
                                where j.ContestID == id
                                select j).ToList();
                foreach (var joinlog in joinlogs)
                    DbContext.JoinLogs.Remove(joinlog);
                DbContext.SaveChanges();
            }
            contest.Begin = model.Begin;
            contest.RestBegin = model.RestBegin;
            contest.RestEnd = model.RestEnd;
            contest.End = model.End;
            contest.Title = model.Title;
            contest.Password = model.Password;
            contest.Content = model.Content;
            contest.Format = model.Format;
            contest.RatingBegin = model.RatingBegin;
            contest.RatingEnd = model.RatingEnd;
            contest.AllowClarification = model.AllowClarification;
            contest.AllowPrintRequest = model.AllowPrintRequest;
            if (model.RatingBegin > model.RatingEnd)
            {
                return RedirectToAction("Message", "Shared", new { msg = "能力值不合法" });
            }
            if (((Entity.User)ViewBag.CurrentUser).Role >= Entity.UserRole.Master)
            {
                contest.Ready = model.Ready;//这里只有系统管理员才可以设置这个选项，比赛创建者、管理员均无权
            }
            if (contest.Format == Entity.ContestFormat.TopCoder)
            {
                if (!(contest.Begin < contest.RestBegin && contest.RestBegin <= contest.RestEnd && contest.RestEnd < contest.End))
                {
                    return RedirectToAction("Message", "Shared", new { msg = "比赛时间或休息时间不合法" });
                }
            }
            else
            {
                if (!(contest.Begin < contest.End))
                {
                    return RedirectToAction("Message", "Shared", new { msg = "比赛时间设定不合法" });
                }
                contest.RestBegin = null;
                contest.RestEnd = null;
            }
            if (string.IsNullOrEmpty(contest.Password))
                contest.Password = null;
            DbContext.SaveChanges();
            return RedirectToAction("General", "ContestSettings", new { id = id });
        }

        [Authorize]
        public ActionResult Problems(int id)
        {
            if (!ViewBag.IsMaster)
                return RedirectToAction("Message", "Shared", new { msg = "您没有权限对本场比赛进行任何修改！" });
            ViewBag.ContestFormats = Enum.GetNames(typeof(Entity.ContestFormat));
            var contest = DbContext.Contests.Find(id);
            return View(contest);
        }

        [Authorize]
        public ActionResult Manager(int id)
        {
            if (!ViewBag.IsCreator)
                return RedirectToAction("Message", "Shared", new { msg = "您无权执行本操作！" });
            ViewBag.ContestFormats = Enum.GetNames(typeof(Entity.ContestFormat));
            var contest = DbContext.Contests.Find(id);
            return View(contest);
        }

        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult AddManager(int id, int user_id)
        {
            if (!ViewBag.IsCreator)
                return RedirectToAction("Message", "Shared", new { msg = "您无权执行本操作！" });
            ViewBag.ContestFormats = Enum.GetNames(typeof(Entity.ContestFormat));
            var existed = (from cm in DbContext.ContestManagers
                           where cm.UserID == user_id
                           select cm).Count() > 0;
            if(existed)
                return RedirectToAction("Message", "Shared", new { msg = "这个用户已经是管理员了！" });
            DbContext.ContestManagers.Add(new Entity.ContestManager 
            { 
                ContestID = id,
                IsCreator = false,
                UserID = user_id
            });
            DbContext.SaveChanges();
            return RedirectToAction("Manager", "ContestSettings", new { id = id });
        }

        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult DeleteManager(int id, int manager_id)
        {
            if (!ViewBag.IsCreator)
                return RedirectToAction("Message", "Shared", new { msg = "您无权执行本操作！" });
            ViewBag.ContestFormats = Enum.GetNames(typeof(Entity.ContestFormat));
            var manager = DbContext.ContestManagers.Find(manager_id);
            if (manager.IsCreator)
                return RedirectToAction("Message", "Shared", new { msg = "您无权执行本操作！" });
            DbContext.ContestManagers.Remove(manager);
            DbContext.SaveChanges();
            return RedirectToAction("Manager", "ContestSettings", new { id = id });
        }

        [Authorize]
        public ActionResult More(int id)
        {
            if (!ViewBag.IsMaster)
                return RedirectToAction("Message", "Shared", new { msg = "您没有权限对本场比赛进行任何修改！" });
            var contest = DbContext.Contests.Find(id);
            ViewBag.ContestFormats = Enum.GetNames(typeof(Entity.ContestFormat));
            var checks = new List<Models.View.ContestCheck>();
            foreach (var p in contest.Problems)
            {
                var chk = new Models.View.ContestCheck 
                { 
                    Problem = p.Title,
                    Status = Models.View.CheckStatus.Info,
                    Result = ""
                };
                if (!string.IsNullOrEmpty(p.SpecialJudge))
                {
                    chk.Status = Models.View.CheckStatus.Info;
                    chk.Result += "检测到了特殊比较器, ";
                }
                if (p.Solutions.Count == 0)
                {
                    chk.Status = Models.View.CheckStatus.Warning;
                    chk.Result += "没有设置题解, ";
                }
                if (p.TestCases.Where(x => x.Type == Entity.TestCaseType.Sample).Count() == 0)
                {
                    chk.Status = Models.View.CheckStatus.Warning;
                    chk.Result += "没有设置样例数据, ";
                    if (contest.Format == Entity.ContestFormat.TopCoder)
                        chk.Status = Models.View.CheckStatus.Error;
                }
                if (contest.Format == Entity.ContestFormat.CodeComb && p.TestCases.Where(x => x.Type == Entity.TestCaseType.Unilateralism).Count() == 0)
                {
                    chk.Status = Models.View.CheckStatus.Error;
                    chk.Result += "没有设置Unilateralism数据, ";
                }
                if (contest.Format == Entity.ContestFormat.Codeforces || contest.Format == Entity.ContestFormat.TopCoder)
                {
                    if (string.IsNullOrEmpty(p.StandardSource))
                    {
                        chk.Status = Models.View.CheckStatus.Error;
                        chk.Result += "没有设置标程, ";
                    }
                    if (string.IsNullOrEmpty(p.RangeChecker))
                    {
                        chk.Status = Models.View.CheckStatus.Error;
                        chk.Result += "没有设置数据范围校验器, ";
                    }
                }
                if (p.Credit == 0)
                {
                    chk.Status = Models.View.CheckStatus.Error;
                    chk.Result += "没有设置题目分数/顺序, ";
                }
                chk.Result = chk.Result.TrimEnd(' ').TrimEnd(',');
                if (chk.Result == "")
                {
                    chk.Result = "该题通过了审查";
                }
                checks.Add(chk);
            }
            ViewBag.Checks = checks;
            return View(contest);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Rejudge(int id)
        {
            var user = (Entity.User)ViewBag.CurrentUser;
            var contest = DbContext.Contests.Find(id);
            if (user.Role < Entity.UserRole.Master && (from cm in contest.Managers select cm.ID).Contains(user.ID))
                return RedirectToAction("Message", "Shared", new { msg = "您无权执行本操作！" });
            var statuses = (from s in DbContext.Statuses
                            where s.Problem.ContestID == id
                            select s.ID).ToList();
            foreach (var sid in statuses)
                StatusController._Rejudge(sid);
            return RedirectToAction("More", "ContestSettings", new { id = id });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Rehack(int id)
        {
            var user = (Entity.User)ViewBag.CurrentUser;
            var contest = DbContext.Contests.Find(id);
            if(user.Role < Entity.UserRole.Master && (from cm in contest.Managers select cm.ID).Contains(user.ID))
                return RedirectToAction("Message", "Shared", new { msg = "您无权执行本操作！" });
            var hacks = (from h in DbContext.Hacks
                            where h.Status.Problem.ContestID == id
                            select h.ID).ToList();
            foreach (var hid in hacks)
                StatusController._Rehack(hid);
            return RedirectToAction("More", "ContestSettings", new { id = id });
        }
	}
}