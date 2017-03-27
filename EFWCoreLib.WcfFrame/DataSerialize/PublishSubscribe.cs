using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace EFWCoreLib.WcfFrame.DataSerialize
{
    /// <summary>
    /// 发布服务对象
    /// </summary>
    [DataContract]
    public class PublishServiceObject
    {

        /// <summary>
        /// 发布服务名称标识
        /// </summary>
        [DataMember]
        public string publishServiceName { get; set; }
        /// <summary>
        /// 服务中文说明
        /// </summary>
        [DataMember]
        public string explain { get; set; }

        /// <summary>
        /// 是否发布
        /// </summary>
        //public bool whether { get; set; }
        //[DataMember]
        //public string pluginname { get; set; }
        //[DataMember]
        //public string controller { get; set; }
        //[DataMember]
        //public string method { get; set; }
        //[DataMember]
        //public string argument { get; set; }
    }
}
