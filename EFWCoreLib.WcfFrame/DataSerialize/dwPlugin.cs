using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace EFWCoreLib.WcfFrame.DataSerialize
{
    /// <summary>
    /// 服务插件对象
    /// </summary>
    [DataContract]
    public class dwPlugin
    {
        /// <summary>
        /// 插件名称
        /// </summary>
        [DataMember]
        public string pluginname { get; set; }
        /// <summary>
        /// 插件内的控制器集合
        /// </summary>
        [DataMember]
        public List<dwController> controllerlist { get; set; }

    }
    /// <summary>
    /// 服务控制器对象
    /// </summary>
    [DataContract]
    public class dwController
    {
        /// <summary>
        /// 控制器名称
        /// </summary>
        [DataMember]
        public string controllername { get; set; }
        /// <summary>
        /// 控制器内的方法集合
        /// </summary>
        [DataMember]
        public List<string> methodlist { get; set; }
    }
}
