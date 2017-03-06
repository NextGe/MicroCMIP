using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFWCoreLib.CoreFrame.ProcessManage
{
    public class efwplusBaseManager
    {
        /// <summary>
        /// 开启efwplusBase
        /// </summary>
        public static void StartBase()
        {
            string baseExe = AppDomain.CurrentDomain.BaseDirectory + @"\efwplusBase.exe";

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
        }
        /// <summary>
        /// 停止efwplusBase
        /// </summary>
        public static void StopBase()
        {
            Process[] proc = Process.GetProcessesByName("efwplusBase");//创建一个进程数组，把与此进程相关的资源关联。
            for (int i = 0; i < proc.Length; i++)
            {
                proc[i].Kill();  //逐个结束进程.
            }
        }
    }
}
