using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using EFWCoreLib.CoreFrame.Business.AttributeInfo;
using EFWCoreLib.CoreFrame.Common;
using EFWCoreLib.CoreFrame.Init;
using EFWCoreLib.CoreFrame.ProcessManage;
using EFWCoreLib.WcfFrame;
using EFWCoreLib.WcfFrame.DataSerialize;
using EFWCoreLib.WebApiFrame;
using EFWCoreLib.WebFrame.WebAPI;

namespace EFWCoreLib.WebAPI.Utility
{
    /// <summary>
    /// 主机配置
    /// http://localhost:8021/hostconfig/hostconfig/showconfiginfo
    /// </summary>
    [efwplusApiController(PluginName = "coresys")]
    public class MNodeConfigController : WebApiController
    {

        //获取配置信息
        [HttpGet]
        public string ShowConfig()
        {
            try
            {
                return WebApiGlobal.normalIPC.CallCmd(IPCName.GetProcessName(IPCType.efwplusBase), "getmnodeconfig", null);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        //获取配置信息
        [HttpGet]
        public string ShowText()
        {
            try
            {
                return WebApiGlobal.normalIPC.CallCmd(IPCName.GetProcessName(IPCType.efwplusBase), "getmnodetext", null);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        [HttpGet]
        public string ClientList()
        {
            try
            {
                return WebApiGlobal.normalIPC.CallCmd(IPCName.GetProcessName(IPCType.efwplusBase), "clientlist", null);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        [HttpGet]
        public string SeviceList()
        {
            try
            {
                return WebApiGlobal.normalIPC.CallCmd(IPCName.GetProcessName(IPCType.efwplusBase), "sevicelist", null);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        [HttpGet]
        public string DebugLog(string logtype,string date)
        {
            try
            {
                if (string.IsNullOrEmpty(logtype) || string.IsNullOrEmpty(date))
                {
                    return "";
                }
                else {
                    string args = "logtype=" + logtype + "&date=" + date.Replace("-","");
                    return WebApiGlobal.normalIPC.CallCmd(IPCName.GetProcessName(IPCType.efwplusBase), "debuglog", args);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        //执行命令
        [HttpGet]
        public Object ExecuteCmd(string eprocess, string method, string arg)
        {
            try
            {
                WebApiGlobal.normalIPC.CallCmd(eprocess, method, arg);
                return true;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
