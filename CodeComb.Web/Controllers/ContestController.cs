﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CodeComb.Web.Controllers
{
    public class ContestController : ContestBaseController
    {
        //
        // GET: /Contest/
        public ActionResult Index()
        {
            var time = DateTime.Now.AddDays(7);
            var contests = (from c in DbContext.Contests
                            where DateTime.Now < c.End
                            && c.Problems.Count > 0
                            && c.Begin <= time
                            orderby c.Begin ascending
                            select c).ToList();
            return View(contests);
        }

        [HttpGet]
        public ActionResult GetContests(int page, int? format)
        {
            var HistroyTime = Convert.ToDateTime("2014-8-1 0:00");
            var _contests = (from c in DbContext.Contests
                            where DateTime.Now >= c.End
                            && c.End >= HistroyTime
                            && c.Problems.Count > 0
                            select c);
            if (format != null)
                _contests = _contests.Where(x => x.FormatAsInt == format);
            _contests = _contests.OrderByDescending(x => x.End);
            var contests = new List<Models.View.Contest>();
            foreach (var contest in _contests.Skip(10 * page).Take(10).ToList())
                contests.Add(new Models.View.Contest(contest));
            return Json(contests, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Show(int id)
        {
            var contest = DbContext.Contests.Find(id);
            if (!Helpers.PrivateContest.IsUserInPrivateContest(ViewBag.CurrentUser == null ? null : (Entity.User)ViewBag.CurrentUser, contest))
                return RedirectToAction("Private", "Contest", new { id = id });
            ViewBag.UserCount = (from s in DbContext.Statuses
                         let pid = (from p in DbContext.Problems
                                    where p.ContestID == contest.ID
                                    select p.ID).ToList()
                         where pid.Contains(s.ProblemID)
                         && s.Time >= contest.Begin
                         && s.Time < contest.End
                         select s.UserID).Distinct().Count();
            if (DateTime.Now < contest.Begin)
                ViewBag.InfoCss = "pending";
            else if (DateTime.Now <= contest.End)
                ViewBag.InfoCss = "inprogress";
            else
                ViewBag.InfoCss = "";
            var lstProblems = new List<Models.View.ProblemInContest>();
            Entity.User user = null;
            if(User.Identity.IsAuthenticated)
                user = (from u in DbContext.Users where u.Username == User.Identity.Name select u).Single();
            foreach (var problem in contest.Problems.OrderBy(x => x.Credit))
                lstProblems.Add(new Models.View.ProblemInContest(problem, user));
            ViewBag.Problems = lstProblems;
            return View(contest);
        }

        public ActionResult Statistics(int id)
        {
            var contest = DbContext.Contests.Find(id);
            if (!Helpers.PrivateContest.IsUserInPrivateContest(ViewBag.CurrentUser == null ? null : (Entity.User)ViewBag.CurrentUser, contest))
                return RedirectToAction("Private", "Contest", new { id = id });
            var statistics = new int[contest.Problems.Count, 9];
            var i = 0;
            foreach (var p in contest.Problems.OrderBy(x => x.Credit))
            {
                var statuses = p.GetContestStatuses();
                if (contest.Format == Entity.ContestFormat.OI)
                {
                    var user_ids = (from s in statuses
                                 select s.UserID).Distinct();
                    foreach (var uid in user_ids)
                    {
                        var last_status = statuses.Where(x => x.UserID == uid).LastOrDefault();
                        if (last_status != null)
                        {
                            if(last_status.ResultAsInt < 8)
                                statistics[i, last_status.ResultAsInt]++;
                            else
                                statistics[i, 8]++;
                        }
                    }
                }
                else
                { 
                    statistics[i,0] = statuses.Where(x => x.Result == Entity.JudgeResult.Accepted).Count();
                    statistics[i,1] = statuses.Where(x => x.Result == Entity.JudgeResult.PresentationError).Count();
                    statistics[i,2] = statuses.Where(x => x.Result == Entity.JudgeResult.WrongAnswer).Count();
                    statistics[i,3] = statuses.Where(x => x.Result == Entity.JudgeResult.OutputLimitExceeded).Count();
                    statistics[i,4] = statuses.Where(x => x.Result == Entity.JudgeResult.TimeLimitExceeded).Count();
                    statistics[i,5] = statuses.Where(x => x.Result == Entity.JudgeResult.MemoryLimitExceeded).Count();
                    statistics[i,6] = statuses.Where(x => x.Result == Entity.JudgeResult.RuntimeError).Count();
                    statistics[i,7] = statuses.Where(x => x.Result == Entity.JudgeResult.CompileError).Count();
                    statistics[i,8] = statuses.Where(x => x.Result == Entity.JudgeResult.Hacked).Count();
                }
                i++;
            }
            ViewBag.Statistics = statistics;
            return View(contest);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Print(int id, string content)
        {
            var contest = DbContext.Contests.Find(id);
            if (!Helpers.PrivateContest.IsUserInPrivateContest(ViewBag.CurrentUser == null ? null : (Entity.User)ViewBag.CurrentUser, contest))
                return RedirectToAction("Private", "Contest", new { id = id });
            if (!contest.AllowPrintRequest)
                return RedirectToAction("Message", "Shared", new { msg = "这场比赛没有开放打印服务！" });
            if(DateTime.Now < contest.Begin || DateTime.Now >= contest.End)
                return RedirectToAction("Message", "Shared", new { msg = "只有在比赛进行时才可以使用打印服务！" });
            DbContext.PrintRequests.Add(new Entity.PrintRequest 
            {
                PrintFinished = false,
                Content = content,
                Time = DateTime.Now,
                ContestID = id,
                UserID = ViewBag.CurrentUser.ID
            });
            DbContext.SaveChanges();
            return RedirectToAction("Message", "Shared", new { msg = "打印材料提交成功，您当前的队列位置为：" + contest.PrintRequests.Where(x => !x.PrintFinished).Count() });
        }

        [Authorize]
        public ActionResult Print(int id)
        {
            var contest = DbContext.Contests.Find(id);
            if (!Helpers.PrivateContest.IsUserInPrivateContest(ViewBag.CurrentUser == null ? null : (Entity.User)ViewBag.CurrentUser, contest))
                return RedirectToAction("Private", "Contest", new { id = id });
            if (!contest.AllowPrintRequest)
                return RedirectToAction("Message", "Shared", new { msg = "这场比赛没有开放打印服务！" });
            if (DateTime.Now < contest.Begin || DateTime.Now >= contest.End)
                return RedirectToAction("Message", "Shared", new { msg = "只有在比赛进行时才可以使用打印服务！" });
            return View(contest);
        }

        [HttpGet]
        public ActionResult GetClar(int id)
        {
            var uid = 0;
            if (User.Identity.IsAuthenticated)
                uid = (int)ViewBag.CurrentUser.ID;
            var clar = DbContext.Clarifications.Find(id);
            if (clar.Status != Entity.ClarificationStatus.BroadCast && clar.UserID != uid)
                return Json(null, JsonRequestBehavior.AllowGet);
            return Json(new Models.View.Clar(clar), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Clar(int id)
        {
            if (!Helpers.PrivateContest.IsUserInPrivateContest(ViewBag.CurrentUser == null ? null : (Entity.User)ViewBag.CurrentUser, DbContext.Contests.Find(id)))
                return RedirectToAction("Private", "Contest", new { id = id });
            var clarifications = (from c in DbContext.Clarifications
                                  where c.ContestID == id
                                  && c.StatusAsInt == (int)Entity.ClarificationStatus.BroadCast
                                  select c).ToList();
            ViewBag.CurrentContest = DbContext.Contests.Find(id);
            if (User.Identity.IsAuthenticated)
            {
                var _problems = (from p in DbContext.Problems
                                 where p.ContestID == id
                                 orderby p.Credit ascending
                                 select p).ToList();
                var problems = new List<Models.View.ClarProblem>();
                foreach (var p in _problems)
                    problems.Add(new Models.View.ClarProblem(p));
                ViewBag.Problems = problems;
                var uid = (int)ViewBag.CurrentUser.ID;
                clarifications = clarifications.Union((from c in DbContext.Clarifications
                                                       where c.ContestID == id
                                                       && c.UserID == uid
                                                       select c).ToList()).ToList();
            }
            clarifications = clarifications.OrderByDescending(x => x.Time).ToList();
            return View(clarifications);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        [Authorize]
        public ActionResult Clar(int id,int? problem_id, string question)
        {
            //合法性验证
            var contest = DbContext.Contests.Find(id);
            if (!Helpers.PrivateContest.IsUserInPrivateContest(ViewBag.CurrentUser == null ? null : (Entity.User)ViewBag.CurrentUser, contest))
                return RedirectToAction("Private", "Contest", new { id = id });
            if (!contest.AllowClarification)
                return RedirectToAction("Message", "Shared", new { msg = "本场比赛没有开启答疑平台！" });
            var problem = DbContext.Problems.Find(problem_id);
            if (problem != null && problem.ContestID != contest.ID)
                return RedirectToAction("Message", "Shared", new { msg = "没有找到该题！" });

            var clar = new Entity.Clarification
            {
                ContestID = id,
                ProblemID = problem_id,
                Status = Entity.ClarificationStatus.Pending,
                Question = question,
                Answer = "",
                Time = DateTime.Now,
                UserID = ViewBag.CurrentUser.ID
            };
            DbContext.Clarifications.Add(clar);
            DbContext.SaveChanges();
            var masters = (from u in DbContext.Users
                           let master_ids = (from m in DbContext.ContestManagers
                                             where m.ContestID == contest.ID
                                             select m.UserID).ToList()
                           where u.RoleAsInt >= (int)Entity.UserRole.Master
                           || master_ids.Contains(u.ID)
                           select u).Distinct().ToList();
            foreach (var master in masters)
            {
                SignalR.CodeCombHub.context.Clients.Group(master.Username).onClarificationsRequested(clar.ID);
                SignalR.MobileHub.PushTo(master.ID, (clar.ProblemID == null ? "General" : clar.Problem.Title) + ":" + clar.Question);
                SignalR.MobileHub.context.Clients.Group(master.Username).onClarificationsRequested(new CodeComb.Models.WebAPI.Clarification 
                {
                    ClarID = clar.ID,
                    Question = clar.Question,
                    Answer = clar.Answer,
                    Category = clar.ProblemID == null?"General":clar.Problem.Title,
                    Status = clar.Status.ToString(),
                    StatusAsInt = clar.StatusAsInt,
                    ContestID = clar.ContestID,
                    Time = clar.Time
                });
            }

            //重新载入Model
            var clarifications = (from c in DbContext.Clarifications
                                  where c.ContestID == id
                                  && c.StatusAsInt == (int)Entity.ClarificationStatus.BroadCast
                                  select c).ToList();
            ViewBag.CurrentContest = DbContext.Contests.Find(id);
            if (User.Identity.IsAuthenticated)
            {
                var _problems = (from p in DbContext.Problems
                                    where p.ContestID == id
                                    orderby p.Credit descending
                                    select p).ToList();
                var problems = new List<Models.View.ClarProblem>();
                foreach (var p in _problems) 
                    problems.Add(new Models.View.ClarProblem(p));
                ViewBag.Problems = problems;
                var uid = (int)ViewBag.CurrentUser.ID;
                clarifications = clarifications.Union((from c in DbContext.Clarifications
                                                       where c.ContestID == id
                                                       && c.UserID == uid
                                                       select c).ToList()).ToList();
            }
            clarifications = clarifications.OrderByDescending(x => x.Time).ToList();
            return View(clarifications);
        }

        [HttpPost]
        [ValidateInput(false)]
        [Authorize]
        public ActionResult ResponseClar(int id, string answer, bool broadcast)
        {
            if (!ViewBag.IsMaster)
            {
                return Content("No");
            }
            var clar = DbContext.Clarifications.Find(id);
            if(broadcast)
                clar.Status = Entity.ClarificationStatus.BroadCast;
            else 
                clar.Status = Entity.ClarificationStatus.Private;
            clar.Answer = answer;
            DbContext.SaveChanges();
            if (broadcast)
            {
                SignalR.CodeCombHub.context.Clients.All.onClarificationsResponsed(new Models.View.Clar(clar));
                SignalR.MobileHub.context.Clients.All.onClarificationsResponsed(new CodeComb.Models.WebAPI.Clarification 
                { 
                    Answer = clar.Answer,
                    ClarID = clar.ID,
                    Category = clar.ProblemID==null?"General":clar.Problem.Title,
                    Time = clar.Time,
                    ContestID = clar.ContestID,
                    Question = clar.Question,
                    Status = clar.Status.ToString(),
                    StatusAsInt = clar.StatusAsInt
                });
                SignalR.MobileHub.PushToAll((clar.ProblemID == null ? "General" : clar.Problem.Title) + ":" + clar.Answer);
            }
            else
            {
                SignalR.CodeCombHub.context.Clients.Group(clar.User.Username).onClarificationsResponsed(new Models.View.Clar(clar));
                SignalR.MobileHub.context.Clients.Group(clar.User.Username).onClarificationsResponsed(new CodeComb.Models.WebAPI.Clarification
                {
                    Answer = clar.Answer,
                    ClarID = clar.ID,
                    Category = clar.ProblemID == null ? "General" : clar.Problem.Title,
                    Time = clar.Time,
                    ContestID = clar.ContestID,
                    Question = clar.Question,
                    Status = clar.Status.ToString(),
                    StatusAsInt = clar.StatusAsInt
                });
                SignalR.MobileHub.PushTo(clar.UserID, (clar.ProblemID == null ? "General" : clar.Problem.Title) + ":" + clar.Answer);
            }
            return Content("Yes");
        }

        public ActionResult Standings(int id)
        {
            var contest = DbContext.Contests.Find(id);
            if (!Helpers.PrivateContest.IsUserInPrivateContest(ViewBag.CurrentUser == null ? null : (Entity.User)ViewBag.CurrentUser, contest))
                return RedirectToAction("Private", "Contest", new { id = id });
            if (contest.Format == Entity.ContestFormat.OI && DateTime.Now < contest.End && !ViewBag.IsMaster)
                return RedirectToAction("Message", "Shared", new { msg = "目前不提供比赛排名显示。" });
            ViewBag.AllowHack = false;
            if (User.Identity.IsAuthenticated)
            {
                if (contest.Format == Entity.ContestFormat.Codeforces && DateTime.Now >= contest.Begin && DateTime.Now < contest.End)
                    ViewBag.AllowHack = true;
                else if(contest.Format == Entity.ContestFormat.TopCoder && DateTime.Now >= contest.RestEnd && DateTime.Now < contest.End)
                    ViewBag.AllowHack = true;
            }
            return View(contest);
        }

        [HttpGet]
        public ActionResult GetStandings(int id)
        {
            var contest = DbContext.Contests.Find(id);
            if (!Helpers.PrivateContest.IsUserInPrivateContest(ViewBag.CurrentUser == null ? null : (Entity.User)ViewBag.CurrentUser, contest))
                return RedirectToAction("Private", "Contest", new { id = id });
            if (contest.Format == Entity.ContestFormat.OI && DateTime.Now < contest.End && !ViewBag.IsMaster)
                return Json(null, JsonRequestBehavior.AllowGet);
            var standings = Helpers.Standings.Build(id);
            return Json(standings, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Hack(int id)
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Create()
        {
            var contest = new Entity.Contest 
            { 
                AllowClarification = true,
                AllowPrintRequest = false,
                Begin = Convert.ToDateTime("2099-1-1 8:00"),
                End = Convert.ToDateTime("2099-1-1 11:00"),
                Format = Entity.ContestFormat.OI,
                Password = null,
                Content = "",
                Ready = false,
                Title = ViewBag.CurrentUser.Nickname +"创建的比赛",
                RatingBegin = 0,
                RatingEnd = 4000
            };
            var manager = new Entity.ContestManager 
            {
                ContestID = contest.ID,
                IsCreator = true,
                UserID = ViewBag.CurrentUser.ID
            };
            DbContext.Contests.Add(contest);
            DbContext.ContestManagers.Add(manager);
            DbContext.SaveChanges();
            return RedirectToAction("General", "ContestSettings", new { id = contest.ID });
        }

        [Authorize]
        public ActionResult Private(int id)
        {
            var contest = DbContext.Contests.Find(id);
            if (string.IsNullOrEmpty(contest.Password))
                return RedirectToAction("Show", "Contest", new { id = id });
            var user = (Entity.User)ViewBag.CurrentUser;
            var joinlogs = (from j in DbContext.JoinLogs
                            where j.UserID == user.ID
                            && j.ContestID == id
                            select j).FirstOrDefault();
            if (joinlogs == null)
            {
                if (user.Role >= Entity.UserRole.Master || (from cm in contest.Managers select cm.UserID).Contains(user.ID))
                {
                    joinlogs = new Entity.JoinLog
                    {
                        UserID = user.ID,
                        ContestID = id
                    };
                    DbContext.JoinLogs.Add(joinlogs);
                    DbContext.SaveChanges();
                    return RedirectToAction("Show", "Contest", new { id = id });
                }
                else
                {
                    ViewBag.ContestTitle = contest.Title;
                    return View();
                }
            }
            else
            {
                return RedirectToAction("Show", "Contest", new { id = id });
            }
        }

        [Authorize]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        [HttpPost]
        public ActionResult Private(int id, string password)
        {
            var contest = DbContext.Contests.Find(id);
            if (string.IsNullOrEmpty(contest.Password))
                return RedirectToAction("Show", "Contest", new { id = id });
            var user = (Entity.User)ViewBag.CurrentUser;
            var joinlogs = (from j in DbContext.JoinLogs
                            where j.UserID == user.ID
                            && j.ContestID == id
                            select j).FirstOrDefault();
            if (joinlogs != null)
                return RedirectToAction("Show", "Contest", new { id = id });
            if (password == contest.Password)
            {
                joinlogs = new Entity.JoinLog
                {
                    UserID = user.ID,
                    ContestID = id
                };
                DbContext.JoinLogs.Add(joinlogs);
                DbContext.SaveChanges();
                return RedirectToAction("Show", "Contest", new { id = id });
            }
            else
            {
                return RedirectToAction("Message", "Shared", new { msg="您输入的参赛密码不正确，请返回重试！"});
            }
        }
	}
}