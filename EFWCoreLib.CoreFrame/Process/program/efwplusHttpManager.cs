using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EFWCoreLib.CoreFrame.Init;

namespace EFWCoreLib.CoreFrame.ProcessManage
{
    public class efwplusHttpManager
    {
        public static Action<string> ShowMsg;
        public static bool isHttp = true;
        public static string baseExe = "";
        //private static bool Iswcfservice = false;
        /// <summary>
        /// 开启efwplusHttp
        /// </summary>
        public static Process StartHttp()
        {
            baseExe = AppDomain.CurrentDomain.BaseDirectory + @"efwplusHttp.exe";

            System.Diagnostics.Process pro = new System.Diagnostics.Process();
            pro.StartInfo.FileName = baseExe;
            pro.StartInfo.UseShellExecute = false;
            //pro.StartInfo.RedirectStandardInput = true;
            //pro.StartInfo.RedirectStandardOutput = true;
            //pro.StartInfo.RedirectStandardError = true;
            pro.StartInfo.CreateNoWindow = true;
            pro.Start();
            //pro.WaitForExit();
            //pro.StandardInput.AutoFlush = true;

            ShowMsg("Http程序已启动");

            return pro;
        }
        /// <summary>
        /// 停止efwplusHttp
        /// </summary>
        public static void StopHttp()
        {

            Process[] proc = Process.GetProcessesByName("efwplusHttp");//创建一个进程数组，把与此进程相关的资源关联。
            for (int i = 0; i < proc.Length; i++)
            {
                proc[i].Kill();  //逐个结束进程.
            }
        }
    }
}
