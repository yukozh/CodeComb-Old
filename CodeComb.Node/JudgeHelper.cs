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
            while (Program.CurrentThreads >= Program.MaxThreads)
            {
                System.Threading.Thread.Sleep(500);
            }

            Program.CurrentThreads++;

            JudgeFeedback jfb = new JudgeFeedback()
            {
                ID = jt.ID,
                MemoryUsage = 0,
                TimeUsage = 0,
                Success = false
            };

            try
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

                MakeCodeFile(jt.ID, jt.SpecialJudgeCode, (int)jt.SpecialJudgeCodeLanguage, Mode.Spj);

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
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                jfb.Hint = System.Web.HttpUtility.HtmlEncode(ex.ToString());
                jfb.Result = Entity.JudgeResult.SystemError;
                Program.CurrentThreads--;
                Feedback(jfb);
            }
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
            if (language_id == (int)Entity.Language.Java)
                p.StandardInput.WriteLine(Program.CompileTimeLimit + 2000);
            else
                p.StandardInput.WriteLine(Program.CompileTimeLimit);
            p.StandardInput.WriteLine(128 * 1024);
            p.StandardInput.WriteLine(Program.CompileTimeLimit);
            p.StandardInput.Close();
            p.WaitForExit();
            var ResultAsString = p.StandardOutput.ReadToEnd();
            JavaScriptSerializer jss = new JavaScriptSerializer();
            var Result = jss.Deserialize<Result>(ResultAsString);
            if (Result.TimeUsage > Program.CompileTimeLimit)
            {
                if (Mode == JudgeHelper.Mode.Range)
                {
                    jfb.Hint = "数据范围校验器编译超时";
                    jfb.Result = Entity.JudgeResult.SystemError;
                }
                else if (Mode == JudgeHelper.Mode.Spj)
                {
                    jfb.Hint = "比较器编译超时";
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
                    jfb.Hint = File.ReadAllText(Program.TempPath + @"\" + jfb.ID + @"\compile.out", Encoding.GetEncoding(936)) + File.ReadAllText(Program.TempPath + @"\" + jfb.ID + @"\compile.err", Encoding.GetEncoding(936));
                    if (Mode == JudgeHelper.Mode.Range)
                    {
                        jfb.Hint = "数据范围校验器编译失败\n" + jfb.Hint;
                        jfb.Result = Entity.JudgeResult.SystemError;
                    }
                    else if (Mode == JudgeHelper.Mode.Spj)
                    {
                        jfb.Hint = "比较器编译失败\n" + jfb.Hint;
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
                JudgeFeedBack.Hint = File.ReadAllText(Program.TempPath + @"\" + id + @"\Spj.out", GetType(Program.TempPath + @"\" + id + @"\Spj.out"));
            return true;
        }
        public static System.Text.Encoding GetType(string FILE_NAME)
        {
            FileStream fs = new FileStream(FILE_NAME, FileMode.Open, FileAccess.Read);
            Encoding r = GetType(fs);
            fs.Close();
            return r;
        }
        public static System.Text.Encoding GetType(FileStream fs)
        {
            byte[] Unicode = new byte[] { 0xFF, 0xFE, 0x41 };
            byte[] UnicodeBIG = new byte[] { 0xFE, 0xFF, 0x00 };
            byte[] UTF8 = new byte[] { 0xEF, 0xBB, 0xBF }; //带BOM
            Encoding reVal = Encoding.Default;

            BinaryReader r = new BinaryReader(fs, System.Text.Encoding.Default);
            int i;
            int.TryParse(fs.Length.ToString(), out i);
            byte[] ss = r.ReadBytes(i);
            if (IsUTF8Bytes(ss) || (ss[0] == 0xEF && ss[1] == 0xBB && ss[2] == 0xBF))
            {
                reVal = Encoding.UTF8;
            }
            else if (ss[0] == 0xFE && ss[1] == 0xFF && ss[2] == 0x00)
            {
                reVal = Encoding.BigEndianUnicode;
            }
            else if (ss[0] == 0xFF && ss[1] == 0xFE && ss[2] == 0x41)
            {
                reVal = Encoding.Unicode;
            }
            r.Close();
            return reVal;

        }

        /// <summary>
        /// 判断是否是不带 BOM 的 UTF8 格式
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private static bool IsUTF8Bytes(byte[] data)
        {
            int charByteCounter = 1;　 //计算当前正分析的字符应还有的字节数
            byte curByte; //当前分析的字节.
            for (int i = 0; i < data.Length; i++)
            {
                curByte = data[i];
                if (charByteCounter == 1)
                {
                    if (curByte >= 0x80)
                    {
                        //判断当前
                        while (((curByte <<= 1) & 0x80) != 0)
                        {
                            charByteCounter++;
                        }
                        //标记位首位若为非0 则至少以2个1开始 如:110XXXXX...........1111110X　
                        if (charByteCounter == 1 || charByteCounter > 6)
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    //若是UTF-8 此时第一位必须为1
                    if ((curByte & 0xC0) != 0x80)
                    {
                        return false;
                    }
                    charByteCounter--;
                }
            }
            if (charByteCounter > 1)
            {
                throw new Exception("非预期的byte格式");
            }
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
            Program.CurrentThreads--;
            try
            {
                if (System.IO.Directory.Exists(Program.TempPath + @"\" + jfb.ID))
                    System.IO.Directory.Delete(Program.TempPath + @"\" + jfb.ID, true);
            }
            catch { }
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
