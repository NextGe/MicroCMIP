using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EFWCoreLib.CoreFrame.Mongodb;

namespace EFWCoreLib.WcfFrame.Utility.MonitorPlatform
{
    public class MidNode: AbstractMongoModel
    {
        public string identify { get; set; }
        public string nodename { get; set; }
        /// <summary>
        /// 机器码
        /// </summary>
        public string machinecode { get; set; }
        /// <summary>
        /// 注册码
        /// </summary>
        public string regcode { get; set; }

        public string createdate { get; set; }
        public string memo { get; set; }
        /// <summary>
        /// 停用 0正常 1停用
        /// </summary>
        public int delflag { get; set; }
    }
}
