using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using EFWCoreLib.CoreFrame.Business.AttributeInfo;
using EFWCoreLib.CoreFrame.ProcessManage;
using EFWCoreLib.WebFrame.WebAPI;

namespace EFWCoreLib.WebApiFrame.Utility
{
    /// <summary>
    /// 监控平台
    /// </summary>
    [efwplusApiController(PluginName = "coresys")]
    public class MonitorController : WebApiController
    {
        [HttpGet]
        public string hello()
        {
            return "hello world";
        }
        /// <summary>
        /// 初始化监控平台
        /// </summary>
        [HttpGet]
        public string InitMonitor()
        {
            //WcfFrame.Utility.MonitorPlatform.MonitorPlatformManage.Init();
            return WebApiGlobal.normalIPC.CallCmd(IPCName.GetProcessName(IPCType.efwplusBase), "initmonitor", null); ;
        }
    }
}
