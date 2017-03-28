using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using EFWCoreLib.CoreFrame.ProcessManage;

namespace WCFHosting
{
    static class Program
    {
        static FrmHosting host;
        public static efwplusServerIPCManager serverIPC;
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            host = new FrmHosting(ExecCmd);

            Func<string, Dictionary<string,string>, string> _funcExecCmd = ExecCmd;
            Action<string> _actionReceiveData = ((string data) =>
            {
                string text = ("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] : " + data);
                host.showmsg(text);
            });
            serverIPC = new efwplusServerIPCManager(_funcExecCmd, _actionReceiveData);

            MongodbManager.ShowMsg = _actionReceiveData;
            NginxManager.ShowMsg = _actionReceiveData;
            efwplusBaseManager.ShowMsg= _actionReceiveData;
            efwplusRouteManager.ShowMsg= _actionReceiveData;
            efwplusWebAPIManager.ShowMsg= _actionReceiveData;
            Application.Run(host);
        }

        static string ExecCmd(string m, Dictionary<string,string> a)
        {
            try
            {
                switch (m)
                {
                    case "startall":
                        MongodbManager.StartDB();
                        NginxManager.StartWeb();

                        efwplusBaseManager.StartBase();
                        efwplusRouteManager.StartRoute();
                        efwplusWebAPIManager.StartAPI();
                        break;
                    case "quitall":
                        efwplusBaseManager.StopBase();
                        efwplusRouteManager.StopRoute();
                        efwplusWebAPIManager.StopAPI();
                        MongodbManager.StopDB();
                        NginxManager.StopWeb();
                        break;
                    case "exit":
                        efwplusBaseManager.StopBase();
                        efwplusRouteManager.StopRoute();
                        efwplusWebAPIManager.StopAPI();
                        MongodbManager.StopDB();
                        NginxManager.StopWeb();
                        Process.GetCurrentProcess().Kill();
                        break;
                    case "restart":
                        efwplusBaseManager.StopBase();
                        efwplusRouteManager.StopRoute();
                        efwplusWebAPIManager.StopAPI();
                        MongodbManager.StopDB();
                        NginxManager.StopWeb();

                        MongodbManager.StartDB();
                        NginxManager.StartWeb();

                        efwplusBaseManager.StartBase();
                        efwplusRouteManager.StartRoute();
                        efwplusWebAPIManager.StartAPI();
                        break;
                    case "restartbase":
                        efwplusBaseManager.StopBase();
                        efwplusBaseManager.StartBase();
                        break;
                    case "restartroute":
                        efwplusRouteManager.StopRoute();
                        efwplusRouteManager.StartRoute();
                        break;
                    case "restartwebapi":
                        efwplusWebAPIManager.StopAPI();
                        efwplusWebAPIManager.StartAPI();
                        break;
                    case "restartmongodb":
                        efwplusWebAPIManager.StopAPI();
                        efwplusWebAPIManager.StartAPI();
                        break;
                    case "restartnginx":
                        efwplusWebAPIManager.StopAPI();
                        efwplusWebAPIManager.StartAPI();
                        break;
                }

                return "succeed";
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }
    }
}
