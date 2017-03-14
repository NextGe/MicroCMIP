using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EFWCoreLib.CoreFrame.Mongodb;

namespace EFWCoreLib.WcfFrame.Utility.MonitorPlatform
{
    /// <summary>
    /// 中间件节点配置插件服务
    /// </summary>
    public class MNodePService : AbstractMongoModel
    {
        //public string nodename { get; set; }
        public string identify { get; set; }
        /// <summary>
        /// 路径策略 0随机 1最短路径
        /// </summary>
        public int pathstrategy { get; set; }
        /// <summary>
        /// 本地插件，插件名称
        /// </summary>
        public List<string> localplugin { get; set; }
        /// <summary>
        /// 远程插件，插件名称
        /// </summary>
        public List<RemotePService> remoteplugin { get; set; }
    }
    /// <summary>
    /// 远程插件服务
    /// </summary>
    public class RemotePService
    {
        public string pluginname { get; set; }
        public List<string> mnodeidentify { get; set; }
    }
}
