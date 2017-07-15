using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using EFWCoreLib.CoreFrame.Common;
using EFWCoreLib.CoreFrame.Init;
using EFWCoreLib.CoreFrame.ProcessManage;
using EFWCoreLib.WcfFrame.DataSerialize;

namespace EFWCoreLib.WcfFrame.ServerManage
{
    /// <summary>
    /// 升级管理程序
    /// </summary>
    public class UpgradeManage
    {
        public static Action UpgradeVersionChangeEvent;//升级包版本改变触发事件

        /// <summary>
        /// 开始升级包程序
        /// </summary>
        public static void Start()
        {
            string publishServiceName = "UpgradeManage";
            //将升级包管理作为服务添加到发布订阅服务列表
            PublishServiceObject pso = new PublishServiceObject();
            pso.publishServiceName = publishServiceName;
            pso.explain = "升级包程序";
            PublisherManage.AddPublishService(pso);
            //触发通知
            UpgradeManage.UpgradeVersionChangeEvent = (() =>
            {
                PublisherManage.SendNotify(publishServiceName);//订阅服务发送通知
            });
        }
        /// <summary>
        /// 停止升级包程序
        /// </summary>
        public static void Stop()
        {

        }
        /// <summary>
        /// 更新升级包
        /// </summary>
        public static void UpdateUpgrade()
        {
            if (UpgradeVersionChangeEvent != null)
            {
                UpgradeVersionChangeEvent();
            }
        }
    }

    /// <summary>
    /// 升级包程序客户端
    /// </summary>
    public class UpgradeClient
    {
        private static string rootpath;
        private static string clientupgrade = @"ClientUpgrade\";//客户端升级包路径
        private static string webupgrade = @"WebUpgrade\";
        private static string mnodeupgrade = @"MNodeUpgrade\";
        private static string pluginupgrade = @"PluginUpgrade\";

        private static string publishServiceName = "UpgradeManage";//订阅服务名

        /// <summary>
        /// 分布式缓存客户端开启
        /// </summary>
        public static void Start()
        {
            rootpath = AppGlobal.AppRootPath + "FileStore\\";
            //创建客户端升级包目录
            if (!Directory.Exists(rootpath + clientupgrade))
            {
                Directory.CreateDirectory(rootpath + clientupgrade);
            }
            //创建Web程序升级包目录
            if (!Directory.Exists(rootpath + webupgrade))
            {
                Directory.CreateDirectory(rootpath + webupgrade);
            }
            //创建中间件程序升级包目录
            if (!Directory.Exists(rootpath + mnodeupgrade))
            {
                Directory.CreateDirectory(rootpath + mnodeupgrade);
            }
            //创建插件服务程序升级包目录
            if (!Directory.Exists(rootpath + pluginupgrade))
            {
                Directory.CreateDirectory(rootpath + pluginupgrade);
            }

            //客户端订阅分布式缓存
            SubscribeServiceObject ssObject = new SubscribeServiceObject();
            ssObject.publishServiceName = publishServiceName;
            ssObject.ProcessService = ((ClientLink _clientLink) =>
            {

                //DownLoadUpgrade(clientupgrade + "update.xml", clientupgrade + "update.zip", _clientLink);
                //DownLoadUpgrade(webupgrade + "update.xml", webupgrade + "update.zip", _clientLink);
                //DownLoadUpgrade(mnodeupgrade + "update.xml", mnodeupgrade + "update.zip", _clientLink);
                //下载插件
                //DownLoadPlugin(_clientLink);

                //UpgradeManage.UpdateUpgrade();//通知下级中间件升级
            });
            SubscriberManager.Subscribe(ssObject);
        }

        public static void Stop()
        {
            //客户端取消订阅分布式缓存
            SubscriberManager.UnSubscribe(publishServiceName);
        }
        //下载插件包
        public static void DownLoadPlugin(ClientLink _clientLink)
        {
            try
            {
                if (EFWCoreLib.CoreFrame.Init.HostSettingConfig.GetValue("autoupdater") == "1")//是否启动自动升级程序
                {
                    //1.节点新增了本地插件
                    //2.节点本地插件更新了版本
                    //3.节点本地插件卸载
                    MNodePlugin mnplugin = RemotePluginManage.GetLocalPlugin();
                    if (mnplugin == null || mnplugin.LocalPinfoList == null)
                    {
                        return;
                    }
                    List<string> addplugin = new List<string>();//新增插件
                    List<string> updateplugin = new List<string>();//更新插件
                    List<string> deleteplugin = new List<string>();//删除插件

                    //本地插件新增
                    foreach (string p in mnplugin.LocalPlugin)
                    {
                        if (AppPluginManage.PluginDic.Keys.ToList().FindIndex(x => x == p) == -1)
                        {
                            //新增插件
                            addplugin.Add(p);
                        }
                    }
                    //本地插件更新
                    foreach (string p in mnplugin.LocalPlugin)
                    {
                        if (AppPluginManage.PluginDic.ContainsKey(p))
                        {
                            //版本比较，是对比中心与本地插件的版本号
                            Version local = new Version(AppPluginManage.PluginDic[p].plugin.version);
                            Version remote = new Version(mnplugin.LocalPinfoList.Find(x => x.pluginname == p).versions);
                            int tm = local.CompareTo(remote);
                            if (tm < 0)//本地版本小
                            {
                                updateplugin.Add(p);
                                //downloadRemotePlugin(pluginupgrade + p + ".zip", _clientLink);
                                //下载之后触发安装，重启基础服务
                            }
                        }
                    }
                    //本地插件卸载
                    foreach (var p in AppPluginManage.PluginDic)
                    {
                        if (mnplugin.LocalPlugin.FindIndex(x => x == p.Key) == -1)
                        {
                            //已移除插件
                            deleteplugin.Add(p.Key);
                        }
                    }

                    //下载插件包
                    foreach (string p in addplugin)
                    {
                        downloadRemotePlugin(pluginupgrade + p + ".zip", _clientLink);
                    }

                    foreach (string p in updateplugin)
                    {
                        downloadRemotePlugin(pluginupgrade + p + ".zip", _clientLink);
                    }

                    //重启服务
                    if (addplugin.Count > 0 || updateplugin.Count > 0 || deleteplugin.Count > 0)
                    {
                        using (StreamWriter sw = new StreamWriter(AppGlobal.AppRootPath + WcfGlobal.pluginUpgradeFile, false))
                        {
                            sw.WriteLine("addplugin:" + string.Join(",", addplugin));
                            sw.WriteLine("updateplugin:" + string.Join(",", updateplugin));
                            sw.WriteLine("deleteplugin:" + string.Join(",", deleteplugin));
                        }
                        WcfGlobal.normalIPC.CallCmd(IPCName.GetProcessName(IPCType.efwplusServer), "upgradeplugin", null);
                    }
                }
            }
            catch (Exception err)
            {
                CoreFrame.Common.MiddlewareLogHelper.WriterLog(err.Message + err.StackTrace);
            }
        }
        //下载升级包
        public static void DownLoadUpgrade(string updatexml, string updatezip, ClientLink _clientLink)
        {
            try
            {
                //1.查询本地的update.xml配置文件
                //2.如果不存在则直接下载
                //3.如果存在则下载中心update.xml配置文件
                //4.比对两个版本的大小
                //5.如果本地版本小则直接下载，大则放弃下载

                FileInfo finfo = new FileInfo(rootpath + updatexml);
                if (finfo.Exists == false)//本地不存在，直接下载
                {
                    File.Delete(updatexml);
                    File.Delete(updatezip);
                    downloadRemoteFile(updatexml, updatezip, _clientLink);
                }
                else//本地存在升级包文件
                {
                    Version localver = readLocalUpdateXml(updatexml);
                    Version remotever = getRemoteUpdateXml(updatexml, _clientLink);
                    int tm = localver.CompareTo(remotever);
                    if (tm < 0)//本地版本小
                    {
                        File.Delete(updatexml);
                        File.Delete(updatezip);
                        downloadRemoteFile(updatexml, updatezip, _clientLink);
                    }
                }
            }
            catch(Exception err) {
                CoreFrame.Common.MiddlewareLogHelper.WriterLog(err.Message + err.StackTrace);
            }
        }

        private static void downloadRemoteFile(string updatexml, string updatezip, ClientLink _clientLink)
        {
            DownFile df = new DownFile();
            df.clientId = Guid.NewGuid().ToString();
            df.DownKey = Guid.NewGuid().ToString();
            df.FileName = updatexml;
            df.FileType = 0;
            FileStream fs = new FileStream(rootpath + updatexml, FileMode.Create, FileAccess.Write);
            _clientLink.RootDownLoadFile(df, fs, null);

            df = new DownFile();
            df.clientId = Guid.NewGuid().ToString();
            df.DownKey = Guid.NewGuid().ToString();
            df.FileName = updatezip;
            df.FileType = 0;
            fs = new FileStream(rootpath + updatezip, FileMode.Create, FileAccess.Write);
            _clientLink.RootDownLoadFile(df, fs, (delegate (int _num)
            {
                MiddlewareLogHelper.WriterLog(LogType.MidLog, true, System.Drawing.Color.Black, "升级包下载进度：%" + _num);
            }));
        }

        private static Version readLocalUpdateXml(string updatexml)
        {
            System.Xml.XmlDocument xmlDoc = new System.Xml.XmlDocument();
            xmlDoc.Load(rootpath + updatexml);
            System.Xml.XmlNode xn = xmlDoc.DocumentElement.SelectSingleNode("AppVersion");
            Version ver = new Version(xn.InnerText);
            return ver;
        }

        private static Version getRemoteUpdateXml(string updatexml, ClientLink _clientLink)
        {
            DownFile df = new DownFile();
            df.clientId = Guid.NewGuid().ToString();
            df.DownKey = Guid.NewGuid().ToString();
            df.FileName = updatexml;
            df.FileType = 1;
            //MemoryStream update_ms = new MemoryStream();
            MemoryStream ms = new MemoryStream();
            _clientLink.RootDownLoadFile(df, ms, null);
            //ms.CopyTo(update_ms);
            String str = System.Text.Encoding.Default.GetString(ms.ToArray());
            ms.Close();
            System.Xml.XmlDocument xmlDoc = new System.Xml.XmlDocument();
            xmlDoc.LoadXml(str);
            System.Xml.XmlNode xn = xmlDoc.DocumentElement.SelectSingleNode("AppVersion");
            Version ver = new Version(xn.InnerText);
            return ver;
        }

        //从根节点下载
        private static void downloadRemotePlugin(string pluginzip, ClientLink _clientLink)
        {
            DownFile df = new DownFile();
            df.clientId = Guid.NewGuid().ToString();
            df.DownKey = Guid.NewGuid().ToString();
            df.FileName = pluginzip;
            df.FileType = 0;
            FileStream fs = new FileStream(rootpath + pluginzip, FileMode.Create, FileAccess.Write);
            try
            {
                _clientLink.RootDownLoadFile(df, fs, (delegate (int _num)//从根节点下载
                {
                    MiddlewareLogHelper.WriterLog(LogType.MidLog, true, System.Drawing.Color.Black, "插件包下载进度：%" + _num);
                }));
            }
            catch (Exception err){
                CoreFrame.Common.MiddlewareLogHelper.WriterLog(err.Message+err.StackTrace);
            }
        }

        /// <summary>
        /// 延时函数
        /// </summary>
        /// <param name="delayTime">需要延时多少秒</param>
        /// <returns></returns>
        static bool Delay(int delayTime)
        {
            DateTime now = DateTime.Now;
            int s;
            do
            {
                TimeSpan spand = DateTime.Now - now;
                s = spand.Seconds;
                System.Windows.Forms.Application.DoEvents();
            }
            while (s < delayTime);
            return true;
        }
    }
}
