using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using CodeComb.Judge.Models;
using Microsoft.AspNet.SignalR.Client;

namespace CodeComb.Service
{
    partial class JudgeService : ServiceBase
    {
        public static HubConnection HubConnection;
        public static IHubProxy hubJudge;
        public static string Username, Password, TempPath, DataPath, LibPath;

        public JudgeService()
        {
            InitializeComponent();
            HubConnection = new HubConnection(ConfigurationManager.AppSettings["Host"]);
            hubJudge = HubConnection.CreateHubProxy("Judge");
            Username = ConfigurationManager.AppSettings["Username"];
            Password = ConfigurationManager.AppSettings["Password"];
            TempPath = ConfigurationManager.AppSettings["TempPath"];
            DataPath = ConfigurationManager.AppSettings["DataPath"];
            LibPath = ConfigurationManager.AppSettings["LibPath"];
        }

        protected override void OnStart(string[] args)
        {
            hubJudge.On<JudgeTask>("Judge", jt => 
            { 
                
            });
            HubConnection.Start().Wait();
            hubJudge.Invoke("Auth", Username, Password).Wait();
        }

        protected override void OnStop()
        {
            // TODO:  在此处添加代码以执行停止服务所需的关闭操作。
        }
    }
}
