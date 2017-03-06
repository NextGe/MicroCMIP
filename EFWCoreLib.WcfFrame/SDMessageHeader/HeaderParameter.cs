using EFWCoreLib.CoreFrame.Business;
using EFWCoreLib.WcfFrame.DataSerialize;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace EFWCoreLib.WcfFrame.SDMessageHeader
{

    /// <summary>
    /// 自定义消息头参数
    /// </summary>
    [DataContract]
    public class HeaderParameter
    {
        /// <summary>
        /// 路由命令
        /// </summary>
        [DataMember]
        public string cmd { get; set; }
        /// <summary>
        /// 路由ID
        /// </summary>
        [DataMember]
        public string routerid { get; set; }
        /// <summary>
        /// 插件名称
        /// </summary>
        [DataMember]
        public string pluginname { get; set; }
        /// <summary>
        /// 路由服务重新回调节点标识
        /// </summary>
        [DataMember]
        public string replyidentify { get; set; }
        /// <summary>
        /// 令牌
        /// </summary>
        [DataMember]
        public string token { get; set; }
        /// <summary>
        /// 压缩Json字符
        /// </summary>
        [DataMember]
        public bool iscompressjson { get; set; }
        /// <summary>
        /// 加密Json字符
        /// </summary>
        [DataMember]
        public bool isencryptionjson { get; set; }
        /// <summary>
        /// 序列化类型
        /// </summary>
        [DataMember]
        public SerializeType serializetype { get; set; }

        /// <summary>
        /// 登录信息
        /// </summary>
        [DataMember]
        public SysLoginRight LoginRight { get; set; }
        /// <summary>
        /// 请求节点路径
        /// </summary>
        [DataMember]
        public MNodePath NodePath { get; set; }
    }
    
}
