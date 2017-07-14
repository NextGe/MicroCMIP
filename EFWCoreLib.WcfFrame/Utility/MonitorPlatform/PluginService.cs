using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EFWCoreLib.CoreFrame.Mongodb;

namespace EFWCoreLib.WcfFrame.Utility.MonitorPlatform
{
    /// <summary>
    /// 插件服务
    /// </summary>
    public class PluginService : AbstractMongoModel
    {
        /// <summary>
        /// 插件服务类型 0默认服务 1适配器服务
        /// </summary>
        public int type { get; set; }
        /// <summary>
        /// 插件名
        /// </summary>
        public string pluginname { get; set; }
        /// <summary>
        /// 标题名称
        /// </summary>
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

    /// <summary>
    /// 插件服务脚本
    /// </summary>
    public class PluginServiceScript : AbstractMongoModel
    {
        /// <summary>
        /// 插件名
        /// </summary>
        public string pluginname { get; set; }

        /// <summary>
        /// 脚本
        /// </summary>
        public string script { get; set; }
    }
}
