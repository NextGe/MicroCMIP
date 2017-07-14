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
            try
            {
                MyHttpServer.ShowMsg = ShowMsg;
                MyHttpServer.Listen();
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message+err.StackTrace);
            }
            finally
            {
                while (true)
                {
                    System.Threading.Thread.Sleep(30 * 1000);
                }
            }
        }

        static void ShowMsg(string msg)
        {
            string text = ("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] : " + msg);
            Console.WriteLine(text);
        }
    }
}
