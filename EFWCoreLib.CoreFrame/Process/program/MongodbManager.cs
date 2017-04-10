using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using EFWCoreLib.CoreFrame.Init;

namespace EFWCoreLib.CoreFrame.ProcessManage
{
    /// <summary>
    /// Mongodb处理
    /// </summary>
    public class MongodbManager
    {
        public static Action<string> ShowMsg;
        public static bool Ismongodb = false;
        public static string mongodExe = "";
        /// <summary>
        /// 开启Mongodb
        /// </summary>
        public static Process StartDB()
        {
            Ismongodb = HostSettingConfig.GetValue("mongodb") == "1" ? true : false;
            if (Ismongodb)
            {
                string config = String.Format(HostMongoDBConfig.GetConfig_Temp(), (HostSettingConfig.GetValue("mongodb_dbpath") == "" ? AppDomain.CurrentDomain.BaseDirectory : HostSettingConfig.GetValue("mongodb_dbpath")));
                HostMongoDBConfig.SetConfig(config);

                mongodExe = HostSettingConfig.GetValue("mongodb_binpath") + @"\mongod.exe";
                string mongoConf = AppDomain.CurrentDomain.BaseDirectory + @"Config\mongo.conf";

                System.Diagnostics.Process pro = new System.Diagnostics.Process();
                pro.StartInfo.FileName = mongodExe;
                pro.StartInfo.Arguments = "--config " + mongoConf;
                pro.StartInfo.UseShellExecute = false;
                //pro.StartInfo.RedirectStandardInput = true;
                //pro.StartInfo.RedirectStandardOutput = true;
                //pro.StartInfo.RedirectStandardError = true;
                pro.StartInfo.CreateNoWindow = true;
                pro.Start();
                //pro.WaitForExit();

                ShowMsg("Mongodb已启动");

                return pro;
            }

            return null;
        }
        /// <summary>
        /// 停止Mongodb
        /// </summary>
        public static void StopDB()
        {
            Ismongodb = HostSettingConfig.GetValue("mongodb") == "1" ? true : false;
            if (Ismongodb == false) return;

            Process[] proc = Process.GetProcessesByName("mongod");//创建一个进程数组，把与此进程相关的资源关联。
            for (int i = 0; i < proc.Length; i++)
            {
                proc[i].Kill();  //逐个结束进程.
            }
        }
    }
}
