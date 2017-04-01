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

            Updater();//升级
            StartListen();

            host = new FrmHosting(ExecCmd);

            Func<string, Dictionary<string,string>, string> _funcExecCmd = ExecCmd;
            Action<string> _actionReceiveData = ((string data) =>
            {
                string text = ("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] : " + data);
                host.showmsg(text);
            });
            serverIPC = new efwplusServerIPCManager(_funcExecCmd, _actionReceiveData);

            efwplusHttpManager.ShowMsg = _actionReceiveData;
            MongodbManager.ShowMsg = _actionReceiveData;
            NginxManager.ShowMsg = _actionReceiveData;
            efwplusBaseManager.ShowMsg= _actionReceiveData;
            efwplusRouteManager.ShowMsg= _actionReceiveData;
            efwplusWebAPIManager.ShowMsg= _actionReceiveData;
            Application.Run(host);
        }

        static System.Timers.Timer timer;
        //
        static void StartListen()
        {
            timer = new System.Timers.Timer();
            timer.Interval = 5000;//1s
            timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);
            timer.Start();
        }

        static void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                timer.Enabled = false;
                Updater();
                timer.Enabled = true;
            }
            catch
            {
                timer.Enabled = true;
            }
        }

        static void Updater()
        {
            var updater = FSLib.App.SimpleUpdater.Updater.Instance;
            //当检查发生错误时,这个事件会触发
            //updater.Error += new EventHandler(updater_Error);
            //没有找到更新的事件
            //updater.NoUpdatesFound += new EventHandler(updater_NoUpdatesFound);
            //找到更新的事件.但在此实例中,找到更新会自动进行处理,所以这里并不需要操作
            //updater.UpdatesFound += new EventHandler(updater_UpdatesFound);
            //开始检查更新-这是最简单的模式.请现在 assemblyInfo.cs 中配置更新地址,参见对应的文件.
            FSLib.App.SimpleUpdater.Updater.CheckUpdateSimple("http://localhost:8810/FileStore/MNodeUpgrade/update.xml");
        }

        static string ExecCmd(string m, Dictionary<string,string> a)
        {
            try
            {
                switch (m)
                {
                    case "startall":
                        efwplusHttpManager.StartHttp();
                        MongodbManager.StartDB();
                        NginxManager.StartWeb();

                        efwplusBaseManager.StartBase();
                        efwplusRouteManager.StartRoute();
                        efwplusWebAPIManager.StartAPI();
                        break;
                    case "quitall":
                        efwplusHttpManager.StopHttp();
                        efwplusBaseManager.StopBase();
                        efwplusRouteManager.StopRoute();
                        efwplusWebAPIManager.StopAPI();
                        MongodbManager.StopDB();
                        NginxManager.StopWeb();
                        break;
                    case "exit":
                        efwplusHttpManager.StopHttp();
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

                        //efwplusHttpManager.StartHttp();
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
