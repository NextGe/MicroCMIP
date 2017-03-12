using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using EFWCoreLib.CoreFrame.Business.AttributeInfo;
using EFWCoreLib.CoreFrame.Mongodb;
using EFWCoreLib.CoreFrame.ProcessManage;
using EFWCoreLib.WcfFrame.Utility.MonitorPlatform;
using EFWCoreLib.WebFrame.WebAPI;
using MongoDB.Bson;
using MongoDB.Driver.Builders;
using Newtonsoft.Json;

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
    }
}
