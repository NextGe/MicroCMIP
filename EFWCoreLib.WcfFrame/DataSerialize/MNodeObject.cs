using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace EFWCoreLib.WcfFrame.DataSerialize
{
    /// <summary>
    /// 中间件节点
    /// </summary>
    [DataContract]
    public class MNodeObject
    {
        /// <summary>
        /// 中间件节点标识
        /// </summary>
        [DataMember]
        public string ServerIdentify { get; set; }
        /// <summary>
        /// 中间件节点名称
        /// </summary>
        [DataMember]
        public string ServerName { get; set; }
        /// <summary>
        /// 是否在线
        /// </summary>
        [DataMember]
        public bool IsConnect { get; set; }
        /// <summary>
        /// 指向的中间件节点标识
        /// </summary>
        [DataMember]
        public string PointToMNode { get; set; }
    }
}
