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
        public static string Username, Password, TempPath, DataPath, LibPath;
        static Program()
        {
            HubConnection = new HubConnection(ConfigurationManager.AppSettings["Host"]);
            hubJudge = HubConnection.CreateHubProxy("judgeHub");
            Username = ConfigurationManager.AppSettings["Username"];
            Password = ConfigurationManager.AppSettings["Password"];
            TempPath = ConfigurationManager.AppSettings["TempPath"];
            DataPath = ConfigurationManager.AppSettings["DataPath"];
            LibPath = ConfigurationManager.AppSettings["LibPath"];
        }
        static void Main(string[] args)
        {
            hubJudge.On<JudgeTask>("Judge", jt =>
            {
                JudgeHelper.Judge(jt);
            });
            hubJudge.On<JudgeTask>("onMessage", msg =>
            {
                Console.WriteLine(msg);
            });
            HubConnection.Start().Wait();
            HubConnection.Reconnected += HubConnection_Reconnected;
            hubJudge.Invoke("Auth", Username, Password).Wait();
            string cmd;
            while (true)
            {
                cmd = Console.ReadLine();
            }
        }

        static void HubConnection_Reconnected()
        {
            hubJudge.Invoke("Auth", Username, Password).Wait();
        }
    }
}
