using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EFWCoreLib.CoreFrame.Init;

namespace EFWCoreLib.CoreFrame.ProcessManage
{
    public class efwplusRouteManager
    {
        public static Action<string> ShowMsg;
        public static bool Isrouter = false;
        public static string routeExe = "";
        /// <summary>
        /// 开启efwplusRoute
        /// </summary>
        public static Process StartRoute()
        {
            Isrouter = HostSettingConfig.GetValue("router") == "1" ? true : false;
            if (Isrouter)
            {
                routeExe = AppDomain.CurrentDomain.BaseDirectory + @"efwplusRoute.exe";

                System.Diagnostics.Process pro = new System.Diagnostics.Process();
                pro.StartInfo.FileName = routeExe;
                pro.StartInfo.UseShellExecute = false;
                //pro.StartInfo.RedirectStandardInput = true;
                //pro.StartInfo.RedirectStandardOutput = true;
                //pro.StartInfo.RedirectStandardError = true;
                pro.StartInfo.CreateNoWindow = true;
                pro.Start();
                //pro.WaitForExit();

                ShowMsg("路由程序已启动");

                return pro;
            }

            return null;
        }
        /// <summary>
        /// 停止efwplusBase
        /// </summary>
        public static void StopRoute()
        {
            Isrouter = HostSettingConfig.GetValue("router") == "1" ? true : false;
            if (Isrouter == false) return;

            Process[] proc = Process.GetProcessesByName("efwplusRoute");//创建一个进程数组，把与此进程相关的资源关联。
            for (int i = 0; i < proc.Length; i++)
            {
                proc[i].Kill();  //逐个结束进程.
            }
        }
    }
}
