using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using EFWCoreLib.CoreFrame.Business.AttributeInfo;
using EFWCoreLib.CoreFrame.Mongodb;
using EFWCoreLib.CoreFrame.ProcessManage;
using EFWCoreLib.WcfFrame.DataSerialize;
using EFWCoreLib.WcfFrame.Utility.MonitorPlatform;
using EFWCoreLib.WebApiFrame;
using EFWCoreLib.WebFrame.WebAPI;
using MongoDB.Bson;
using MongoDB.Driver.Builders;
using Newtonsoft.Json;

namespace EFWCoreLib.WebAPI.Utility
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
        /// <summary>
        /// 获取节点
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public Object GetMNodeList()
        {
            if (WebApiGlobal.IsRootMNode)
            {
                MongoHelper<MidNode> helperNode = new MongoHelper<MidNode>(WebApiGlobal.MongoConnStr, MonitorPlatformManage.dbName);
                List<MidNode> nodeList = helperNode.FindAll(null);
                JsonSerializerSettings jsetting = new JsonSerializerSettings();
                jsetting.DefaultValueHandling = DefaultValueHandling.Ignore;
                return JsonConvert.SerializeObject(nodeList, jsetting);
            }

            return null;
        }
        /// <summary>
        /// 保存节点
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public bool SaveMNode([FromUri] MidNode node)
        {
            if (WebApiGlobal.IsRootMNode)
            {
                MongoHelper<MidNode> helperNode = new MongoHelper<MidNode>(WebApiGlobal.MongoConnStr, MonitorPlatformManage.dbName);
                if (string.IsNullOrEmpty(node.id_string))
                {
                    node.id = ObjectId.Empty;
                    node.delflag = 0;
                    node.identify = null;
                    node.regcode = null;
                    node.identify = DateTime.Now.Ticks.ToString();//新增的时候生成标识码
                    node.createdate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    helperNode.Insert(node);
                }
                else
                {
                    MidNode _node = helperNode.Find(Query.EQ("_id", new ObjectId(node.id_string)));
                    _node.nodename = node.nodename;
                    _node.machinecode = node.machinecode;
                    _node.memo = node.memo;
                    _node.regcode = node.regcode;
                    //_node.identify = node.identify;
                    helperNode.Update(_node);
                }

                return true;
            }
            return false;
        }
        /// <summary>
        /// 生成注册码
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public Object CreateRegCode(string identify, string machinecode, string regdate)
        {
            try
            {
                if (WebApiGlobal.IsRootMNode)
                {
                    //string identify = DateTime.Now.Ticks.ToString();
                    regdate = Convert.ToDateTime(regdate).ToString("yyyyMMdd");
                    string regcode = Encryption.EncryPW(TimeCDKEY.CreatSerialNumber(identify, machinecode, regdate), "kakake!@#123");
                    return new { identify = identify, regcode = regcode };
                }

                throw new Exception("没有权限");
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// 启用或停用中间件节点
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public bool OnOffMidNode(string id)
        {
            if (WebApiGlobal.IsRootMNode)
            {
                MongoHelper<MidNode> helperNode = new MongoHelper<MidNode>(WebApiGlobal.MongoConnStr, MonitorPlatformManage.dbName);
                MidNode node = helperNode.Find(Query.EQ("_id", new ObjectId(id)));
                //node.id = ObjectId.Empty;
                node.delflag = node.delflag == 0 ? 1 : 0;
                helperNode.Update(node);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 获取插件服务
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public Object GetPluginList()
        {
            if (WebApiGlobal.IsRootMNode)
            {
                MongoHelper<PluginService> helperPlugin = new MongoHelper<PluginService>(WebApiGlobal.MongoConnStr, MonitorPlatformManage.dbName);
                List<PluginService> pluginList = helperPlugin.FindAll(null);
                JsonSerializerSettings jsetting = new JsonSerializerSettings();
                jsetting.DefaultValueHandling = DefaultValueHandling.Ignore;
                return JsonConvert.SerializeObject(pluginList, jsetting);
            }

            return null;
        }

        [HttpGet]
        public bool SavePlugin(string para)
        {
            if (WebApiGlobal.IsRootMNode)
            {
                PluginService plugin = JsonConvert.DeserializeObject<PluginService>(para);
                MongoHelper<PluginService> helperPlugin = new MongoHelper<PluginService>(WebApiGlobal.MongoConnStr, MonitorPlatformManage.dbName);
                if (string.IsNullOrEmpty(plugin.id_string))
                {
                    PluginService _plugin = helperPlugin.Find(Query.EQ("pluginname", new BsonString(plugin.pluginname)));
                    if (_plugin == null)
                    {
                        plugin.id = ObjectId.Empty;
                        plugin.delflag = 0;
                        helperPlugin.Insert(plugin);
                    }
                }
                else
                {
                    PluginService _plugin = helperPlugin.Find(Query.EQ("_id", new ObjectId(plugin.id_string)));
                    _plugin.pluginname = plugin.pluginname;
                    _plugin.title = plugin.title;
                    _plugin.versions = plugin.versions;
                    _plugin.author = plugin.author;
                    _plugin.introduce = plugin.introduce;
                    helperPlugin.Update(_plugin);
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// 启用或停用插件服务
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public bool OnOffPlugin(string id)
        {
            if (WebApiGlobal.IsRootMNode)
            {
                MongoHelper<PluginService> helperPlugin = new MongoHelper<PluginService>(WebApiGlobal.MongoConnStr, MonitorPlatformManage.dbName);
                PluginService plugin = helperPlugin.Find(Query.EQ("_id", new ObjectId(id)));
                //node.id = ObjectId.Empty;
                plugin.delflag = plugin.delflag == 0 ? 1 : 0;
                helperPlugin.Update(plugin);
                return true;
            }
            return false;
        }

        [HttpGet]
        public Object GetMNodePluginViewData(string identify)
        {
            if (WebApiGlobal.IsRootMNode)
            {
                MongoHelper<MidNode> helperNode = new MongoHelper<MidNode>(WebApiGlobal.MongoConnStr, MonitorPlatformManage.dbName);
                List<MidNode> nodeList = helperNode.FindAll(null);

                MongoHelper<PluginService> helperPlugin = new MongoHelper<PluginService>(WebApiGlobal.MongoConnStr, MonitorPlatformManage.dbName);
                List<PluginService> pluginList = helperPlugin.FindAll(null);

                Object tree = GetMNodePService(identify);

                JsonSerializerSettings jsetting = new JsonSerializerSettings();
                jsetting.DefaultValueHandling = DefaultValueHandling.Ignore;
                return JsonConvert.SerializeObject(new { nodes = nodeList, plugins = pluginList, tree = tree }, jsetting);
            }

            return null;
        }

        /// <summary>
        /// 获取节点对应的插件服务
        /// </summary>
        /// <param name="identify"></param>
        /// <returns></returns>
        [HttpGet]
        public Object GetMNodePService(string identify)
        {
            if (WebApiGlobal.IsRootMNode)
            {
                List<amazeuitreenode> tree = new List<amazeuitreenode>();
                MongoHelper<MNodePService> helpernps = new MongoHelper<MNodePService>(WebApiGlobal.MongoConnStr, MonitorPlatformManage.dbName);
                MNodePService ps = helpernps.Find(Query.EQ("identify", new BsonString(identify)));
                if (ps != null)
                {
                    MongoHelper<MidNode> helperNode = new MongoHelper<MidNode>(WebApiGlobal.MongoConnStr, MonitorPlatformManage.dbName);
                    List<MidNode> nodeList = helperNode.FindAll(null);

                    amazeuitreenode root = new amazeuitreenode();
                    root.title = nodeList.Find(x => x.identify == ps.identify).nodename + "(" + ps.identify + ")";
                    root.type = "folder";
                    root.childs = new List<amazeuitreenode>();
                    tree.Add(root);
                    //本地插件
                    amazeuitreenode l_root = new amazeuitreenode();
                    l_root.title = "本地插件服务";
                    l_root.type = "folder";
                    l_root.childs = new List<amazeuitreenode>();
                    root.childs.Add(l_root);
                    foreach (string p in ps.localplugin)
                    {
                        amazeuitreenode node_lp = new amazeuitreenode();
                        node_lp.title = p;
                        node_lp.type = "item";
                        node_lp.attr = new Dictionary<string, string>();
                        node_lp.attr.Add("type", "localplugin");
                        node_lp.attr.Add("value", p);
                        l_root.childs.Add(node_lp);
                    }
                    //远程插件
                    amazeuitreenode r_root = new amazeuitreenode();
                    r_root.title = "远程插件服务";
                    r_root.type = "folder";
                    r_root.childs = new List<amazeuitreenode>();
                    root.childs.Add(r_root);
                    foreach (var p in ps.remoteplugin)
                    {
                        amazeuitreenode node_rp = new amazeuitreenode();
                        node_rp.title = p.pluginname;
                        node_rp.type = "folder";
                        //node_rp.attr = new Dictionary<string, string>();
                        //node_rp.attr.Add("type", "remoteplugin");
                        //node_rp.attr.Add("value", p.pluginname);
                        node_rp.childs = new List<amazeuitreenode>();
                        r_root.childs.Add(node_rp);
                        foreach (string i in p.mnodeidentify)
                        {
                            amazeuitreenode node = new amazeuitreenode();
                            node.title = nodeList.Find(x => x.identify == i).nodename;
                            node.type = "item";
                            node.attr = new Dictionary<string, string>();
                            node.attr.Add("type", "remoteplugin");
                            node.attr.Add("value", p.pluginname);
                            node_rp.childs.Add(node);
                        }
                    }

                }
                return tree;
            }
            return null;
        }

        /// <summary>
        /// 增加节点对应的插件服务
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public bool AddMNodePService(string identify, int type, string data)
        {
            if (WebApiGlobal.IsRootMNode)
            {
                MongoHelper<MNodePService> helpernps = new MongoHelper<MNodePService>(WebApiGlobal.MongoConnStr, MonitorPlatformManage.dbName);
                MNodePService ps = helpernps.Find(Query.EQ("identify", new BsonString(identify)));
                if (ps == null)//新增
                {
                    ps = new MNodePService();
                    ps.id = ObjectId.Empty;
                    ps.identify = identify;
                    ps.pathstrategy = 0;
                    ps.localplugin = new List<string>();
                    ps.remoteplugin = new List<RemotePService>();
                    helpernps.Insert(ps);
                }

                if (type == 0)//本地
                {
                    List<string> add_lp = JsonConvert.DeserializeObject<List<string>>(data);
                    foreach (string p in add_lp)
                    {
                        if (ps.localplugin.FindIndex(x => x == p) == -1)
                        {
                            ps.localplugin.Add(p);
                        }
                    }
                }
                else//远程
                {
                    RemotePService add_rp = JsonConvert.DeserializeObject<RemotePService>(data);
                    RemotePService rps = ps.remoteplugin.Find(x => x.pluginname == add_rp.pluginname);
                    if (rps == null)
                    {
                        ps.remoteplugin.Add(add_rp);
                    }
                    else
                    {
                        rps = add_rp;
                    }

                }

                helpernps.Update(ps);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 删除节点对应的插件服务
        /// </summary>
        /// <param name="identify">节点标识</param>
        /// <param name="type">类型 0本地 1远程</param>
        /// <param name="pluginname">插件名</param>
        /// <returns></returns>
        [HttpGet]
        public bool DelMNodePService(string identify, int type, string pluginname)
        {
            if (WebApiGlobal.IsRootMNode)
            {
                MongoHelper<MNodePService> helpernps = new MongoHelper<MNodePService>(WebApiGlobal.MongoConnStr, MonitorPlatformManage.dbName);
                MNodePService ps = helpernps.Find(Query.EQ("identify", new BsonString(identify)));
                if (ps != null)
                {
                    if (type == 0)//本地
                    {
                        if (ps.localplugin.FindIndex(x => x == pluginname) != -1)
                            ps.localplugin.Remove(pluginname);
                    }
                    else//远程
                    {
                        RemotePService rps= ps.remoteplugin.Find(x => x.pluginname == pluginname);
                        if (rps != null)
                            ps.remoteplugin.Remove(rps);
                    }
                    helpernps.Update(ps);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 获取节点监控图
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public Object GetMonitorMap()
        {
            if (WebApiGlobal.IsRootMNode)
            {
                string data = WebApiGlobal.normalIPC.CallCmd(IPCName.GetProcessName(IPCType.efwplusBase), "getmnodetree", null);
                MNodeTree nodetree = JsonConvert.DeserializeObject<MNodeTree>(data);

                List<amazeuitreenode> tree = new List<amazeuitreenode>();
                amazeuitreenode root = new amazeuitreenode();
                root.title = nodetree.RootMNode.ServerName+"("+ nodetree.RootMNode.ServerIdentify + ")";
                root.type = "folder";
                root.childs = new List<amazeuitreenode>();
                root.attr = new Dictionary<string, string>();
                root.attr.Add("identify", nodetree.RootMNode.ServerIdentify);
                root.attr.Add("icon", "am-icon-home");
                tree.Add(root);

                //在线并排除根节点
                List<MNodeObject> onmnodelist = nodetree.AllMNodeList.FindAll(x => x.IsConnect == true && x.ServerIdentify!= nodetree.RootMNode.ServerIdentify);
                loadMonitorMap(nodetree.RootMNode.ServerIdentify, root, onmnodelist);

                amazeuitreenode offroot = new amazeuitreenode();
                offroot.title = "离线中间件节点";
                offroot.type = "folder";
                offroot.childs = new List<amazeuitreenode>();
                offroot.attr = new Dictionary<string, string>();
                offroot.attr.Add("icon", "am-icon-power-off");
                tree.Add(offroot);
                List<MNodeObject> offmnodelist = nodetree.AllMNodeList.FindAll(x => x.IsConnect == false);
                foreach (var o in offmnodelist)
                {
                    amazeuitreenode node = new amazeuitreenode();
                    node.title = o.ServerName + "(" + o.ServerIdentify + ")";
                    node.type = "item";
                    node.attr = new Dictionary<string, string>();
                    //node.attr.Add("identify", o.ServerIdentify);
                    node.attr.Add("icon", "am-icon-desktop");
                    offroot.childs.Add(node);
                }
                return tree;
            }
            return null;
        }

        private void loadMonitorMap(string p_identify, amazeuitreenode p_node, List<MNodeObject> onmnodelist)
        {
            List<MNodeObject> nodelist = onmnodelist.FindAll(x => x.PointToMNode == p_identify);
            foreach (var o in nodelist)
            {
                amazeuitreenode node = new amazeuitreenode();
                node.title = o.ServerName + "(" + o.ServerIdentify + ")";
                node.type = "folder";
                node.attr = new Dictionary<string, string>();
                node.attr.Add("identify", o.ServerIdentify);
                node.attr.Add("icon", "am-icon-desktop");
                p_node.childs.Add(node);

                if (onmnodelist.FindIndex(x => x.PointToMNode == o.ServerIdentify) == -1)
                {
                    node.type = "item";
                }
                else {
                    loadMonitorMap(o.ServerIdentify, node, onmnodelist);
                }
            }
        }

        [HttpGet]
        public Object GetRemoteNodeConfig(string identify)
        {
            string args = "identify=" + identify + "&eprocess=efwplusbase&method=rootremotecommand&arg=";
            return WebApiGlobal.normalIPC.CallCmd(IPCName.GetProcessName(IPCType.efwplusBase), "rootremotecommand", args);
        }
    }
}
