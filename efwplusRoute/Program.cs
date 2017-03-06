using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using EFWCoreLib.CoreFrame.Common;
using EFWCoreLib.WcfFrame;

namespace efwplusRoute
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                MiddlewareLogHelper.hostwcfMsg = new MiddlewareMsgHandler(ShowMsg);
                MiddlewareLogHelper.StartWriteFileLog();//开放中间件日志

                WcfGlobal.MainRoute();
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
            WcfGlobal.ExitRoute();
        }

        static void ShowMsg(Color clr, DateTime time, string msg)
        {
            Console.WriteLine("[" + time.ToString("yyyy-MM-dd HH:mm:ss") + "] : " + msg);
        }
    }
}
