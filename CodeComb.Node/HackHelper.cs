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
    public class HackHelper
    {
        public const int CompileTimeLimit = 3000;
        public static string[] FileNames = 
        { 
            "{Name}.c",
            "{Name}.cpp",
            "{Name}.cpp",
            "{Name}.java",
            "{Name}.pas",
            "{Name}.py",
            "{Name}.py",
            "{Name}.rb",
            "{Name}.cs",
            "{Name}.vb",
            "{Name}.fs"
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
            "csc {Name}.cs", 
            "vbc {Name}.vb",
            "fsc {Name}.fs" 
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
            "{Name}.exe",
            "{Name}.exe"
        };
        public const string SpjArgs = " Std.out Main.out input.txt";
        public static void CheckPath(int id)
        {
            if (!Directory.Exists(Program.TempPath + @"\h" + id + @"\"))
                Directory.CreateDirectory(Program.TempPath + @"\h" + id + @"\");
        }

        public static void MakeCodeFile(int id, string code, int language_id, Mode mode)
        {
            if (!File.Exists(Program.TempPath + @"\h" + id + @"\" + FileNames[language_id].Replace("{Name}", mode.ToString())))
                File.WriteAllText(Program.TempPath + @"\h" + id + @"\" + FileNames[language_id].Replace("{Name}", mode.ToString()), code);
        }

        public static void Hack(HackTask ht)
        {
            while (Program.CurrentThreads >= Program.MaxThreads)
            {
                System.Threading.Thread.Sleep(500);
            }

            Program.CurrentThreads++;

            HackFeedback hfb = new HackFeedback()
            {
                HackID = ht.HackID,
                StatusID = ht.StatusID,
                MemoryUsage = 0,
                TimeUsage = 0,
                Success = false
            };

            try
            {

                long ExitCode;

                CheckPath(ht.HackID);

                if (!Directory.Exists(Program.TempPath + @"\h" + ht.HackID))
                    Directory.CreateDirectory(Program.TempPath + @"\h" + ht.HackID);

                //制作Custom数据
                if (string.IsNullOrEmpty(ht.DataMakerCode))
                {
                    File.WriteAllText(Program.TempPath + @"\h" + ht.HackID + @"\input.txt", ht.InputData);
                }
                else
                {
                    File.WriteAllText(Program.TempPath + @"\h" + ht.HackID + @"\" + FileNames[(int)ht.DataMakerCodeLanguage].Replace("{Name}", Mode.Std.ToString()), ht.DataMakerCode);
                    if (!Compile(ht.HackID, (int)ht.DataMakerCodeLanguage, Mode.DataMaker, ref hfb))
                        return;
                    if (!Run(ht.HackID, (int)ht.DataMakerCodeLanguage, ht.TimeLimit, ht.MemoryLimit, Mode.DataMaker, out ExitCode, ref hfb))
                        return;
                }

                //编译并运行数据范围校验器
                File.WriteAllText(Program.TempPath + @"\h" + ht.HackID + @"\" + FileNames[(int)ht.RangeValidatorCodeLanguage].Replace("{Name}", Mode.Range.ToString()), ht.RangeValidatorCode);
                if (!Compile(ht.HackID, (int)ht.RangeValidatorCodeLanguage, Mode.Range, ref hfb))
                    return;
                if (!Run(ht.HackID, (int)ht.RangeValidatorCodeLanguage, ht.TimeLimit, ht.MemoryLimit, Mode.Range, out ExitCode, ref hfb))
                    return;

                //编译并运行标程
                File.WriteAllText(Program.TempPath + @"\h" + ht.HackID + @"\" + FileNames[(int)ht.StandardCodeLanguage].Replace("{Name}", Mode.Std.ToString()), ht.StandardCode);
                if (!Compile(ht.HackID, (int)ht.StandardCodeLanguage, Mode.Std, ref hfb))
                    return;
                if (!Run(ht.HackID, (int)ht.StandardCodeLanguage, ht.TimeLimit, ht.MemoryLimit, Mode.Std, out ExitCode, ref hfb))
                    return;

                //
                MakeCodeFile(ht.HackID, ht.Code, (int)ht.CodeLanguage, Mode.Main);

                //编译选手程序
                if (!Compile(ht.HackID, (int)ht.CodeLanguage, Mode.Main, ref hfb))
                    return;

                //编译SPJ
                if (!string.IsNullOrEmpty(ht.SpecialJudgeCode))
                {
                    File.WriteAllText(Program.TempPath + @"\h" + ht.HackID + @"\" + FileNames[(int)ht.SpecialJudgeCodeLanguage].Replace("{Name}", Mode.Spj.ToString()), ht.SpecialJudgeCode);
                    if (!Compile(ht.HackID, (int)ht.SpecialJudgeCodeLanguage, Mode.Spj, ref hfb))
                    {
                        return;
                    }
                }
                else
                {
                    File.Copy(Program.LibPath + @"\CodeComb.Validator.exe", Program.TempPath + @"\h" + ht.HackID + @"\Spj.exe", true);
                }

                //运行选手程序
                if (!Run(ht.HackID, (int)ht.CodeLanguage, ht.TimeLimit, ht.MemoryLimit, Mode.Main, out ExitCode, ref hfb))
                    return;

                if (!string.IsNullOrEmpty(ht.SpecialJudgeCode))
                {
                    if (!Run(ht.HackID, (int)ht.SpecialJudgeCodeLanguage, ht.TimeLimit, ht.MemoryLimit, Mode.Spj, out ExitCode, ref hfb))
                        return;
                }
                else
                {
                    if (!Run(ht.HackID, (int)Entity.Language.Cxx, ht.TimeLimit, ht.MemoryLimit, Mode.Spj, out ExitCode, ref hfb))
                        return;
                }

                //校验结果
                if (ExitCode != 0)
                {
                    hfb.Result = Entity.HackResult.Success;
                    hfb.JudgeResult = (Entity.JudgeResult)ExitCode;
                    hfb.Output = File.ReadAllText(Program.TempPath + @"\h" + ht.HackID + @"\Std.out");
                }
                else
                {
                    hfb.Result = Entity.HackResult.Failure;
                }
                Feedback(hfb);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                hfb.Hint = System.Web.HttpUtility.HtmlEncode(ex.ToString());
                hfb.Result = Entity.HackResult.SystemError;
                Program.CurrentThreads--;
                Feedback(hfb);
            }
        }

        public static bool Compile(int id, int language_id, Mode Mode, ref HackFeedback hfb)
        {
            if(CompileArgs[language_id] == "") return true;
            Process p = new Process();
            p.StartInfo.FileName = Program.LibPath + @"\CodeComb.Core.exe";
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.WorkingDirectory = Program.TempPath + @"\h" + id;
            if (Program.LocalAuth != null)
            {
                p.StartInfo.UserName = Program.LocalAuth.Username;
                p.StartInfo.Password = Program.LocalAuth.secPassword;
            }
            if (!string.IsNullOrEmpty(Program.GccInclude))
            {
                if (p.StartInfo.EnvironmentVariables["C_INCLUDE_PATH"] == null)
                    p.StartInfo.EnvironmentVariables["C_INCLUDE_PATH"] = Program.GccInclude;
                else
                    p.StartInfo.EnvironmentVariables["C_INCLUDE_PATH"] += ";" + Program.GccInclude;
                if (p.StartInfo.EnvironmentVariables["CPLUS_INCLUDE_PATH"] == null)
                    p.StartInfo.EnvironmentVariables["CPLUS_INCLUDE_PATH"] = Program.GccInclude;
                else
                    p.StartInfo.EnvironmentVariables["CPLUS_INCLUDE_PATH"] += ";" + Program.GccInclude;
            }
            if (!string.IsNullOrEmpty(Program.GccBin))
            {
                if (p.StartInfo.EnvironmentVariables["PATH"] == null)
                    p.StartInfo.EnvironmentVariables["PATH"] = Program.GccBin;
                else
                    p.StartInfo.EnvironmentVariables["PATH"] += ";" + Program.GccBin;
            }
            if (!string.IsNullOrEmpty(Program.FpcBin))
            {
                if (p.StartInfo.EnvironmentVariables["PATH"] == null)
                    p.StartInfo.EnvironmentVariables["PATH"] = Program.FpcBin;
                else
                    p.StartInfo.EnvironmentVariables["PATH"] += ";" + Program.FpcBin;
            }
            if (!string.IsNullOrEmpty(Program.JavaBin))
            {
                if (p.StartInfo.EnvironmentVariables["PATH"] == null)
                    p.StartInfo.EnvironmentVariables["PATH"] = Program.JavaBin;
                else
                    p.StartInfo.EnvironmentVariables["PATH"] += ";" + Program.JavaBin;
            }
            if (!string.IsNullOrEmpty(Program.Python27Bin))
            {
                if (p.StartInfo.EnvironmentVariables["PATH"] == null)
                    p.StartInfo.EnvironmentVariables["PATH"] = Program.Python27Bin;
                else
                    p.StartInfo.EnvironmentVariables["PATH"] += ";" + Program.Python27Bin;
            }
            if (!string.IsNullOrEmpty(Program.Python33Bin))
            {
                if (p.StartInfo.EnvironmentVariables["PATH"] == null)
                    p.StartInfo.EnvironmentVariables["PATH"] = Program.Python33Bin;
                else
                    p.StartInfo.EnvironmentVariables["PATH"] += ";" + Program.Python33Bin;
            }
            if (!string.IsNullOrEmpty(Program.RubyBin))
            {
                if (p.StartInfo.EnvironmentVariables["PATH"] == null)
                    p.StartInfo.EnvironmentVariables["PATH"] = Program.RubyBin;
                else
                    p.StartInfo.EnvironmentVariables["PATH"] += ";" + Program.RubyBin;
            }
            if (!string.IsNullOrEmpty(Program.Net4Bin))
            {
                if (p.StartInfo.EnvironmentVariables["PATH"] == null)
                    p.StartInfo.EnvironmentVariables["PATH"] = Program.Net4Bin;
                else
                    p.StartInfo.EnvironmentVariables["PATH"] += ";" + Program.Net4Bin;
            }
            if (!string.IsNullOrEmpty(Program.FscBin))
            {
                if (p.StartInfo.EnvironmentVariables["PATH"] == null)
                    p.StartInfo.EnvironmentVariables["PATH"] = Program.FscBin;
                else
                    p.StartInfo.EnvironmentVariables["PATH"] += ";" + Program.FscBin;
            }
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
                if (Mode == HackHelper.Mode.Range)
                {
                    hfb.Hint = "数据范围校验器编译超时";
                    hfb.Result = Entity.HackResult.SystemError;
                }
                else if (Mode == HackHelper.Mode.Spj)
                {
                    hfb.Hint = "比较器编译超时";
                    hfb.Result = Entity.HackResult.SystemError;
                }
                else if (Mode == HackHelper.Mode.Std)
                {
                    hfb.Hint = "标程编译超时";
                    hfb.Result = Entity.HackResult.SystemError;
                }
                else if (Mode == HackHelper.Mode.DataMaker)
                {
                    hfb.Hint = "数据产生器编译超时";
                    hfb.Result = Entity.HackResult.DatamakerError;
                }
                else
                {
                    hfb.Hint = "选手程序编译超时";
                    hfb.Result = Entity.HackResult.SystemError;
                }
                Feedback(hfb);
                return false;
            }
            else
            {
                if (Result.ExitCode != 0)
                {
                    hfb.Hint = File.ReadAllText(Program.TempPath + @"\h" + hfb.HackID + @"\compile.out") + File.ReadAllText(Program.TempPath + @"\h" + hfb.HackID + @"\compile.err");
                    if (Mode == HackHelper.Mode.Range)
                    {
                        hfb.Hint = "数据范围校验器编译失败\n" + hfb.Hint;
                        hfb.Result = Entity.HackResult.SystemError;
                    }
                    else if (Mode == HackHelper.Mode.Spj)
                    {
                        hfb.Hint = "比较器编译失败\n" + hfb.Hint;
                        hfb.Result = Entity.HackResult.SystemError;
                    }
                    else if (Mode == HackHelper.Mode.Std)
                    {
                        hfb.Hint = "标程编译失败\n" + hfb.Hint;
                        hfb.Result = Entity.HackResult.SystemError;
                    }
                    else if (Mode == HackHelper.Mode.DataMaker)
                    {
                        hfb.Hint = "数据产生器编译失败\n" + hfb.Hint;
                        hfb.Result = Entity.HackResult.DatamakerError;
                    }
                    else
                    {
                        hfb.Hint = "选手程序编译失败\n" + hfb.Hint;
                        hfb.Result = Entity.HackResult.SystemError;
                    }
                    hfb.Hint = System.Web.HttpUtility.HtmlEncode(hfb.Hint);
                    Feedback(hfb);
                    return false;
                }
                else return true;
            }
        }

        public static bool Run(int id, int language_id, int time, int memory, Mode Mode, out long ExitCode, ref HackFeedback hfb)
        {
            Process p = new Process();
            p.StartInfo.FileName = Program.LibPath + @"\CodeComb.Core.exe";
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.WorkingDirectory = Program.TempPath + @"\h" + id;
            if (Program.LocalAuth != null)
            {
                p.StartInfo.UserName = Program.LocalAuth.Username;
                p.StartInfo.Password = Program.LocalAuth.secPassword;
            } if (!string.IsNullOrEmpty(Program.GccInclude))
            {
                if (p.StartInfo.EnvironmentVariables["C_INCLUDE_PATH"] == null)
                    p.StartInfo.EnvironmentVariables["C_INCLUDE_PATH"] = Program.GccInclude;
                else
                    p.StartInfo.EnvironmentVariables["C_INCLUDE_PATH"] += ";" + Program.GccInclude;
                if (p.StartInfo.EnvironmentVariables["CPLUS_INCLUDE_PATH"] == null)
                    p.StartInfo.EnvironmentVariables["CPLUS_INCLUDE_PATH"] = Program.GccInclude;
                else
                    p.StartInfo.EnvironmentVariables["CPLUS_INCLUDE_PATH"] += ";" + Program.GccInclude;
            }
            if (!string.IsNullOrEmpty(Program.GccBin))
            {
                if (p.StartInfo.EnvironmentVariables["PATH"] == null)
                    p.StartInfo.EnvironmentVariables["PATH"] = Program.GccBin;
                else
                    p.StartInfo.EnvironmentVariables["PATH"] += ";" + Program.GccBin;
            }
            if (!string.IsNullOrEmpty(Program.FpcBin))
            {
                if (p.StartInfo.EnvironmentVariables["PATH"] == null)
                    p.StartInfo.EnvironmentVariables["PATH"] = Program.FpcBin;
                else
                    p.StartInfo.EnvironmentVariables["PATH"] += ";" + Program.FpcBin;
            }
            if (!string.IsNullOrEmpty(Program.JavaBin))
            {
                if (p.StartInfo.EnvironmentVariables["PATH"] == null)
                    p.StartInfo.EnvironmentVariables["PATH"] = Program.JavaBin;
                else
                    p.StartInfo.EnvironmentVariables["PATH"] += ";" + Program.JavaBin;
            }
            if (!string.IsNullOrEmpty(Program.Python27Bin))
            {
                if (p.StartInfo.EnvironmentVariables["PATH"] == null)
                    p.StartInfo.EnvironmentVariables["PATH"] = Program.Python27Bin;
                else
                    p.StartInfo.EnvironmentVariables["PATH"] += ";" + Program.Python27Bin;
            }
            if (!string.IsNullOrEmpty(Program.Python33Bin))
            {
                if (p.StartInfo.EnvironmentVariables["PATH"] == null)
                    p.StartInfo.EnvironmentVariables["PATH"] = Program.Python33Bin;
                else
                    p.StartInfo.EnvironmentVariables["PATH"] += ";" + Program.Python33Bin;
            }
            if (!string.IsNullOrEmpty(Program.RubyBin))
            {
                if (p.StartInfo.EnvironmentVariables["PATH"] == null)
                    p.StartInfo.EnvironmentVariables["PATH"] = Program.RubyBin;
                else
                    p.StartInfo.EnvironmentVariables["PATH"] += ";" + Program.RubyBin;
            }
            if (!string.IsNullOrEmpty(Program.Net4Bin))
            {
                if (p.StartInfo.EnvironmentVariables["PATH"] == null)
                    p.StartInfo.EnvironmentVariables["PATH"] = Program.Net4Bin;
                else
                    p.StartInfo.EnvironmentVariables["PATH"] += ";" + Program.Net4Bin;
            }
            if (!string.IsNullOrEmpty(Program.FscBin))
            {
                if (p.StartInfo.EnvironmentVariables["PATH"] == null)
                    p.StartInfo.EnvironmentVariables["PATH"] = Program.FscBin;
                else
                    p.StartInfo.EnvironmentVariables["PATH"] += ";" + Program.FscBin;
            }
            p.Start();
            if (Mode == HackHelper.Mode.Spj)
                p.StandardInput.WriteLine(ExcuteArgs[language_id].Replace("{Name}", Mode.ToString()) + SpjArgs);
            else
                p.StandardInput.WriteLine(ExcuteArgs[language_id].Replace("{Name}", Mode.ToString()));
            if (Mode == HackHelper.Mode.Main || Mode == HackHelper.Mode.Range || Mode == HackHelper.Mode.Std)
                p.StandardInput.WriteLine("input.txt");
            else
                p.StandardInput.WriteLine("");
            if (Mode == HackHelper.Mode.DataMaker)
                p.StandardInput.WriteLine("input.txt");
            else
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
            if (Mode == HackHelper.Mode.Main)
            {
                hfb.TimeUsage = TimeUsage;
                hfb.MemoryUsage = MemoryUsage;
            }
            ExitCode = Result.ExitCode;
            if (!(Result.ExitCode == 0 || Result.ExitCode == 1 && (Entity.Language)language_id == Entity.Language.C || Mode == HackHelper.Mode.Spj && Result.ExitCode >= 0 && Result.ExitCode <= 3 || Mode == HackHelper.Mode.Range && Result.ExitCode >= -1 && Result.ExitCode <= 0))
            {
                if (Mode == HackHelper.Mode.DataMaker)
                {
                    hfb.Result = Entity.HackResult.DatamakerError;
                    hfb.Hint = String.Format(GetModeName(Mode) + "运行时崩溃");
                    Feedback(hfb);
                    return false;
                }
                else if (Mode == HackHelper.Mode.Main)
                {
                    hfb.Result = Entity.HackResult.Success;
                    hfb.JudgeResult = Entity.JudgeResult.RuntimeError;
                    hfb.Output = File.ReadAllText(Program.TempPath + @"\h" + id + @"\Std.out");
                    hfb.Hint = String.Format(GetModeName(Mode) + "运行时崩溃");
                    Feedback(hfb);
                    return false;
                }
                else
                {
                    hfb.Result = Entity.HackResult.SystemError;
                    hfb.Hint = String.Format(GetModeName(Mode) + "运行时崩溃");
                    Feedback(hfb);
                    return false;
                }
            }
            if (TimeUsage > time )
            {
                if (Mode == HackHelper.Mode.DataMaker)
                {
                    hfb.Result = Entity.HackResult.DatamakerError;
                    hfb.Hint = String.Format(GetModeName(Mode) + "用时 {0}ms，超出了题目规定时间{1}ms", TimeUsage, time);
                    Feedback(hfb);
                    return false;
                }
                else if (Mode == HackHelper.Mode.Main)
                {
                    hfb.Result = Entity.HackResult.Success;
                    hfb.JudgeResult = Entity.JudgeResult.TimeLimitExceeded;
                    hfb.Output = File.ReadAllText(Program.TempPath + @"\h" + id + @"\Std.out");
                    hfb.Hint = String.Format(GetModeName(Mode) + "用时 {0}ms，超出了题目规定时间{1}ms", TimeUsage, time);
                    Feedback(hfb);
                    return false;
                }
                else
                {
                    hfb.Result = Entity.HackResult.SystemError;
                    hfb.Hint = String.Format(GetModeName(Mode) + "用时 {0}ms，超出了题目规定时间{1}ms", TimeUsage, time);
                    Feedback(hfb);
                    return false;
                }
            }
            if (MemoryUsage > memory && Mode == HackHelper.Mode.Main)
            {
                hfb.Result = Entity.HackResult.Success;
                hfb.JudgeResult = Entity.JudgeResult.MemoryLimitExceeded;
                hfb.Hint = String.Format(GetModeName(Mode) + "使用空间 {0}KiB，超出了题目规定空间{1}KiB", MemoryUsage, memory);
                Feedback(hfb);
                return false;
            }
            if (Mode == HackHelper.Mode.Spj)
                hfb.Hint = File.ReadAllText(Program.TempPath + @"\h" + id + @"\Spj.out", Encoding.GetEncoding(936));
            return true;
        }

        public static void Feedback(HackFeedback hfb)
        {
            Program.hubJudge.Invoke("HackFeedBack", hfb);
            Program.CurrentThreads--;
        }

        public enum Mode {Main, Spj, Range, Std, DataMaker};

        public static string GetModeName(Mode mode)
        {
            switch (mode)
            {
                case Mode.Main: return "选手程序";
                case Mode.Range: return "范围校验器";
                case Mode.Spj: return "比较器";
                case Mode.Std: return "标程";
                case Mode.DataMaker: return "数据产生器";
                default: return "";
            }
        }
    }
}
