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
        public static string Username, Password, TempPath, DataPath, LibPath, Host;
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
                System.Threading.Thread thread = new System.Threading.Thread(() => 
                {
                    JudgeHelper.Judge(jt);
                });
                thread.Start();
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
