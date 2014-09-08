using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CodeComb.Entity;
using CodeComb.Database;

namespace CodeComb.Web.Controllers
{
    public class ProblemController : BaseController
    {
        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            base.Initialize(requestContext);
            ViewBag.IsMaster = false;
            ViewBag.IsCreator = false;
            if (!User.Identity.IsAuthenticated) return;
            try
            {
                var id = requestContext.RouteData.Values["id"] == null ? null : (int?)Convert.ToInt32(requestContext.RouteData.Values["id"]);
                if (id == null) return;
                var contest = DbContext.Problems.Find(id).Contest;
                var user = (from u in DbContext.Users where u.Username == User.Identity.Name select u).Single();
                if (user.Role >= UserRole.Master)
                {
                    ViewBag.IsMaster = true;
                    ViewBag.IsCreator = true;
                }
                else
                {
                    var contestmanager = (from cm in contest.Managers where cm.UserID == user.ID select cm).SingleOrDefault();
                    if (contestmanager != null)
                    {
                        ViewBag.IsMaster = true;
                        if (contestmanager.IsCreator)
                            ViewBag.IsCreator = true;
                    }
                }
            }
            catch { }
        }//id = problem
        //
        // GET: /Problem/
        public ActionResult Index()
        {
            var problem_count = (from p in DbContext.Problems
                                 select p).Count();
            var page_count = (int)Math.Ceiling(problem_count / 100.0);
            var pager = new List<Models.View.ProblemPager>();
            for (int i = 1; i <= page_count; i++)
            {
                pager.Add(new Models.View.ProblemPager(i));
            }
            ViewBag.Pager = pager;
            var tags = (from at in DbContext.AlgorithmTags
                        where at.FatherID == null
                        select at).ToList();
            ViewBag.Tags = tags;
            return View();
        }

        public ActionResult Show(int id)
        {
            var problem = DbContext.Problems.Find(id);
            var contest = problem.Contest;
            if (!Helpers.PrivateContest.IsUserInPrivateContest(ViewBag.CurrentUser == null ? null : (Entity.User)ViewBag.CurrentUser, contest))
                return RedirectToAction("Private", "Contest", new { id = id });
            var user = ViewBag.CurrentUser == null ? new Entity.User() : (Entity.User)ViewBag.CurrentUser;
            if (DateTime.Now < contest.Begin && user.Role < Entity.UserRole.Master && !(from m in contest.Managers select m.ID).ToList().Contains(user.ID))
                return RedirectToAction("Message", "Shared", new { msg = "您无权查看该题目！" });
            var Problems = new List<Models.View.StatusSnapshot>();
            foreach (var p in problem.Contest.Problems.OrderBy(x=>x.Credit))
                Problems.Add(new Models.View.StatusSnapshot(p, ViewBag.CurrentUser));
            ViewBag.Problems = Problems;
            var statuses = problem.GetContestStatuses().Where(x => x.User.Username == User.Identity.Name).OrderByDescending(x => x.Time).ToList();
            if (contest.Format == ContestFormat.OI)
            {
                foreach (var s in statuses)
                {
                    s.Result = JudgeResult.Hidden;
                }
            }
            if (contest.Format == ContestFormat.Codeforces)
            {
                ViewBag.IsLocked = false;
                if (User.Identity.IsAuthenticated)
                {
                    var locked = (from l in DbContext.Locks
                                  where l.ProblemID == problem.ID
                                  && l.UserID == user.ID
                                  select l).Count();
                    if(locked > 0)
                        ViewBag.IsLocked = true;
                }
            }
            ViewBag.Statuses = statuses;
            return View(problem);
        }
        
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Create(int id)
        {
            var contest = DbContext.Contests.Find(id);
            var user = ViewBag.CurrentUser == null ? new Entity.User() : (Entity.User)ViewBag.CurrentUser;
            if (user.Role < Entity.UserRole.Master && !(from cm in contest.Managers select cm.UserID).Contains(user.ID))
                return RedirectToAction("Message", "Shared", new { msg = "您无权执行本操作！" });
            DbContext.Problems.Add(new Problem 
            { 
                Difficulty = 0,
                Background = null,
                Input = null,
                Output =null,
                Description = null,
                SpecialJudge="",
                SpecialJudgeLanguage = Language.C,
                StandardSource = "",
                StandardSourceLanguage = Language.C,
                RangeChecker = "",
                RangeCheckerLanguage = Language.C,
                MemoryLimit = 65536,
                TimeLimit = 1000,
                Title = "新建题目",
                Hint = null,
                ContestID = id,
                Credit = 0
            });
            DbContext.SaveChanges();
            return RedirectToAction("Problems", "ContestSettings", new { id = id });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var problem = DbContext.Problems.Find(id);
            if (!ViewBag.IsMaster)
                return RedirectToAction("Message", "Shared", new { msg = "您无权执行本操作！" });
            DbContext.Problems.Remove(problem);
            DbContext.SaveChanges();
            return RedirectToAction("Problems", "ContestSettings", new { id = id });
        }

        [Authorize]
        public ActionResult Edit(int id)
        {
            if (!ViewBag.IsMaster)
                return RedirectToAction("Message", "Shared", new { msg = "您无权执行本操作！" });
            var problem = DbContext.Problems.Find(id);
            return View(problem);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit(int id, Problem model)
        {
            if (!ViewBag.IsMaster)
                return RedirectToAction("Message", "Shared", new { msg = "您无权执行本操作！" });
            var user = ViewBag.CurrentUser == null ? new Entity.User() : (Entity.User)ViewBag.CurrentUser;
            var problem = DbContext.Problems.Find(id);
            problem.TimeLimit = model.TimeLimit;
            problem.MemoryLimit = model.MemoryLimit;
            problem.Background = model.Background;
            problem.Description = model.Description;
            problem.Input = model.Input;
            problem.Output = model.Output;
            problem.Hint = model.Hint;
            problem.Title = model.Title;
            problem.Credit = model.Credit;
            if (user.Role >= Entity.UserRole.Master)
            {
                problem.Difficulty = model.Difficulty;
            }
            else
            {
                if (problem.TimeLimit > 5000) problem.TimeLimit = 5000;
                if (problem.MemoryLimit > 1024 * 128) problem.TimeLimit = 1024 * 128;
            }
            DbContext.SaveChanges();
            return RedirectToAction("Edit", "Problem", new { id = id });
        }

        [Authorize]
        public ActionResult TestCases(int id)
        {
            if (!ViewBag.IsMaster)
                return RedirectToAction("Message", "Shared", new { msg = "您无权执行本操作！" });
            var problem = DbContext.Problems.Find(id);
            return View(problem);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult DeleteTestCase(int id)
        {
            var testcase = DbContext.TestCases.Find(id);
            var problem_id = testcase.ProblemID;
            var contest = testcase.Problem.Contest;
            var user = ViewBag.CurrentUser == null ? new Entity.User() : (Entity.User)ViewBag.CurrentUser;
            if(user.Role < Entity.UserRole.Master && !(from cm in contest.Managers select cm.UserID).Contains(user.ID))
                return RedirectToAction("Message", "Shared", new { msg = "您无权执行本操作！" });
            DbContext.TestCases.Remove(testcase);
            DbContext.SaveChanges();
            return RedirectToAction("TestCases", "Problem", new { id = problem_id });
        }

        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult ChangeTestCaseType(int tid, int type)
        {
            var testcase = DbContext.TestCases.Find(tid);
            var contest = testcase.Problem.Contest;
            var user = ViewBag.CurrentUser == null ? new Entity.User() : (Entity.User)ViewBag.CurrentUser;
            if (user.Role < Entity.UserRole.Master && !(from cm in contest.Managers select cm.UserID).Contains(user.ID))
                return RedirectToAction("Message", "Shared", new { msg = "您无权执行本操作！" });
            testcase.TypeAsInt = type;
            DbContext.SaveChanges();
            return RedirectToAction("TestCases", "Problem", new { id = testcase.ProblemID });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult TestCaseEdit(int id,string input, string output)
        {
            var testcase = DbContext.TestCases.Find(id);
            var contest = testcase.Problem.Contest;
            var user = ViewBag.CurrentUser == null ? new Entity.User() : (Entity.User)ViewBag.CurrentUser;
            if (user.Role < Entity.UserRole.Master && !(from cm in contest.Managers select cm.UserID).Contains(user.ID))
                return RedirectToAction("Message", "Shared", new { msg = "您无权执行本操作！" });
            testcase.Input = input;
            testcase.Output = output;
            DbContext.SaveChanges();
            return RedirectToAction("TestCases", "Problem", new { id = testcase.ProblemID});
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult TestCaseTextUpload(int id, string input, string output)
        {
            var problem = DbContext.Problems.Find(id);
            var contest = problem.Contest;
            var user = ViewBag.CurrentUser == null ? new Entity.User() : (Entity.User)ViewBag.CurrentUser;
            if (user.Role < Entity.UserRole.Master && !(from cm in contest.Managers select cm.UserID).Contains(user.ID))
                return RedirectToAction("Message", "Shared", new { msg = "您无权执行本操作！" });
            DbContext.TestCases.Add(new Entity.TestCase 
            { 
                Input = input,
                Output = output,
                ProblemID = id,
                Type = TestCaseType.Overall,
                Hash = Helpers.Security.SHA1(input)
            });
            DbContext.SaveChanges();
            return RedirectToAction("TestCases", "Problem", new { id = problem.ID });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult TestCaseUpload(int id)
        {
            var problem = DbContext.Problems.Find(id);
            var contest = problem.Contest;
            var user = ViewBag.CurrentUser == null ? new Entity.User() : (Entity.User)ViewBag.CurrentUser;
            if (user.Role < Entity.UserRole.Master && !(from cm in contest.Managers select cm.UserID).Contains(user.ID))
                return RedirectToAction("Message", "Shared", new { msg = "您无权执行本操作！" });
            var file = Request.Files[0];
            var timestamp = Helpers.String.ToTimeStamp(DateTime.Now);
            var filename = timestamp+".zip";
            var dir = Server.MapPath("~") + @"\Temp\";
            file.SaveAs(dir + filename);
            Helpers.Zip.UnZip(dir + filename, dir + timestamp);
            System.IO.File.Delete(dir + filename);
            var files = System.IO.Directory.GetFiles(dir + timestamp);
            foreach (var f in files)
            {
                if (System.IO.Path.GetExtension(f) == ".in")
                {
                    var testcase = new Entity.TestCase();
                    testcase.Input = System.IO.File.ReadAllText(f);
                    var outfile = f.Substring(0, f.Length - 3) + ".out";
                    var exist = false;
                    if (System.IO.File.Exists(outfile))
                    {
                        testcase.Output = System.IO.File.ReadAllText(outfile);
                        exist = true;
                    }
                    outfile = f.Substring(0, f.Length - 3) + ".ans";
                    if (System.IO.File.Exists(outfile))
                    {
                        testcase.Output = System.IO.File.ReadAllText(outfile);
                        exist = true;
                    }
                    if (!exist) continue;
                    testcase.ProblemID = id;
                    testcase.Type = TestCaseType.Overall;
                    testcase.Hash = Helpers.Security.SHA1(testcase.Input);
                    DbContext.TestCases.Add(testcase);
                }
            }
            DbContext.SaveChanges();
            System.IO.Directory.Delete(dir + timestamp, true);
            return RedirectToAction("TestCases", "Problem", new { id = problem.ID });
        }

        [HttpGet]
        [Authorize]
        public ActionResult GetTestCase(int id)
        { 
            var testcase = DbContext.TestCases.Find(id);
            var contest = testcase.Problem.Contest;
            var user = ViewBag.CurrentUser == null ? new Entity.User() : (Entity.User)ViewBag.CurrentUser;
            if (user.Role < Entity.UserRole.Master && !(from cm in contest.Managers select cm.UserID).Contains(user.ID))
                return Json(null, JsonRequestBehavior.AllowGet);
            return Json(new Models.View.TestCase(testcase), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public ActionResult Special(int id)
        {
            if (!ViewBag.IsMaster)
                return RedirectToAction("Message", "Shared", new { msg = "您无权执行本操作！" });
            var problem = DbContext.Problems.Find(id);
            return View(problem);
        }

        [Authorize]
        public ActionResult Standard(int id)
        {
            if (!ViewBag.IsMaster)
                return RedirectToAction("Message", "Shared", new { msg = "您无权执行本操作！" });
            var problem = DbContext.Problems.Find(id);
            return View(problem);
        }

        [Authorize]
        public ActionResult Range(int id)
        {
            if (!ViewBag.IsMaster)
                return RedirectToAction("Message", "Shared", new { msg = "您无权执行本操作！" });
            var problem = DbContext.Problems.Find(id);
            return View(problem);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Special(int id, string special, int language)
        {
            if (!ViewBag.IsMaster)
                return RedirectToAction("Message", "Shared", new { msg = "您无权执行本操作！" });
            var problem = DbContext.Problems.Find(id);
            problem.SpecialJudge = special;
            problem.SpecialJudgeLanguageAsInt = language;
            DbContext.SaveChanges();
            return RedirectToAction("Special", "Problem", new { id = id });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Standard(int id, string standard, int language)
        {
            if (!ViewBag.IsMaster)
                return RedirectToAction("Message", "Shared", new { msg = "您无权执行本操作！" });
            var problem = DbContext.Problems.Find(id);
            problem.StandardSource = standard;
            problem.StandardSourceLanguageAsInt = language;
            DbContext.SaveChanges();
            return RedirectToAction("Standard", "Problem", new { id = id });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Range(int id, string range, int language)
        {
            if (!ViewBag.IsMaster)
                return RedirectToAction("Message", "Shared", new { msg = "您无权执行本操作！" });
            var problem = DbContext.Problems.Find(id);
            problem.RangeChecker = range;
            problem.RangeCheckerLanguageAsInt = language;
            DbContext.SaveChanges();
            return RedirectToAction("Range", "Problem", new { id = id });
        }

        [HttpGet]
        public ActionResult GetProblems(int page, string tags, string title, int? morethan, int? lessthan)
        {
            tags = tags.Trim(',');
            if (title == null) title = "";
            var tag_ids = new List<int>();
            if (!string.IsNullOrEmpty(tags))
            {
                foreach (var tag in tags.Split(','))
                {
                    tag_ids.Add(Convert.ToInt32(tag));
                }
            }
            var now = DateTime.Now;
            IEnumerable<Entity.Problem> _problems = (from p in DbContext.Problems
                             where p.Title.Contains(title)
                             && !p.Hide
                             && DateTime.Now >= p.Contest.End
                             select p);
            if (tag_ids.Count > 0)
            {
                var __problems = (from p in DbContext.SolutionTags
                                where tag_ids.Contains(p.AlgorithmTagID)
                                select p.Solution.ProblemID).ToList();
                _problems = (from p in _problems
                             where __problems.Contains(p.ID)
                             select p);
            }
            if (morethan != null && lessthan != null)
                _problems = _problems.Where(x => x.ID >= morethan && x.ID <= lessthan);
            _problems = _problems.OrderBy(x => x.ID).Skip(100 * page).Take(100).ToList();
            List<Models.View.ProblemInList> problems = new List<Models.View.ProblemInList>();
            foreach (var problem in _problems)
                problems.Add(new Models.View.ProblemInList(problem, ViewBag.CurrentUser));
            return Json(problems, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Lock(int id)
        {
            var problem = DbContext.Problems.Find(id);
            var contest = problem.Contest;
            var user = (Entity.User)ViewBag.CurrentUser;
            if (contest.Format != ContestFormat.Codeforces)
                return Content("只有Codeforces比赛可以锁定题目！");
            var ac_count = problem.GetContestStatuses().Where(x => x.UserID == user.ID && x.ResultAsInt == (int)Entity.JudgeResult.Accepted).Count();
            if (ac_count == 0)
                return Content("您必须先通过该题才能进行锁定。");
            var locked = (from l in DbContext.Locks
                          where l.ProblemID == problem.ID
                          && l.UserID == user.ID
                          select l).Count();
            if (locked > 0)
                return Content("该题已经被锁定！");
            DbContext.Locks.Add(new Lock { 
                ProblemID = problem.ID,
                Time = DateTime.Now,
                UserID = user.ID
            });
            DbContext.SaveChanges();
            return Content("您已成功锁定" + HttpUtility.HtmlEncode(problem.Title) + "!");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Hide(int id, bool hide)
        {
            if (ViewBag.CurrentUser.Role < Entity.UserRole.Master)
                return Content("NO");
            var problem = DbContext.Problems.Find(id);
            problem.Hide = hide;
            DbContext.SaveChanges();
            return Content("OK");
        }
	}   
}