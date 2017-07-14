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
        /// 获取本地插件
        /// </summary>
        /// <returns></returns>
        public static MNodePlugin GetLocalPlugin()
        {
            MNodePlugin localPlugin = GetCachePlugin();
            if (localPlugin == null)
            {
                localPlugin = new MNodePlugin();
                localPlugin.ServerIdentify = WcfGlobal.Identify;
                localPlugin.PathStrategy = 0;
                localPlugin.LocalPlugin = CoreFrame.Init.AppPluginManage.PluginDic.Keys.ToList();
                localPlugin.RemotePlugin = new List<RemotePlugin>();
            }
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
                    //调试
                    CoreFrame.Common.MiddlewareLogHelper.WriterLog(data.value);
                    return JsonConvert.DeserializeObject<MNodePlugin>(data.value);
                }
            }
            return null;
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
