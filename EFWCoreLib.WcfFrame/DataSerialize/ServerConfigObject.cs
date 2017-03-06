using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace EFWCoreLib.WcfFrame.DataSerialize
{
    /// <summary>
    /// 服务端配置对象
    /// </summary>
    [DataContract]
    public class ServerConfigObject
    {
        //中间件唯一标识
        public string Identify { get; set; }
        //中间件显示名称
        public string HostName { get; set; }
        public bool IsToken { get; set; }
        /// <summary>
        /// 是否心跳
        /// </summary>
        [DataMember]
        public bool IsHeartbeat { get; set; }
        [DataMember]
        public int HeartbeatTime { get; set; }
        [DataMember]
        public bool IsMessage { get; set; }
        [DataMember]
        public int MessageTime { get; set; }
        [DataMember]
        public bool IsCompressJson { get; set; }
        [DataMember]
        public bool IsEncryptionJson { get; set; }
        [DataMember]
        public int SerializeType { get; set; }
    }
}
