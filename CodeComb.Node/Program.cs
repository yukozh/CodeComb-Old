using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using CodeComb.Judge.Models;
using Microsoft.AspNet.SignalR.Client;
using System.Configuration;

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
                 System.Threading.Thread.Sleep(0);
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
            HubConnection.TraceLevel = TraceLevels.All;
            HubConnection.TraceWriter = Console.Out;
            HubConnection.Start().Wait();
            HubConnection.ConnectionSlow += HubConnection_ConnectionSlow;
            hubJudge.Invoke("Auth", Username, Password).Wait();
            System.Threading.Thread t = new System.Threading.Thread(KeepAlive);
            t.Start();
        }

        static void HubConnection_ConnectionSlow()
        {
            Console.WriteLine("{0} Connection slow", DateTime.Now.ToString("HH:mm:ss"));
            HubConnection.Start();
        }
    }
}
