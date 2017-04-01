using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EFWCoreLib.CoreFrame.Common;
using EFWCoreLib.CoreFrame.Mongodb;
using EFWCoreLib.CoreFrame.Plugin;
using EFWCoreLib.WcfFrame.DataSerialize;
using EFWCoreLib.WcfFrame.Utility.MonitorPlatform;
using EFWCoreLib.WcfFrame.WcfHandler;
using Newtonsoft.Json;

namespace EFWCoreLib.WcfFrame.ServerManage
{
    /// <summary>
    /// 节点远程插件
    /// </summary>
    public class RemotePluginManage
    {
        private static string cacheName = "mnodeplugin";
        /// <summary>
        /// 开始
        /// </summary>
        public static void Start()
        {
            if (RemotePluginManage.timer == null)
                RemotePluginManage.StartListen();
            else
                RemotePluginManage.timer.Start();
        }
        /// <summary>
        /// 获取本地插件
        /// </summary>
        /// <returns></returns>
        public static MNodePlugin GetLocalPlugin()
        {
            MNodePlugin localPlugin = GetCachePlugin();
            //if (localPlugin == null)
            //{
            //    localPlugin = new MNodePlugin();
            //    localPlugin.ServerIdentify = WcfGlobal.Identify;
            //    localPlugin.PathStrategy = 0;
            //    localPlugin.LocalPlugin = CoreFrame.Init.AppPluginManage.PluginDic.Keys.ToList();
            //    localPlugin.RemotePlugin = new List<RemotePlugin>();
            //}
            //else
            //{
            //    //交集
            //    if (localPlugin.LocalPlugin != null)
            //    {
            //        List<string> rmPList = localPlugin.LocalPlugin.ToList();
            //        foreach (string p in localPlugin.LocalPlugin)
            //        {
            //            if (CoreFrame.Init.AppPluginManage.PluginDic.Keys.ToList().FindIndex(x => x == p) == -1)
            //            {
            //                rmPList.Remove(p);
            //            }
            //        }
            //        localPlugin.LocalPlugin = rmPList.ToList(); 
            //    }
            //}
            return localPlugin;
        }

        //从缓存获取节点插件
        private static MNodePlugin GetCachePlugin()
        {
            CacheObject cobj = DistributedCacheManage.GetLocalCache(cacheName);
            if (cobj != null)
            {
                List<CacheData> cdatalist = cobj.cacheValue;
                CacheData data = cdatalist.Find(x => x.key == WcfGlobal.Identify && x.deleteflag == false);
                if (data != null)
                {
                    return JsonConvert.DeserializeObject<MNodePlugin>(data.value);
                }
            }
            return null;
        }

        //同步节点插件
        public static void SyncMNodePlugin()
        {
            if (WcfGlobal.IsRootMNode)
            {
                List<MNodePlugin> mpList = LoadMongodb();//从Mongodb加载节点服务配置
                Dictionary<string, string> sync_adddata = new Dictionary<string, string>();//需要同步的数据
                Dictionary<string, string> sync_updatedata = new Dictionary<string, string>();//需要同步的数据
                List<string> sync_deldata = new List<string>();//需要同步的数据

                CacheObject cobj = DistributedCacheManage.GetLocalCache(cacheName);
                if (cobj != null)
                {
                    List<CacheData> cdatalist = cobj.cacheValue;
                    //新增的
                    foreach (var n in mpList)
                    {
                        if (cdatalist.FindIndex(x => x.key == n.ServerIdentify) == -1)
                        {
                            sync_adddata.Add(n.ServerIdentify, JsonConvert.SerializeObject(n));
                        }
                    }
                    //删除的
                    foreach (var o in cdatalist)
                    {
                        if (mpList.FindIndex(x => x.ServerIdentify == o.key) == -1)
                        {
                            sync_deldata.Add(o.key);
                        }
                    }

                    //更新的
                    foreach (var o in cdatalist)
                    {
                        MNodePlugin mp = mpList.Find(x => x.ServerIdentify == o.key);
                        if (mp != null && JsonConvert.SerializeObject(mp) != o.value)
                        {
                            sync_updatedata.Add(o.key, JsonConvert.SerializeObject(mp));
                        }
                    }

                }
                else
                {
                    //新增的
                    foreach (var n in mpList)
                    {
                        sync_adddata.Add(n.ServerIdentify, JsonConvert.SerializeObject(n));
                    }
                }

                DistributedCacheManage.SetCache(cacheName, sync_adddata, sync_updatedata, sync_deldata);
                if (sync_adddata.Count > 0 || sync_updatedata.Count > 0 || sync_deldata.Count > 0)
                {
                    UpgradeManage.UpdateUpgrade();//插件服务进行了更新
                }
            }
        }

        //从Mongodb加载数据
        private static List<MNodePlugin> LoadMongodb()
        {
            //从Mongodb获取中间件节点配置的服务
            MongoHelper<MNodePService> helper = new MongoHelper<MNodePService>(WcfGlobal.MongoConnStr, MonitorPlatformManage.dbName);
            List<MNodePService> npList = helper.FindAll(null);
            MongoHelper<PluginService> pshelper = new MongoHelper<PluginService>(WcfGlobal.MongoConnStr, MonitorPlatformManage.dbName);
            List<PluginService> psList = pshelper.FindAll(null);

            List<MNodePlugin> mpList = new List<MNodePlugin>();
            foreach(var ps in npList)
            {
                MNodePlugin mp = new MNodePlugin();
                mp.ServerIdentify = ps.identify;
                mp.PathStrategy = ps.pathstrategy;
                mp.LocalPlugin = ps.localplugin;
                mp.RemotePlugin = new List<RemotePlugin>();
                foreach (var r in ps.remoteplugin)
                {
                    RemotePlugin rp = new RemotePlugin();
                    rp.PluginName = r.pluginname;
                    rp.MNodeIdentify = r.mnodeidentify;
                    mp.RemotePlugin.Add(rp);
                }
                mp.LocalPinfoList = new List<MNPluginInfo>();
                foreach(string p in mp.LocalPlugin)
                {
                    PluginService pservice = psList.Find(x => x.pluginname == p);
                    if (pservice != null)
                    {
                        MNPluginInfo pinfo = new MNPluginInfo();
                        pinfo.pluginname = pservice.pluginname;
                        pinfo.title = pservice.title;
                        pinfo.versions = pservice.versions;
                        pinfo.author = pservice.author;
                        pinfo.introduce = pservice.introduce;
                        pinfo.updatenotes = pservice.updatenotes;
                        pinfo.delflag = pservice.delflag;
                        mp.LocalPinfoList.Add(pinfo);
                    }
                }
                
                mpList.Add(mp);
            }
            return mpList;
        }

        private static System.Timers.Timer timer;
        public static void StartListen()
        {
            timer = new System.Timers.Timer();
            timer.Interval = 5000;//1s
            //timer.Elapsed -= new System.Timers.ElapsedEventHandler(timer_Elapsed);
            timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);
            timer.Start();
        }

        static void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                timer.Enabled = false;
                SyncMNodePlugin();
                timer.Enabled = true;
            }
            catch { timer.Enabled = true; }
        }
    }


    /// <summary>
    /// 本地插件
    /// </summary>
    public class MNodePlugin
    {
        /// <summary>
        /// 节点标识
        /// </summary>
        public string ServerIdentify { get; set; }
        /// <summary>
        /// 路径策略 0随机 1最短路径
        /// </summary>
        public int PathStrategy { get; set; }
        /// <summary>
        /// 本地插件
        /// </summary>
        public List<string> LocalPlugin { get; set; }
        /// <summary>
        /// 远程插件，插件名和远程中间件标识数组
        /// </summary>
        public List<RemotePlugin> RemotePlugin { get; set; }
        /// <summary>
        /// 插件详细信息
        /// </summary>
        public List<MNPluginInfo> LocalPinfoList { get; set; }
    }

    /// <summary>
    /// 远程插件服务
    /// </summary>
    public class RemotePlugin
    {
        public string PluginName { get; set; }
        public List<string> MNodeIdentify { get; set; }
    }

    public class MNPluginInfo
    {
        public string pluginname { get; set; }
        public string title { get; set; }
        /// <summary>
        /// 版本
        /// </summary>
        public string versions { get; set; }
        /// <summary>
        /// 作者
        /// </summary>
        public string author { get; set; }
        /// <summary>
        /// 介绍
        /// </summary>
        public string introduce { get; set; }
        /// <summary>
        /// 更新说明
        /// </summary>
        public string updatenotes { get; set; }
        public int delflag { get; set; }
    }
}
