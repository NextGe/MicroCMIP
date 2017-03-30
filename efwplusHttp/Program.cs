using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace efwplusHttp
{
    class Program
    {
        static void Main(string[] args)
        {
            MyHttpServer.ShowMsg = ShowMsg;
            MyHttpServer.Listen();
        }

        static void ShowMsg(string msg)
        {
            string text = ("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] : " + msg);
            Console.WriteLine(text);
        }
    }
}
