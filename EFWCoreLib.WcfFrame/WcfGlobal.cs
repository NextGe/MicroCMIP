using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using EFWCoreLib.CoreFrame.Common;
using EFWCoreLib.CoreFrame.Init;
using EFWCoreLib.CoreFrame.ProcessManage;
using EFWCoreLib.WcfFrame.DataSerialize;
using EFWCoreLib.WcfFrame.ServerManage;
using EFWCoreLib.WcfFrame.Utility;
using EFWCoreLib.WcfFrame.Utility.Mongodb;
using EFWCoreLib.WcfFrame.WcfHandler;

namespace EFWCoreLib.WcfFrame
{
    public class WcfGlobal
    {
        public static NormalIPCManager normalIPC;
        public static string ns = "http://www.efwplus.cn/";
        public static string pluginUpgradeFile = "pluginupgrade.txt";//基础服务启动的时候会根据此文件进行升级操作
        private static bool IsStartBase = false;//是否开启数据服务
        private static bool IsStartRoute = false;//是否开启路由服务 

        public static bool IsDebug = false;//调试模式
        public static string Identify = "";//中间件唯一标识
        public static string HostName = "";//中间件显示名称
        public static bool IsCompressJson = false;//是否压缩Json数据
        public static bool IsEncryptionJson = false;//是否加密Json数据
        public static SerializeType serializeType = SerializeType.Newtonsoft;//序列化方式
        public static bool IsToken = false;//身份验证
        public static string MongoConnStr = "";//mongo连接字符串
        public static bool IsOverTime = false;//开启超时记录
        public static int OverTime = 1;//超时记录日志

        public static bool IsRootMNode = false;//是否中间件根节点

        static ServiceHost mAppHost = null;
        static ServiceHost mFileHost = null;
        static ServiceHost mRouterHost = null;
        static ServiceHost mFileRouterHost = null;

        public static void MainBase(string _identify)
        {
            if (IsStartBase == true) return;
            IsStartBase = true;//设置为开启

            SetUpPluginUpgrade();

            IsRootMNode = HostSettingConfig.GetValue("rootmnode") == "1" ? true : false;
            IsDebug = HostSettingConfig.GetValue("debug") == "1" ? true : false;
            Identify = _identify;
            HostName = HostSettingConfig.GetValue("hostname");

            IsCompressJson = HostSettingConfig.GetValue("compress") == "1" ? true : false;
            IsEncryptionJson = HostSettingConfig.GetValue("encryption") == "1" ? true : false;
            serializeType = (SerializeType)Convert.ToInt32(HostSettingConfig.GetValue("serializetype"));
            IsOverTime = HostSettingConfig.GetValue("overtime") == "1" ? true : false;
            OverTime = Convert.ToInt32(HostSettingConfig.GetValue("overtimetime"));
            IsToken = HostSettingConfig.GetValue("token") == "1" ? true : false;
            MongoConnStr = HostSettingConfig.GetValue("mongodb_conn");




            WcfGlobal.Run(StartType.BaseService);
            WcfGlobal.Run(StartType.FileService);
            WcfGlobal.Run(StartType.SuperClient);
            WcfGlobal.Run(StartType.PublishService);//发布订阅
            WcfGlobal.Run(StartType.DistributedCache);//分布式缓存
            WcfGlobal.Run(StartType.Upgrade);//升级包
            WcfGlobal.Run(StartType.MNodeState);//节点状态
            WcfGlobal.Run(StartType.RemotePlugin);//远程插件
            WcfGlobal.Run(StartType.MiddlewareTask);//定时任务

            CoreFrame.SSO.SsoHelper.Start();//单点登录启动

            GetAllConfig();//获取所有配置
        }

        public static void ExitBase()
        {
            if (IsStartBase == false) return;
            IsStartBase = false;//设置为开启

            MiddlewareLogHelper.WriterLog(LogType.MidLog, true, Color.Red, "正在准备关闭中间件服务，请等待...");
            ClientLinkManage.UnAllConnection();//关闭所有连接

            WcfGlobal.Quit(StartType.PublishService);
            WcfGlobal.Quit(StartType.MiddlewareTask);
            WcfGlobal.Quit(StartType.SuperClient);
            WcfGlobal.Quit(StartType.BaseService);
            WcfGlobal.Quit(StartType.FileService);

            WcfGlobal.Quit(StartType.DistributedCache);
            WcfGlobal.Quit(StartType.Upgrade);
            //WcfGlobal.Quit(StartType.MongoDB);
            //WcfGlobal.Quit(StartType.Nginx);
        }

        public static void MainRoute()
        {
            if (IsStartRoute == true) return;
            IsStartRoute = true;//设置为开启

            WcfGlobal.Run(StartType.RouterBaseService);
            WcfGlobal.Run(StartType.RouterFileService);
        }

        public static void ExitRoute()
        {
            if (IsStartRoute == false) return;
            IsStartRoute = false;//设置为开启

            WcfGlobal.Quit(StartType.RouterBaseService);
            WcfGlobal.Quit(StartType.RouterFileService);
        }

        public static void Run(StartType type)
        {
            
            switch (type)
            {
                case StartType.BaseService:
                    mAppHost = new ServiceHost(typeof(BaseService));
                    //初始化连接池,默认10分钟清理连接
                    ClientLinkPoolCache.Init(true, 200, 30, 600, "wcfserver", 30);

                    AppGlobal.AppRootPath = System.Windows.Forms.Application.StartupPath + "\\";
                    AppGlobal.appType = AppType.WCF;
                    AppGlobal.IsSaas = System.Configuration.ConfigurationManager.AppSettings["IsSaas"] == "true" ? true : false;
                    AppGlobal.AppStart();


                    ClientManage.IsHeartbeat = HostSettingConfig.GetValue("heartbeat") == "1" ? true : false;
                    ClientManage.HeartbeatTime = Convert.ToInt32(HostSettingConfig.GetValue("heartbeattime"));

                    ClientManage.StartHost();
                    mAppHost.Open();

                    MiddlewareLogHelper.WriterLog(LogType.MidLog, true, Color.Blue, "数据服务启动完成");
                    break;

                case StartType.FileService:
                    AppGlobal.AppRootPath = System.Windows.Forms.Application.StartupPath + "\\";

                    mFileHost = new ServiceHost(typeof(FileService));
                    mFileHost.Open();

                    MiddlewareLogHelper.WriterLog(LogType.MidLog, true, Color.Blue, "文件服务启动完成");
                    break;
                case StartType.RouterBaseService:
                    mRouterHost = new ServiceHost(typeof(RouterBaseService));
                    RouterManage.Start();
                    mRouterHost.Open();

                    MiddlewareLogHelper.WriterLog(LogType.MidLog, true, Color.Blue, "路由数据服务启动完成");
                    break;
                case StartType.RouterFileService:
                    mFileRouterHost = new ServiceHost(typeof(RouterFileService));
                    mFileRouterHost.Open();

                    MiddlewareLogHelper.WriterLog(LogType.MidLog, true, Color.Blue, "路由文件服务启动完成");
                    break;
                case StartType.SuperClient:
                    SuperClient.Start();
                    MiddlewareLogHelper.WriterLog(LogType.MidLog, true, Color.Blue, "超级客户端启动完成");
                    break;
                case StartType.MiddlewareTask:
                    MiddlewareTask.StartTask();//开启定时任务
                    MiddlewareLogHelper.WriterLog(LogType.MidLog, true, Color.Blue, "定时任务启动完成");
                    break;
                case StartType.PublishService://订阅
                    PublisherManage.Start();
                    SubscriberManager.Start();
                    MiddlewareLogHelper.WriterLog(LogType.MidLog, true, Color.Blue, "发布订阅启动完成");
                    break;
                case StartType.MNodeState://中间件节点状态处理
                    MNodeStateManage.Start();
                    MiddlewareLogHelper.WriterLog(LogType.MidLog, true, Color.Blue, "节点状态监控启动完成");
                    break;
                case StartType.DistributedCache:
                    DistributedCacheManage.Start();
                    DistributedCacheClient.Start();
                    MiddlewareLogHelper.WriterLog(LogType.MidLog, true, Color.Blue, "分布式缓存启动完成");
                    break;
                case StartType.RemotePlugin:
                    RemotePluginManage.Start();
                    MiddlewareLogHelper.WriterLog(LogType.MidLog, true, Color.Blue, "远程插件启动完成");
                    break;

                case StartType.Upgrade:
                    UpgradeManage.Start();
                    UpgradeClient.Start();
                    MiddlewareLogHelper.WriterLog(LogType.MidLog, true, Color.Blue, "升级包管理启动完成");
                    break;
            }

        }

        public static void Quit(StartType type)
        {
           
            switch (type)
            {
                case StartType.BaseService:
                    try
                    {
                        if (mAppHost != null)
                        {
                            //EFWCoreLib.WcfFrame.ClientLinkPoolCache.Dispose();
                            ClientManage.StopHost();
                            mAppHost.Close();
                            MiddlewareLogHelper.WriterLog(LogType.MidLog, true, Color.Red, "数据服务已关闭！");
                        }
                    }
                    catch
                    {
                        if (mAppHost != null)
                            mAppHost.Abort();
                    }
                    break;

                case StartType.FileService:
                    try
                    {
                        if (mFileHost != null)
                        {
                            mFileHost.Close();
                            MiddlewareLogHelper.WriterLog(LogType.MidLog, true, Color.Red, "文件传输服务已关闭！");
                        }
                    }
                    catch
                    {
                        if (mFileHost != null)
                            mFileHost.Abort();
                    }
                    break;
                case StartType.RouterBaseService:
                    try
                    {
                        if (mRouterHost != null)
                        {
                            mRouterHost.Close();
                            MiddlewareLogHelper.WriterLog(LogType.MidLog, true, Color.Red, "数据路由服务已关闭！");
                        }
                    }
                    catch
                    {
                        if (mRouterHost != null)
                            mRouterHost.Abort();
                    }
                    break;
                case StartType.RouterFileService:
                    try
                    {
                        if (mFileRouterHost != null)
                        {
                            mFileRouterHost.Close();
                            MiddlewareLogHelper.WriterLog(LogType.MidLog, true, Color.Red, "文件路由服务已关闭！");
                        }
                    }
                    catch
                    {
                        if (mFileRouterHost != null)
                            mFileRouterHost.Abort();
                    }
                    break;
                case StartType.SuperClient:
                    SuperClient.Stop();
                    MiddlewareLogHelper.WriterLog(LogType.TimingTaskLog, true, System.Drawing.Color.Red, "超级客户端已关闭！");
                    break;
                case StartType.MiddlewareTask:
                    MiddlewareTask.StopTask();//停止任务
                    MiddlewareLogHelper.WriterLog(LogType.TimingTaskLog, true, System.Drawing.Color.Red, "定时任务已停止！");
                    break;
                case StartType.PublishService://订阅
                    MiddlewareLogHelper.WriterLog(LogType.MidLog, true, Color.Red, "订阅服务已停止");
                    break;
                case StartType.Upgrade://升级包
                    UpgradeManage.Stop();
                    UpgradeClient.Stop();
                    MiddlewareLogHelper.WriterLog(LogType.MidLog, true, Color.Red, "升级包服务已停止");
                    break;
            }
        }

        private static void GetAllConfig()
        {
            #region 收集配置信息
            Action<HostRunConfigSubject> psAction = ((HostRunConfigSubject subject) =>
            {
                MNodePlugin nodeP = RemotePluginManage.GetLocalPlugin();
                if (nodeP != null)
                {
                    HostRunConfigObject configObj;
                    foreach (var p in nodeP.RemotePlugin)
                    {
                        configObj = new HostRunConfigObject();
                        configObj.Label = "远程插件";
                        configObj.Key = p.PluginName;
                        configObj.Value = p.PluginName;
                        configObj.Memo = p.PluginName + "\t" + String.Join(",", p.MNodeIdentify.ToArray());
                        subject.ConfigObjList.Add(configObj);
                    }
                }
            });
            Action<HostRunConfigSubject> pubsAction = ((HostRunConfigSubject subject) =>
            {
                if (PublisherManage.GetPublishServiceList() != null)
                {
                    HostRunConfigObject configObj;
                    foreach (var p in PublisherManage.GetPublishServiceList())
                    {
                        configObj = new HostRunConfigObject();
                        configObj.Label = "发布服务";
                        configObj.Key = p.publishServiceName;
                        configObj.Value = p.publishServiceName;
                        configObj.Memo = p.publishServiceName + "\t" + p.explain;
                        subject.ConfigObjList.Add(configObj);
                    }
                }
            });
            Action<HostRunConfigSubject> subsAction = ((HostRunConfigSubject subject) =>
            {
                if (SubscriberManager.GetSubscribeService() != null)
                {
                    HostRunConfigObject configObj;
                    foreach (var p in SubscriberManager.GetSubscribeService())
                    {
                        configObj = new HostRunConfigObject();
                        configObj.Label = "订阅服务";
                        configObj.Key = p.publishServiceName;
                        configObj.Value = p.publishServiceName;
                        configObj.Memo = p.publishServiceName;
                        subject.ConfigObjList.Add(configObj);
                    }
                }
            });
            Action<HostRunConfigSubject> taskAction = ((HostRunConfigSubject subject) =>
            {
                if (MiddlewareTask.TaskConfigList != null)
                {
                    HostRunConfigObject configObj;
                    foreach (var p in MiddlewareTask.TaskConfigList)
                    {
                        configObj = new HostRunConfigObject();
                        configObj.Label = p.taskname;
                        configObj.Key = p.taskname;
                        configObj.Value = p.taskname;
                        configObj.Memo = (p.qswitch ? "已开启" : "未开启") + "\t" + p.execfrequencyName + "\t" + p.shorttimeName + "\t" + p.serialorparallelName;
                        subject.ConfigObjList.Add(configObj);
                    }
                }
            });
            HostRunConfigInfo.LoadConfigInfo(Identify, psAction, pubsAction, subsAction, taskAction);
            #endregion
        }
        //安装插件升级包
        private static void SetUpPluginUpgrade()
        {
            string pufile = AppGlobal.AppRootPath + WcfGlobal.pluginUpgradeFile;
            if (File.Exists(pufile) == true)
            {
                List<string> addplugin = new List<string>();//新增插件
                List<string> updateplugin = new List<string>();//更新插件
                List<string> deleteplugin = new List<string>();//删除插件

                using (StreamReader sr = new StreamReader(pufile))
                {
                    string addrow = sr.ReadLine();
                    addplugin = addrow.Split(':')[1].Split(',').ToList();

                    string updaterow = sr.ReadLine();
                    updateplugin = updaterow.Split(':')[1].Split(',').ToList();

                    string deleterow = sr.ReadLine();
                    deleteplugin = deleterow.Split(':')[1].Split(',').ToList();
                }

                //删除
                File.Delete(pufile);

                foreach (string p in addplugin)
                {
                    if (p.Trim() != "")
                    {
                        string path = AppGlobal.AppRootPath + "ModulePlugin\\" + p;
                        //删除本地插件
                        if (Directory.Exists(path))
                        {
                            Directory.Delete(path, true);
                        }
                        //解压插件包
                        string zipfile = AppGlobal.AppRootPath + @"FileStore\PluginUpgrade\" + p + ".zip";
                        FastZipHelper.decompress(AppGlobal.AppRootPath + "ModulePlugin\\" + p, zipfile);
                        //修改pluginsys.xml配置文件
                        AppPluginManage.AddPlugin("ModulePlugin\\" + p + "\\plugin.xml");
                    }
                }

                foreach (string p in updateplugin)
                {
                    if (p.Trim() != "")
                    {
                        string path = AppGlobal.AppRootPath + "ModulePlugin\\" + p;
                        //删除本地插件
                        if (Directory.Exists(path))
                        {
                            Directory.Delete(path, true);
                        }
                        //解压插件包
                        string zipfile = AppGlobal.AppRootPath + @"FileStore\PluginUpgrade\" + p + ".zip";
                        FastZipHelper.decompress(AppGlobal.AppRootPath + "ModulePlugin\\" + p, zipfile);
                        //修改pluginsys.xml配置文件
                        AppPluginManage.RemovePlugin(p);
                        AppPluginManage.AddPlugin("ModulePlugin\\" + p + "\\plugin.xml");
                    }
                }

                foreach (string p in deleteplugin)
                {
                    if (p.Trim() != "")
                    {
                        string path = AppGlobal.AppRootPath + "ModulePlugin\\" + p;
                        //删除本地插件
                        if (Directory.Exists(path))
                        {
                            Directory.Delete(path, true);
                        }
                        //修改pluginsys.xml配置文件
                        AppPluginManage.RemovePlugin(p);
                    }
                }

                
            }
        }
    }

    public enum StartType
    {
        BaseService,
        FileService,
        MiddlewareTask,
        SuperClient,
        PublishService,
        DistributedCache,
        Upgrade,
        MNodeState,
        RemotePlugin,
        RouterBaseService,
        RouterFileService
    }
}
