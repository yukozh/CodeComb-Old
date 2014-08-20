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
            return View();
        }

        public ActionResult Show(int id)
        {
            var problem = DbContext.Problems.Find(id);
            var contest = problem.Contest;
            var user = ViewBag.CurrentUser == null ? new Entity.User() : (Entity.User)ViewBag.CurrentUser;
            if (DateTime.Now < contest.Begin && user.Role < Entity.UserRole.Master && (from m in contest.Managers select m.ID).ToList().Contains(user.ID))
                return RedirectToAction("Message", "Shared", new { msg = "您无权查看该题目！" });
            var Problems = new List<Models.View.StatusSnapshot>();
            foreach (var p in problem.Contest.Problems.OrderBy(x=>x.Credit))
                Problems.Add(new Models.View.StatusSnapshot(p.Contest, ViewBag.CurrentUser));
            ViewBag.Problems = Problems;
            var statuses = problem.GetContestStatuses().Where(x => x.User.Username == User.Identity.Name).OrderByDescending(x => x.Time).ToList();
            if (contest.Format == ContestFormat.OI)
            {
                foreach (var s in statuses)
                {
                    s.Result = JudgeResult.Hidden;
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
            var contest = testcase.Problem.Contest;
            var user = ViewBag.CurrentUser == null ? new Entity.User() : (Entity.User)ViewBag.CurrentUser;
            if(user.Role < Entity.UserRole.Master && !(from cm in contest.Managers select cm.UserID).Contains(user.ID))
                return RedirectToAction("Message", "Shared", new { msg = "您无权执行本操作！" });
            DbContext.TestCases.Remove(testcase);
            DbContext.SaveChanges();
            return RedirectToAction("TestCases", "Problem", new { id = contest.ID });
        }

        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult ChangeTestCase(int id)
        {
            var testcase = DbContext.TestCases.Find(id);
            var contest = testcase.Problem.Contest;
            var user = ViewBag.CurrentUser == null ? new Entity.User() : (Entity.User)ViewBag.CurrentUser;
            if (user.Role < Entity.UserRole.Master && !(from cm in contest.Managers select cm.UserID).Contains(user.ID))
                return RedirectToAction("Message", "Shared", new { msg = "您无权执行本操作！" });
            testcase.TypeAsInt = (testcase.TypeAsInt + 1) % Enum.GetNames(typeof(Entity.TestCaseType)).Count();
            DbContext.SaveChanges();
            return RedirectToAction("TestCases", "Problem", new { id = contest.ID });
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
            return RedirectToAction("TestCases", "Problem", new { id = contest.ID });
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
            return RedirectToAction("TestCases", "Problem", new { id = contest.ID });
        }

        public ActionResult TestCaseUpload(int id, HttpPostedFile file)
        {
            var problem = DbContext.Problems.Find(id);
            var contest = problem.Contest;
            var user = ViewBag.CurrentUser == null ? new Entity.User() : (Entity.User)ViewBag.CurrentUser;
            if (user.Role < Entity.UserRole.Master && !(from cm in contest.Managers select cm.UserID).Contains(user.ID))
                return RedirectToAction("Message", "Shared", new { msg = "您无权执行本操作！" });
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
            return RedirectToAction("TestCases", "Problem", new { id = contest.ID });
        }
	}   
}