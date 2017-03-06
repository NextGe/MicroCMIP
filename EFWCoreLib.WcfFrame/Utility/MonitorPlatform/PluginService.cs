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
