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

            setprivatepath();
            //Updater();//升级
            //StartListen();

            host = new FrmHosting(ExecCmd);

            Func<string, Dictionary<string, string>, string> _funcExecCmd = ExecCmd;
            Action<string> _actionReceiveData = showmsg;
            serverIPC = new efwplusServerIPCManager(_funcExecCmd, _actionReceiveData);

            efwplusHttpManager.ShowMsg = _actionReceiveData;
            MongodbManager.ShowMsg = _actionReceiveData;
            NginxManager.ShowMsg = _actionReceiveData;
            efwplusBaseManager.ShowMsg = _actionReceiveData;
            efwplusRouteManager.ShowMsg = _actionReceiveData;
            efwplusWebAPIManager.ShowMsg = _actionReceiveData;
            Application.Run(host);
        }

        static void showmsg(string data)
        {
            string text = ("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] : " + data);
            host.showmsg(text);
        }

        static void setprivatepath()
        {
            //AppDomain.CurrentDomain.SetupInformation.PrivateBinPath = @"Component;ModulePlugin\Books_Wcf\dll;ModulePlugin\WcfMainUIFrame\dll";
            string privatepath = @"Component";

            AppDomain.CurrentDomain.SetData("PRIVATE_BINPATH", privatepath);
            AppDomain.CurrentDomain.SetData("BINPATH_PROBE_ONLY", privatepath);
            var m = typeof(AppDomainSetup).GetMethod("UpdateContextProperty", BindingFlags.NonPublic | BindingFlags.Static);
            var funsion = typeof(AppDomain).GetMethod("GetFusionContext", BindingFlags.NonPublic | BindingFlags.Instance);
            m.Invoke(null, new object[] { funsion.Invoke(AppDomain.CurrentDomain, null), "PRIVATE_BINPATH", privatepath });

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
            //"http://localhost:8810/FileStore/MNodeUpgrade/update.xml"
            FSLib.App.SimpleUpdater.Updater.CheckUpdateSimple(EFWCoreLib.CoreFrame.Init.HostSettingConfig.GetValue("updaterurl"));
        }

        static string ExecCmd(string m, Dictionary<string, string> a)
        {
            try
            {
                //ProcessWatcher.OnStop();
                switch (m)
                {
                    case "startall":
                        ProcessWatcher.OnStop();

                        efwplusHttpManager.StartHttp();
                        MongodbManager.StartDB();
                        NginxManager.StartWeb();

                        efwplusBaseManager.StartBase();
                        efwplusRouteManager.StartRoute();
                        efwplusWebAPIManager.StartAPI();

                        ProcessWatcher.OnStart();
                        break;
                    case "quitall":
                        ProcessWatcher.OnStop();

                        efwplusHttpManager.StopHttp();
                        efwplusBaseManager.StopBase();
                        efwplusRouteManager.StopRoute();
                        efwplusWebAPIManager.StopAPI();
                        MongodbManager.StopDB();
                        NginxManager.StopWeb();
                        break;
                    case "exit":
                        ProcessWatcher.OnStop();

                        efwplusHttpManager.StopHttp();
                        efwplusBaseManager.StopBase();
                        efwplusRouteManager.StopRoute();
                        efwplusWebAPIManager.StopAPI();
                        MongodbManager.StopDB();
                        NginxManager.StopWeb();
                        Process.GetCurrentProcess().Kill();
                        break;
                    case "restart":
                        ProcessWatcher.OnStop();

                        efwplusBaseManager.StopBase();
                        efwplusRouteManager.StopRoute();
                        efwplusWebAPIManager.StopAPI();
                        MongodbManager.StopDB();
                        NginxManager.StopWeb();

                        Application.Restart();
                        Process.GetCurrentProcess().Kill();
                        //MongodbManager.StartDB();
                        //NginxManager.StartWeb();

                        //efwplusBaseManager.StartBase();
                        //efwplusRouteManager.StartRoute();
                        //efwplusWebAPIManager.StartAPI();

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
                        MongodbManager.StopDB();
                        MongodbManager.StartDB();
                        break;
                    case "restartnginx":
                        NginxManager.StopWeb();
                        NginxManager.StartWeb();
                        break;
                    case "upgradeplugin"://升级插件
                        if (EFWCoreLib.CoreFrame.Init.HostSettingConfig.GetValue("autoupdater") == "1")//是否启动自动升级程序
                        {
                            showmsg("准备升级插件...");
                            ExecCmd("quitall", null);
                            try
                            {
                                efwplusHosting.UpgradeProgram.SetUpPluginUpgrade();
                            }
                            catch (Exception err)
                            {
                                showmsg("升级插件失败！" + err.Message+err.StackTrace);
                                showmsg("程序服务未启动.");
                                //Process.GetCurrentProcess().Kill();
                                host.RunState = HostState.NoOpen;
                            }

                            showmsg("升级插件完成,正在启动服务...");
                            ExecCmd("startall", null);
                        }
                        else
                        {
                            showmsg("自动升级插件没有开启!");
                        }
                        break;
                }
                //ProcessWatcher.OnStart();
                return "succeed";
            }
            catch (Exception e)
            {
                showmsg(e.Message + e.StackTrace);
                return e.Message;
            }
        }
    }
}
