using System;
using System.Collections.Generic;
using System.Web;
using System.IO;
using System.Linq;
using Microsoft.Practices.Unity;
using System.Collections;
using EFWCoreLib.CoreFrame.Common;
using Microsoft.Practices.EnterpriseLibrary.Caching;
using EFWCoreLib.CoreFrame.EntLib;
using System.Configuration;
using Microsoft.Practices.Unity.Configuration;
using EFWCoreLib.CoreFrame.DbProvider;
using EFWCoreLib.CoreFrame.Init.AttributeManager;
using EFWCoreLib.CoreFrame.Plugin;
using System.Windows.Forms;
using System.Reflection;


namespace EFWCoreLib.CoreFrame.Init
{
    /// <summary>
    /// 系统启动前初始化环境
    /// </summary>
    public class AppGlobal
    {
        /// <summary>
        /// 应用程序根目录，后面不需要跟\\
        /// </summary>
        public static string AppRootPath;
        /// <summary>
        /// 程序类型
        /// </summary>
        public static AppType appType;
        /// <summary>
        /// 是否Saas模式，where条件是否加workid
        /// </summary>
        public static bool IsSaas = false;


        /// <summary>
        /// 默认Unity对象容器
        /// </summary>
        public static IUnityContainer container;
        /// <summary>
        /// 默认企业库缓存
        /// </summary>
        public static ICacheManager cache;

        /// <summary>
        /// 默认数据库对象
        /// </summary>
        public static AbstractDatabase database;

        /// <summary>
        /// 定制任务
        /// </summary>
        //public static List<TimingTask> taskList;

        /// <summary>
        /// 委托代码
        /// </summary>
        //public static List<FunClass> codeList;

        /// <summary>
        /// 缺失的程序集dll
        /// </summary>
        //public static List<string> missingDll;

        private static bool _isCalled = false;

        private static object locker = new object();

        public static void AppStart()
        {
            lock (locker)
            {
                if (_isCalled == false)
                {
                    try
                    {
                        WriterLog("--------------------------------");
                        WriterLog("应用开始启动！");

                        container = ZhyContainer.CreateUnity();
                        cache = ZhyContainer.CreateCache();
                        database = FactoryDatabase.GetDatabase();
                        //taskList = new List<TimingTask>();
                        //codeList = new List<FunClass>();
                         
                        //加载插件
                        AppPluginManage.LoadAllPlugin();
                         
                        _isCalled = true;
                      
                        WriterLog("应用启动成功！");
                        WriterLog("--------------------------------");
                        //AppMain();
                    }
                    catch(Exception err)
                    {
                        AppGlobal.WriterLog("应用启动失败！");
                        AppGlobal.WriterLog(err.Message);
                        AppGlobal.WriterLog("--------------------------------");
                        throw err;
                    }
                }
            }
        }

       
        public static void AppEnd()
        {
            //GlobalExtend.EndInit();
            //EFWCoreLib.WcfFrame.ClientLinkManage.UnAllConnection();
        }

        public static void WriterLog(string info)
        {
            info = "时间：" + DateTime.Now.ToString() + "\t\t" + "内容：" + info + "\r\n";
            File.AppendAllText(AppRootPath + "startlog.txt", info);
        }
    }

    public enum AppType
    {
        Web,Winform,WCF
    }
}
