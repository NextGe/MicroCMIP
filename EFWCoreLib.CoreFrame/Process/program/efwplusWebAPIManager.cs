using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EFWCoreLib.CoreFrame.Init;

namespace EFWCoreLib.CoreFrame.ProcessManage
{
    public class efwplusWebAPIManager
    {
        public static Action<string> ShowMsg;
        private static bool Iswebapi = false;
        /// <summary>
        /// 开启efwplusWebAPI
        /// </summary>
        public static void StartAPI()
        {
            //Iswebapi = HostSettingConfig.GetValue("webapi") == "1" ? true : false;
            //if (Iswebapi)
            //{
            //    string apiExe = AppDomain.CurrentDomain.BaseDirectory + @"\efwplusWebAPI.exe";

            //    System.Diagnostics.Process pro = new System.Diagnostics.Process();
            //    pro.StartInfo.FileName = apiExe;
            //    pro.StartInfo.UseShellExecute = false;
            //    //pro.StartInfo.RedirectStandardInput = true;
            //    //pro.StartInfo.RedirectStandardOutput = true;
            //    //pro.StartInfo.RedirectStandardError = true;
            //    pro.StartInfo.CreateNoWindow = true;
            //    pro.Start();
            //    //pro.WaitForExit();

            //    ShowMsg("WebApi已启动");
            //}
        }
        /// <summary>
        /// 停止efwplusBase
        /// </summary>
        public static void StopAPI()
        {
            //Iswebapi = HostSettingConfig.GetValue("webapi") == "1" ? true : false;
            //if (Iswebapi == false) return;

            //Process[] proc = Process.GetProcessesByName("efwplusWebAPI");//创建一个进程数组，把与此进程相关的资源关联。
            //for (int i = 0; i < proc.Length; i++)
            //{
            //    proc[i].Kill();  //逐个结束进程.
            //}
        }
    }
}
