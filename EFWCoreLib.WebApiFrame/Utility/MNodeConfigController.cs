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
using Newtonsoft.Json;

namespace EFWCoreLib.WebAPI.Utility
{
    /// <summary>
    /// 主机配置
    /// http://localhost:8021/MNodeConfig/{action}/{id}
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
        //获取客户端列表
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
        //获取插件服务
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
        //获取日志
        [HttpGet]
        public string DebugLog(string logtype, string date)
        {
            try
            {
                if (string.IsNullOrEmpty(logtype))
                {
                    return "";
                }
                else {
                    date = string.IsNullOrEmpty(date) ? DateTime.Now.ToString("yyyy-MM") : date;
                    string args = "logtype=" + logtype + "&date=" + date.Replace("-", "");
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
                return WebApiGlobal.normalIPC.CallCmd(eprocess, method, arg);
                //return true;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        //获取所有服务
        [HttpGet]
        public Object GetAllServices()
        {
            try
            {
                //string data= WebApiGlobal.normalIPC.CallCmd(IPCName.GetProcessName(IPCType.efwplusBase), "getallservices", null);
                //List<WcfFrame.ServerManage.dwPlugin> plist = JsonConvert.DeserializeObject<List<WcfFrame.ServerManage.dwPlugin>>(data);
                List<EFWCoreLib.WcfFrame.ServerManage.dwPlugin> plist = ClientLinkManage.CreateConnection("Test").GetWcfServicesAllInfo();

                List<amazeuitreenode> tree = new List<amazeuitreenode>();
                foreach (var p in plist)
                {
                    amazeuitreenode nodep = new amazeuitreenode();
                    nodep.title = p.pluginname;
                    nodep.type = "folder";
                    nodep.childs = new List<amazeuitreenode>();
                    tree.Add(nodep);
                    foreach (var c in p.controllerlist)
                    {
                        amazeuitreenode nodec = new amazeuitreenode();
                        nodec.title = c.controllername;
                        nodec.type = "folder";
                        nodec.childs = new List<amazeuitreenode>();
                        nodep.childs.Add(nodec);

                        foreach (var m in c.methodlist)
                        {
                            amazeuitreenode nodem = new amazeuitreenode();
                            nodem.title = m;
                            nodem.type = "item";
                            nodem.attr = new Dictionary<string, string>();
                            nodem.attr.Add("plugin", p.pluginname);
                            nodem.attr.Add("controller", c.controllername);
                            nodem.attr.Add("method", m);
                            nodec.childs.Add(nodem);
                        }
                    }
                }
                return tree;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        //测试服务
        [HttpGet]
        public string TestServices(string plugin, string controller, string method, string para)
        {
            try
            {
                Action<ClientRequestData> requestAction = ((ClientRequestData request) =>
                {
                    request.Iscompressjson = false;
                    request.Isencryptionjson = false;
                    request.Serializetype = SerializeType.Newtonsoft;
                    request.LoginRight = new CoreFrame.Business.SysLoginRight(1);
                    request.SetJsonData(para);
                });

                ServiceResponseData response = InvokeWcfService(plugin, controller, method, requestAction);
                return response.GetJsonData();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        //获取任务列表
        [HttpGet]
        public Object GetTaskList()
        {
            try
            {
                string data = WebApiGlobal.normalIPC.CallCmd(IPCName.GetProcessName(IPCType.efwplusBase), "gettasklist", null);
                List<TaskConfig> tasklist = JsonConvert.DeserializeObject<List<TaskConfig>>(data);
                return tasklist;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        //获取机器码
        [HttpGet]
        public string GetMachineCode()
        {
            try
            {
                string data = WebApiGlobal.normalIPC.CallCmd(IPCName.GetProcessName(IPCType.efwplusBase), "getmachinecode", null);
                return data;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        //激活注册码
        [HttpGet]
        public bool ActivateRegCode(string regcode)
        {
            try
            {
                string args = "regcode=" + regcode;
                string data = WebApiGlobal.normalIPC.CallCmd(IPCName.GetProcessName(IPCType.efwplusBase), "activateregcode", args);
                if (data == "true")
                    return true;
                else
                    return false;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
    
}
