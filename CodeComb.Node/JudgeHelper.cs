using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Web.Script.Serialization;
using System.Web.Security;
using System.Diagnostics;
using CodeComb.Judge.Models;

namespace CodeComb.Node
{
    public static class JudgeHelper
    {
        public const int CompileTimeLimit = 4000;
        public static string[] FileNames = 
        { 
            "{Name}.c",
            "{Name}.cpp",
            "{Name}.cpp",
            "{Name}.java",
            "{Name}.py",
            "{Name}.py",
            "{Name}.rb",
            "{Name}.cs",
            "{Name}.vb"
        };
        public static string[] CompileArgs = 
        { 
            "gcc -O2 -o {Name}.exe -DONLINE_JUDGE -lm --static --std=c99 {Name}.c", 
            "g++ -O2 -o {Name}.exe -DONLINE_JUDGE -lm --static --std=c++98 {Name}.cpp", 
            "g++ -O2 -o {Name}.exe -DONLINE_JUDGE -lm --static --std=c++11 {Name}.cpp", 
            "javac {Name}.java", 
            "fpc -O2 -dONLINE_JUDGE {Name}.pas", 
            "", 
            "", 
            "", 
            "csc Main.cs", 
            "vbc Main.vb" 
        };
        public static string[] ExcuteArgs = 
        { 
            "{Name}.exe", 
            "{Name}.exe", 
            "{Name}.exe", 
            "java {Name}", 
            "{Name}.exe", 
            "py27 {Name}.py", 
            "py33 {Name}.py", 
            "ruby {Name}.rb", 
            "{Name}.exe", 
            "{Name}.exe" 
        };
        public const string SpjArgs = " output.txt Main.out input.txt";
        public static void CheckPath(int id)
        {
            if (!Directory.Exists(Program.TempPath + @"\" + id + @"\"))
                Directory.CreateDirectory(Program.TempPath + @"\" + id + @"\");
        }

        public static void MakeCodeFile(int id, string code, int language_id, Mode mode)
        {
            if (!File.Exists(Program.TempPath + @"\" + id + @"\" + FileNames[language_id].Replace("{Name}", mode.ToString())))
                File.WriteAllText(Program.TempPath + @"\" + id + @"\" + FileNames[language_id].Replace("{Name}", mode.ToString()), code);
        }

        public static void Judge(JudgeTask jt)
        {
            CheckPath(jt.ID);

            if (!FileExisted(jt.DataID))
            {
                DownloadFile(jt.DataID);
            }

            //
            MakeCodeFile(jt.ID, jt.Code, (int)jt.CodeLanguage, Mode.Main);

            //编译选手程序
            if (!Compile(jt.ID, (int)jt.CodeLanguage, Mode.Main))
                return;

            MakeCodeFile(jt.ID, jt.SpecialJudgeCode, (int)jt.SpecialJudgeCodeLanguage, Mode.Main);

            //编译SPJ
            if (!string.IsNullOrEmpty(jt.SpecialJudgeCode))
            {
                if (!Compile(jt.ID, (int)jt.SpecialJudgeCodeLanguage, Mode.Spj))
                {
                    return;
                }
            }
            else
            {
                File.Copy(Program.LibPath + @"\CodeComb.Validator.exe", Program.TempPath + @"\" + jt.ID + @"\Spj.exe", true);
            }

            //准备输入数据
            File.Copy(Program.DataPath + @"\" + jt.DataID + @"\input.txt", Program.TempPath + @"\" + jt.ID + @"\input.txt", true);

            long ExitCode;
            JudgeFeedback jfb = new JudgeFeedback()
            {
                ID = jt.ID,
                MemoryUsage = 0,
                TimeUsage = 0,
                Success = false
            };

            //运行选手程序
            if (!Run(jt.ID, (int)jt.CodeLanguage, jt.TimeLimit, jt.MemoryLimit, Mode.Main, out ExitCode, ref jfb))
                return;

            //准备输出数据
            File.Copy(Program.DataPath + @"\" + jt.DataID + @"\output.txt", Program.TempPath + @"\" + jt.ID + @"\output.txt", true);

            if (!string.IsNullOrEmpty(jt.SpecialJudgeCode))
            {
                if (!Run(jt.ID, (int)jt.SpecialJudgeCodeLanguage, jt.TimeLimit, jt.MemoryLimit, Mode.Spj, out ExitCode, ref jfb))
                    return;
            }
            else
            {
                if (!Run(jt.ID, (int)Entity.Language.Cxx, jt.TimeLimit, jt.MemoryLimit, Mode.Spj, out ExitCode, ref jfb))
                    return;
            }

            //校验结果
            Validate(jt.ID, ExitCode, jfb);
        }

        public static bool Compile(int id, int language_id, Mode Mode)
        {
            JudgeFeedback jfb = new JudgeFeedback()
            {
                ID = id,
                MemoryUsage = 0,
                TimeUsage = 0,
                Result = Entity.JudgeResult.CompileError,
                Success = false,
            };
            if(CompileArgs[language_id] == "") return true;
            Process p = new Process();
            p.StartInfo.FileName = Program.LibPath + @"\CodeComb.Core.exe";
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.WorkingDirectory = Program.TempPath + @"\" + id;
            p.Start();
            p.StandardInput.WriteLine(CompileArgs[language_id].Replace("{Name}", Mode.ToString()));
            p.StandardInput.WriteLine("");
            p.StandardInput.WriteLine("compile.out");
            p.StandardInput.WriteLine("compile.err");
            p.StandardInput.WriteLine("");
            p.StandardInput.WriteLine(CompileTimeLimit);
            p.StandardInput.WriteLine(128 * 1024);
            p.StandardInput.WriteLine(1000);
            p.StandardInput.Close();
            p.WaitForExit();
            var ResultAsString = p.StandardOutput.ReadToEnd();
            JavaScriptSerializer jss = new JavaScriptSerializer();
            var Result = jss.Deserialize<Result>(ResultAsString);
            if (Result.TimeUsage > CompileTimeLimit)
            {
                if (Mode == JudgeHelper.Mode.Range)
                {
                    jfb.Hint = "数据范围校验器编译超时";
                    jfb.Result = Entity.JudgeResult.SystemError;
                }
                else if (Mode == JudgeHelper.Mode.Spj)
                {
                    jfb.Hint = "特殊比较器编译超时";
                    jfb.Result = Entity.JudgeResult.SystemError;
                }
                else if (Mode == JudgeHelper.Mode.Std)
                {
                    jfb.Hint = "标程编译超时";
                    jfb.Result = Entity.JudgeResult.SystemError;
                }
                else
                {
                    jfb.Hint = "选手程序编译超时";
                }
                Feedback(jfb);
                return false;
            }
            else
            {
                if (Result.ExitCode != 0)
                {
                    jfb.Hint = File.ReadAllText(Program.TempPath + @"\" + jfb.ID + @"\compile.out") + File.ReadAllText(Program.TempPath + @"\" + jfb.ID + @"\compile.err");
                    if (Mode == JudgeHelper.Mode.Range)
                    {
                        jfb.Hint = "数据范围校验器编译失败\n" + jfb.Hint;
                        jfb.Result = Entity.JudgeResult.SystemError;
                    }
                    else if (Mode == JudgeHelper.Mode.Spj)
                    {
                        jfb.Hint = "特殊比较器编译失败\n" + jfb.Hint;
                        jfb.Result = Entity.JudgeResult.SystemError;
                    }
                    else if (Mode == JudgeHelper.Mode.Std)
                    {
                        jfb.Hint = "标程编译失败\n" + jfb.Hint;
                        jfb.Result = Entity.JudgeResult.SystemError;
                    }
                    else
                    {
                        jfb.Hint = "选手程序编译失败\n" + jfb.Hint;
                    }
                    jfb.Hint = System.Web.HttpUtility.HtmlEncode(jfb.Hint);
                    Feedback(jfb);
                    return false;
                }
                else return true;
            }
        }

        public static bool Run(int id, int language_id,int time,int memory, Mode Mode, out long ExitCode, ref JudgeFeedback JudgeFeedBack)
        {
            Process p = new Process();
            p.StartInfo.FileName = Program.LibPath + @"\CodeComb.Core.exe";
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.WorkingDirectory = Program.TempPath + @"\" + id;
            p.Start();
            if (Mode == JudgeHelper.Mode.Spj)
                p.StandardInput.WriteLine(ExcuteArgs[language_id].Replace("{Name}", Mode.ToString()) + SpjArgs);
            else
                p.StandardInput.WriteLine(ExcuteArgs[language_id].Replace("{Name}", Mode.ToString()));
            if(Mode == JudgeHelper.Mode.Main || Mode == JudgeHelper.Mode.Range || Mode == JudgeHelper.Mode.Std)
                p.StandardInput.WriteLine("input.txt");
            else if (Mode == JudgeHelper.Mode.Spj)
                p.StandardInput.WriteLine("");
            p.StandardInput.WriteLine(Mode.ToString() + ".out");
            p.StandardInput.WriteLine("");
            p.StandardInput.WriteLine("");
            p.StandardInput.WriteLine(time);
            p.StandardInput.WriteLine(memory);
            p.StandardInput.WriteLine(1000);
            p.StandardInput.Close();
            p.WaitForExit();
            var ResultAsString = p.StandardOutput.ReadToEnd();
            JavaScriptSerializer jss = new JavaScriptSerializer();
            var Result = jss.Deserialize<Result>(ResultAsString);
            var TimeUsage = 0;
            var MemoryUsage = 0;
            TimeUsage = Result.TimeUsage;
            if ((Entity.Language)language_id == Entity.Language.Java)
                MemoryUsage = Result.WorkingSet;
            else MemoryUsage = Result.PagedSize;
            if (Mode == JudgeHelper.Mode.Main)
            {
                JudgeFeedBack.TimeUsage = TimeUsage;
                JudgeFeedBack.MemoryUsage = MemoryUsage;
            }
            ExitCode = Result.ExitCode;
            if (!(Result.ExitCode == 0 || Result.ExitCode == 1 && (Entity.Language)language_id == Entity.Language.C || Mode== JudgeHelper.Mode.Spj && Result.ExitCode >=0 && Result.ExitCode <= 3 || Mode == JudgeHelper.Mode.Range && Result.ExitCode >=-1 && Result.ExitCode <=0))
            {
                JudgeFeedBack.Result = Entity.JudgeResult.RuntimeError;
                if (Mode != JudgeHelper.Mode.Main)
                    JudgeFeedBack.Result = Entity.JudgeResult.SystemError;
                JudgeFeedBack.Hint = String.Format(GetModeName(Mode) + "运行时崩溃");
                Feedback(JudgeFeedBack);
                return false;
            }
            if (TimeUsage > time )
            {
                JudgeFeedBack.Result = Entity.JudgeResult.TimeLimitExceeded;
                if(Mode != JudgeHelper.Mode.Main)
                    JudgeFeedBack.Result = Entity.JudgeResult.SystemError;
                JudgeFeedBack.Hint = String.Format(GetModeName(Mode) + "用时 {0}ms，超出了题目规定时间{1}ms", TimeUsage, time);
                Feedback(JudgeFeedBack);
                return false;
            }
            if (MemoryUsage > memory && Mode == JudgeHelper.Mode.Main)
            {
                JudgeFeedBack.Result = Entity.JudgeResult.MemoryLimitExceeded;
                if (Mode != JudgeHelper.Mode.Main)
                    JudgeFeedBack.Result = Entity.JudgeResult.SystemError;
                JudgeFeedBack.Hint = String.Format(GetModeName(Mode) + "使用空间 {0}KiB，超出了题目规定空间{1}KiB", MemoryUsage, memory);
                Feedback(JudgeFeedBack);
                return false;
            }
            if (Mode == JudgeHelper.Mode.Spj)
                JudgeFeedBack.Hint = File.ReadAllText(Program.TempPath + @"\" + id + @"\Spj.out", Encoding.GetEncoding(936));
            return true;
        }

        public static void Validate(int id, long ExitCode, JudgeFeedback jfb)
        {
            jfb.Result = (Entity.JudgeResult)ExitCode;
            Feedback(jfb);
        }

        private static string GetFileHash(int ID)
        {
            var task = Program.hubJudge.Invoke<string>("GetTestCaseHash", ID);
            task.Wait();
            var result = task.Result;
            return result;
        }

        private static string SHA1(string source)
        {
            return FormsAuthentication.HashPasswordForStoringInConfigFile(source, "SHA1");
        }

        public static bool FileExisted(int ID)
        {
            if (File.Exists(Program.DataPath + "\\" + ID + "\\input.txt") && File.Exists(Program.DataPath + "\\" + ID + "\\output.txt"))
            {
                var hash_server = GetFileHash(ID);
                var hash_local = SHA1(File.ReadAllText(Program.DataPath + "\\" + ID + "\\input.txt"));
                if (hash_local == hash_server)
                    return true;
                else
                    return false;
            }
            else
            {
                return false;
            }
        }

        public static void DownloadFile(int ID)
        {
            var task = Program.hubJudge.Invoke<UploadTask>("GetTestCase", ID);
            task.Wait();
            var result = task.Result;
            if (!Directory.Exists(Program.DataPath + "\\" + result.ID))
                Directory.CreateDirectory(Program.DataPath + "\\" + result.ID);
            File.WriteAllText(Program.DataPath + "\\" + result.ID + "\\input.txt", result.Input);
            if (result.HasOutput)
                File.WriteAllText(Program.DataPath + "\\" + result.ID + "\\output.txt", result.Output);
        }

        public static void Feedback(JudgeFeedback jfb)
        {
            Program.hubJudge.Invoke("JudgeFeedBack", jfb);
        }

        public enum Mode {Main, Spj, Range, Std};

        public static string GetModeName(Mode mode)
        {
            switch (mode)
            {
                case Mode.Main: return "选手程序";
                case Mode.Range: return "范围校验器";
                case Mode.Spj: return "特殊比较器";
                case Mode.Std: return "标程";
                default: return "";
            }
        }
    }
}
