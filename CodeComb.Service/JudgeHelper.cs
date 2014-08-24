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


namespace CodeComb.Service
{
    public static class JudgeHelper
    {
        public const int CompileTimeLimit = 4000;
        public static string[] CompileArgs = 
        { 
            "g++ -O2 -o {Name}.exe -DONLINE_JUDGE -lm --static --std=c99 {Name}.c", 
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

        public static void Judge(JudgeTask jt)
        {
            if (!Directory.Exists(JudgeService.TempPath + @"\" + jt.ID))
                Directory.CreateDirectory(JudgeService.TempPath + @"\" + jt.ID);

            //编译选手程序
            if (!Compile(jt.ID, (int)jt.CodeLanguage, jt.Code, Mode.Main))
                return;

            //编译SPJ
            if (!string.IsNullOrEmpty(jt.SpecialJudgeCode))
            {
                if (!Compile(jt.ID, (int)jt.SpecialJudgeCodeLanguage, jt.SpecialJudgeCode, Mode.Spj))
                {
                    return;
                }
            }
            else
            {
                File.Copy(JudgeService.LibPath + @"\CodeComb.Spj.exe", JudgeService.TempPath + @"\" + jt.ID + @"\Spj.exe", true);
            }

            //准备输入数据
            File.Copy(JudgeService.DataPath + @"\" + jt.DataID + @"\input.txt", JudgeService.TempPath + @"\" + jt.ID + @"\input.txt", true);

            //运行选手程序


            //准备输出数据
            File.Copy(JudgeService.DataPath + @"\" + jt.DataID + @"\output.txt", JudgeService.TempPath + @"\" + jt.ID + @"\output.txt", true);

            

            Run();
            //运行成功
            Validate();
        }
        public static bool Compile(int id, int language_id, string code, Mode Mode)
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
            p.StartInfo.FileName = JudgeService.LibPath + @"\CodeComb.Core.exe";
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.UseShellExecute = false;
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
                    jfb.Hint = File.ReadAllText(JudgeService.TempPath + @"\" + jfb.ID + @"\compile.out") + File.ReadAllText(JudgeService.TempPath + @"\" + jfb.ID + @"\compile.err");
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
                    Feedback(jfb);
                    return false;
                }
                else return true;
            }
        }

        public static bool Run(int id, int language_id, Mode Mode)
        { 
            
        }

        public static void Validate()
        { 
        
        }

        private static string GetFileHash(int ID)
        {
            var task = JudgeService.hubJudge.Invoke<string>("GetTestCaseHash", ID);
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
            if (File.Exists(JudgeService.DataPath + "\\" + ID + "\\input.txt") && File.Exists(JudgeService.DataPath + "\\" + ID + "\\output.txt"))
            {
                var hash_server = GetFileHash(ID);
                var hash_local = SHA1(File.ReadAllText(JudgeService.DataPath + "\\" + ID + "\\input.txt"));
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

        /// <summary>
        /// 下载测试数据
        /// </summary>
        /// <param name="ID"></param>
        public static void DownloadFile(int ID)
        {
            var task = JudgeService.hubJudge.Invoke<UploadTask>("GetTestCase", 1);
            task.Wait();
            var result = task.Result;
            File.WriteAllText(JudgeService.DataPath + "\\" + result.ID + "\\input.txt", result.Input);
            if (result.HasOutput)
                File.WriteAllText(JudgeService.DataPath + "\\" + result.ID + "\\output.txt", result.Output);
        }

        public static void Feedback(JudgeFeedback jfb)
        {
            JudgeService.hubJudge.Invoke("JudgeFeedBack", jfb);
        }

        public enum Mode {Main, Spj, Range, Std};
    }
}
