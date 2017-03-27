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
            MNodePlugin mp= RemotePluginManage.GetLocalPlugin();

            List<dwPlugin> pluginlist = new List<dwPlugin>();
            foreach (var pname in mp.LocalPlugin)
            {
                CoreFrame.Plugin.ModulePlugin item = CoreFrame.Init.AppPluginManage.PluginDic[pname];
                dwPlugin p = new dwPlugin();
                p.pluginname = pname;
                p.controllerlist = new List<dwController>();
                List<WcfControllerAttributeInfo> cmdControllerList = (List<WcfControllerAttributeInfo>)item.cache.GetData(pname + "@" + "wcfControllerAttributeList");
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

        //根节点远程获取服务
        public static string RootRemoteGetServices(string identify)
        {
            if (WcfGlobal.Identify == identify)
            {
                return GetServiceConfig();
            }
            else
            {
                MNodePath NodePath = null;
                MNodeTree mtree = new MNodeTree();
                mtree.LoadCache();
                NodePath = mtree.CalculateMNodePath(WcfGlobal.Identify, identify);
                return ReplyRemoteGetServices(NodePath);
            }
        }
        //根节点回调获取远程服务
        public static string ReplyRemoteGetServices(MNodePath NodePath)
        {
            NodePath.NextStep();//节点路径下一步
            if (NodePath.IsEndMNode)//到达终节点
            {
                return GetServiceConfig();
            }
            else
            {
                foreach (var client in ClientManage.ClientDic)
                {
                    if (client.Value.IsMNode && client.Value.ServerIdentify == NodePath.nextMNode)
                    {
                        return client.Value.dataReply.ReplyRemoteGetServices(NodePath);
                    }
                }
                return null;
            }
        }
    }

    
}
