using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EFWCoreLib.CoreFrame.Common;
using EFWCoreLib.CoreFrame.ProcessManage;
using EFWCoreLib.WebApiFrame;

namespace efwplusWebAPI
{
     static class Program
    {
        public static NormalIPCManager normalIPC;
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                setprivatepath();

                Func<string, Dictionary<string, string>, string> _funcExecCmd = ExecuteCmd;
                Action<string> _actionReceiveData = ((string data) =>
                {
                    string text = ("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] : " + data);
                    Console.WriteLine(data);
                });
                normalIPC = new NormalIPCManager(IPCType.efwplusWebAPI, _funcExecCmd, _actionReceiveData);


                MiddlewareLogHelper.hostwcfMsg = new MiddlewareMsgHandler(ShowMsg);
                MiddlewareLogHelper.StartWriteFileLog();//开放中间件日志

                WebApiGlobal.normalIPC = normalIPC;
                WebApiGlobal.Main();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }


            while (true)
            {
                System.Threading.Thread.Sleep(30 * 1000);
            }
        }

        static void Exit()
        {
            WebApiGlobal.Exit();
        }

        static void ShowMsg(Color clr, DateTime time, string msg)
        {
            normalIPC.ShowMsg(msg);
        }

        /// <summary>
        /// 获取或设置应用程序基目录下的目录列表
        /// </summary>
        static void setprivatepath()
        {
            //AppDomain.CurrentDomain.SetupInformation.PrivateBinPath = @"Component;ModulePlugin\Books_Wcf\dll;ModulePlugin\WcfMainUIFrame\dll";
            string privatepath = @"Component";

            foreach (var p in efwplus.configuration.PluginSysManage.GetAllPlugin())
            {
                privatepath += ";" + p.path.Replace("plugin.xml", "dll");
            }

            AppDomain.CurrentDomain.SetData("PRIVATE_BINPATH", privatepath);
            AppDomain.CurrentDomain.SetData("BINPATH_PROBE_ONLY", privatepath);
            var m = typeof(AppDomainSetup).GetMethod("UpdateContextProperty", BindingFlags.NonPublic | BindingFlags.Static);
            var funsion = typeof(AppDomain).GetMethod("GetFusionContext", BindingFlags.NonPublic | BindingFlags.Instance);
            m.Invoke(null, new object[] { funsion.Invoke(AppDomain.CurrentDomain, null), "PRIVATE_BINPATH", privatepath });

        }

        static string ExecuteCmd(string m, Dictionary<string, string> a)
        {
            string retData = "succeed";
            try
            {
                
                switch (m)
                {
                    case "stop":
                        Exit();
                        break;
                    case "start":
                        WebApiGlobal.Main();
                        break;
                    case "close":
                        Environment.Exit(0);
                        break;
                }
                ShowMsg(Color.Black, DateTime.Now, "efwplusWebApi命令执行完成：" + m);
                return retData;
            }
            catch (Exception e)
            {
                retData = e.Message;
                return retData;
            }
        }
    }
}
