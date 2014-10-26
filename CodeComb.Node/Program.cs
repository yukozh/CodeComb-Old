using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using CodeComb.Judge.Models;
using Microsoft.AspNet.SignalR.Client;

namespace CodeComb.Node
{
    static class Program
    {
        public static HubConnection HubConnection;
        public static IHubProxy hubJudge;
        public static readonly string Username, Password, TempPath, DataPath, LibPath, Host;
        public static readonly string GccInclude, GccBin, FpcBin, JavaBin, Python33Bin, Python27Bin, RubyBin, Net4Bin, FscBin;
        public static readonly Identity LocalAuth;
        public static readonly int MaxThreads;
        public static readonly int CompileTimeLimit;
        public static int CurrentThreads = 0;
        static Program()
        {
            Host = ConfigurationManager.AppSettings["Host"];
            HubConnection = new HubConnection(Host);
            hubJudge = HubConnection.CreateHubProxy("judgeHub");
            Username = ConfigurationManager.AppSettings["Username"];
            Password = ConfigurationManager.AppSettings["Password"];
            TempPath = ConfigurationManager.AppSettings["TempPath"];
            DataPath = ConfigurationManager.AppSettings["DataPath"];
            LibPath = ConfigurationManager.AppSettings["LibPath"];
            GccInclude = ConfigurationManager.AppSettings["GCC_INCLUDE"];
            GccBin = ConfigurationManager.AppSettings["GCC_BIN"];
            FpcBin = ConfigurationManager.AppSettings["FPC_BIN"];
            JavaBin = ConfigurationManager.AppSettings["JAVA_BIN"];
            Python33Bin = ConfigurationManager.AppSettings["PYTHON33_BIN"];
            Python27Bin = ConfigurationManager.AppSettings["PYTHON27_BIN"];
            RubyBin = ConfigurationManager.AppSettings["RUBY_BIN"];
            Net4Bin = ConfigurationManager.AppSettings["NET4_BIN"];
            FscBin = ConfigurationManager.AppSettings["FSC_BIN"];
            CompileTimeLimit = Convert.ToInt32(ConfigurationManager.AppSettings["CompileTimeLimit"]);
            MaxThreads = Convert.ToInt32(ConfigurationManager.AppSettings["Threads"]);
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["LocalUsername"]))
            {
                LocalAuth = new Identity();
                LocalAuth.Username = ConfigurationManager.AppSettings["LocalUsername"];
                LocalAuth.Password = ConfigurationManager.AppSettings["LocalPassword"];
            }
        }
        static void KeepAlive()
         {
             while (true)
             {
                 System.Threading.Thread.Sleep(1000);
             }
         }
        static void Main(string[] args)
        {
            Console.WriteLine(@"
   *****               **                 *****                       **     
 *******               **               *******                       **     
 **    *               **               **    *                       **     
**         ****    *** **    ****      **         ****   ** *** ***   ** *** 
**        ******   ******   ******     **        ******  ***********  *******
**       **   **  **   **  **   **     **       **   **  **  ***  **  **   **
**       **   **  **   **  *******     **       **   **  **  ***  **  **   **
***    * **   **  **   **  **          ***    * **   **  **  ***  **  **   **
 ******* ******   *******  ******       ******* ******   **  ***  **  ****** 
   *****  ****     *** **   *****         *****  ****    **  ***  **  ** *** 



Code Comb Node");
            Console.WriteLine("Host: {0}", Host);
            Console.WriteLine("User: {0}", Username);

            hubJudge.On<JudgeTask>("Judge", jt =>
            {
                System.Threading.Tasks.Task.Factory.StartNew(() => 
                {
                    JudgeHelper.Judge(jt);
                });
            });
            hubJudge.On<HackTask>("Hack", ht =>
            {
                System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    HackHelper.Hack(ht);
                });
            });
            hubJudge.On<string>("onMessage", msg =>
            {
                Console.WriteLine("{0} {1}", DateTime.Now.ToString("HH:mm:ss"), msg);
            });
            hubJudge.On("onAuthConflicted", () =>
            {
                Console.WriteLine("{0} {1}", DateTime.Now.ToString("HH:mm:ss"), "本评测机帐号在异地登陆，导致本节点连接终止，如果不是您本人操作，请及时修改密码！");
            });
            HubConnection.TraceLevel = TraceLevels.Events;
            HubConnection.TraceWriter = Console.Out;
            HubConnection.TransportConnectTimeout = TimeSpan.FromDays(30);
            HubConnection.Start().Wait();
            HubConnection.Closed += HubConnection_Closed;
            HubConnection.Reconnected += HubConnection_Reconnected;
            hubJudge.Invoke("Auth", Username, Password, MaxThreads).Wait();
            System.Threading.Thread t = new System.Threading.Thread(KeepAlive);
            t.Start();
        }

        static void HubConnection_Reconnected()
        {
            hubJudge.Invoke("Auth", Username, Password, MaxThreads);
            Console.WriteLine("Reauthentication on reconnected.");
        }

        static void HubConnection_Closed()
        {
            Task.Factory.StartNew(() => 
            {
                bool success = false;
                while (!success)
                {
                    try
                    {
                        HubConnection.Start().Wait();
                        Console.WriteLine("SignalR is restarting.");
                        hubJudge.Invoke("Auth", Username, Password, MaxThreads);
                        Console.WriteLine("Reauthentication on started.");
                    }
                    catch
                    {
                        System.Threading.Thread.Sleep(5000);
                        continue;
                    }
                    success = true;
                }
            });
        }
    }
}
