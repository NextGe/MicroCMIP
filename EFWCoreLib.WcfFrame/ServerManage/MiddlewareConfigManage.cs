using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EFWCoreLib.CoreFrame.Init.AttributeManager;
using EFWCoreLib.WcfFrame.DataSerialize;
using Newtonsoft.Json;

namespace EFWCoreLib.WcfFrame.ServerManage
{
    /// <summary>
    /// 返回插件信息
    /// </summary>
    public class MiddlewareConfigManage
    {
        /// <summary>
        /// 获取本地的插件服务配置
        /// ？还需要增加显示远程的服务配置
        /// </summary>
        /// <returns></returns>
        public static string GetServiceConfig()
        {
            List<dwPlugin> pluginlist = new List<dwPlugin>();
            foreach (var item in CoreFrame.Init.AppPluginManage.PluginDic)
            {
                dwPlugin p = new dwPlugin();
                p.pluginname = item.Key;
                p.controllerlist = new List<dwController>();
                List<WcfControllerAttributeInfo> cmdControllerList = (List<WcfControllerAttributeInfo>)item.Value.cache.GetData(item.Key + "@" + "wcfControllerAttributeList");
                foreach (var cmd in cmdControllerList)
                {
                    dwController c = new dwController();
                    c.controllername = cmd.controllerName;
                    c.methodlist = new List<string>();
                    foreach (var m in cmd.MethodList)
                    {
                        c.methodlist.Add(m.methodName);
                    }
                    p.controllerlist.Add(c);
                }
                pluginlist.Add(p);
            }

            return JsonConvert.SerializeObject(pluginlist);
        }

        /// <summary>
        /// 客户端连接对象需要从服务端获取的配置
        /// </summary>
        /// <returns></returns>
        public static string GetServerConfig()
        {
            ServerConfigObject config = new ServerConfigObject();
            config.Identify = WcfGlobal.Identify;
            config.HostName = WcfGlobal.HostName;
            config.IsToken = WcfGlobal.IsToken;
            config.IsHeartbeat = true;//写死，不通过配置 ClientManage.IsHeartbeat;
            config.HeartbeatTime = ClientManage.HeartbeatTime;
            config.IsMessage = true; //写死 ClientManage.IsMessage;
            config.MessageTime = 3; //写死 ClientManage.MessageTime;
            config.IsCompressJson = WcfGlobal.IsCompressJson;
            config.IsEncryptionJson = WcfGlobal.IsEncryptionJson;
            config.SerializeType = (int)WcfGlobal.serializeType;

            return JsonConvert.SerializeObject(config);
        }
        /// <summary>
        /// 获取中间件节点配置
        /// </summary>
        /// <returns></returns>
        public static string GetMNodeConfig()
        {
            return "";
        }
    }

    /// <summary>
    /// 服务插件对象
    /// </summary>
    public class dwPlugin
    {
        /// <summary>
        /// 插件名称
        /// </summary>
        public string pluginname { get; set; }
        /// <summary>
        /// 插件内的控制器集合
        /// </summary>
        public List<dwController> controllerlist { get; set; }

    }
    /// <summary>
    /// 服务控制器对象
    /// </summary>
    public class dwController
    {
        /// <summary>
        /// 控制器名称
        /// </summary>
        public string controllername { get; set; }
        /// <summary>
        /// 控制器内的方法集合
        /// </summary>
        public List<string> methodlist { get; set; }
    }
}
