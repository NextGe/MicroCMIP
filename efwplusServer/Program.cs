using System;
using System.Collections.Generic;
using EFWCoreLib.CoreFrame.ProcessManage;

namespace efwplusServer
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        static void Main(string[] args)
        {
            try
            {
                Func<string, Dictionary<string, string>, string> _funcExecCmd = ExecCmd;
                Action<string> _actionReceiveData = ((string data) =>
                {
                    string text = ("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] : " + data);
                    Console.WriteLine(text);
                });
                new efwplusServerIPCManager(_funcExecCmd, _actionReceiveData);

                efwplusHttpManager.ShowMsg = _actionReceiveData;
                MongodbManager.ShowMsg = _actionReceiveData;
                NginxManager.ShowMsg = _actionReceiveData;
                efwplusBaseManager.ShowMsg = _actionReceiveData;
                efwplusRouteManager.ShowMsg = _actionReceiveData;
                efwplusWebAPIManager.ShowMsg = _actionReceiveData;

                ExecCmd("quitall", null);
                ExecCmd("startall", null);
                while (true)
                {
                    System.Threading.Thread.Sleep(30 * 1000);
                }
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
            }
        }

        static string ExecCmd(string m, Dictionary<string, string> a)
        {
            try
            {
                switch (m)
                {
                    case "startall":
                        efwplusHttpManager.StartHttp();
                        MongodbManager.StartDB();
                        NginxManager.StartWeb();
                        //先启动mongodb，因为efwplusBaseManager启动时有访问mongodb数据
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
                        Environment.Exit(0);
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
