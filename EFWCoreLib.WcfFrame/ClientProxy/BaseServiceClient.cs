using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using EFWCoreLib.WcfFrame.DataSerialize;
using EFWCoreLib.WcfFrame.ServerManage;
using EFWCoreLib.WcfFrame.WcfHandler;

namespace EFWCoreLib.WcfFrame.ClientProxy
{
    public class BaseServiceClient : ClientBase<IClientHandler>, IClientHandler
    {
        public BaseServiceClient(string endpointConfigurationName)
            : base(endpointConfigurationName)
        { }

        public IAsyncResult BeginProcessRequest(string clientId, string plugin, string controller, string method, string jsondata, AsyncCallback callback, object asyncState)
        {
            return this.Channel.BeginProcessRequest(clientId, plugin, controller, method, jsondata, callback, asyncState);
        }

        public string EndProcessRequest(IAsyncResult result)
        {
            return this.Channel.EndProcessRequest(result);
        }

        public string CreateClient(string clientName, string plugin, string replyidentify)
        {
            return this.Channel.CreateClient(clientName,plugin,replyidentify);
        }

        public string GetMNodeConfig()
        {
            return this.Channel.GetMNodeConfig();
        }

        public string GetServerConfig()
        {
            return this.Channel.GetServerConfig();
        }


        public string GetServiceConfig()
        {
            return this.Channel.GetServiceConfig();
        }


        public List<CacheObject> GetDistributedCacheData(List<CacheIdentify> cacheIdList)
        {
            return this.Channel.GetDistributedCacheData(cacheIdList);
        }

        public bool Heartbeat(string clientId)
        {
            return this.Channel.Heartbeat(clientId);
        }

        

        public string ProcessRequest(string clientId, string plugin, string controller, string method, string jsondata)
        {
            return this.Channel.ProcessRequest(clientId, plugin, controller, method, jsondata);
        }

        //public void RegisterRemotePlugin(string ServerIdentify, string[] plugin)
        //{
        //    this.Channel.RegisterRemotePlugin(ServerIdentify, plugin);
        //}

        public void Subscribe(string ServerIdentify, string publishServiceName)
        {
            this.Channel.Subscribe(ServerIdentify, publishServiceName);
        }

        public bool UnClient(string clientId)
        {
            return this.Channel.UnClient(clientId);
        }

        public void UnSubscribe(string ServerIdentify, string publishServiceName)
        {
            this.Channel.UnSubscribe(ServerIdentify, publishServiceName);
        }

        public List<PublishServiceObject> GetPublishServiceList()
        {
            return this.Channel.GetPublishServiceList();
        }

        public void MNodeState(List<MNodeObject> mnodeList)
        {
            this.Channel.MNodeState(mnodeList);
        }

        public string RootMNodeProcessRequest(string key, string jsonpara)
        {
            return this.Channel.RootMNodeProcessRequest(key, jsonpara);
        }
    }

    public class DuplexBaseServiceClient : DuplexClientBase<IClientHandler>, IClientHandler
    {
        public DuplexBaseServiceClient(object callbackInstance, string endpointConfigurationName) : base(callbackInstance, endpointConfigurationName)
        {

        }
        public IAsyncResult BeginProcessRequest(string clientId, string plugin, string controller, string method, string jsondata, AsyncCallback callback, object asyncState)
        {
            return this.Channel.BeginProcessRequest(clientId, plugin, controller, method, jsondata, callback, asyncState);
        }

        public string EndProcessRequest(IAsyncResult result)
        {
            return this.Channel.EndProcessRequest(result);
        }

        public string CreateClient(string clientName, string plugin, string replyidentify)
        {
            return this.Channel.CreateClient(clientName,plugin,replyidentify);
        }

        public string GetMNodeConfig()
        {
            return this.Channel.GetMNodeConfig();
        }

        public string GetServerConfig()
        {
            return this.Channel.GetServerConfig();
        }


        public string GetServiceConfig()
        {
            return this.Channel.GetServiceConfig();
        }

        public List<CacheObject> GetDistributedCacheData(List<CacheIdentify> cacheIdList)
        {
            return this.Channel.GetDistributedCacheData(cacheIdList);
        }

        public bool Heartbeat(string clientId)
        {
            return this.Channel.Heartbeat(clientId);
        }

        

        public string ProcessRequest(string clientId, string plugin, string controller, string method, string jsondata)
        {
            return this.Channel.ProcessRequest(clientId, plugin, controller, method, jsondata);
        }

        //public void RegisterRemotePlugin(string ServerIdentify, string[] plugin)
        //{
        //    this.Channel.RegisterRemotePlugin(ServerIdentify, plugin);
        //}

        public void Subscribe(string ServerIdentify,string publishServiceName)
        {
            this.Channel.Subscribe(ServerIdentify,  publishServiceName);
        }

        public bool UnClient(string clientId)
        {
            return this.Channel.UnClient(clientId);
        }

        public void UnSubscribe(string ServerIdentify, string publishServiceName)
        {
            this.Channel.UnSubscribe(ServerIdentify, publishServiceName);
        }

        public List<PublishServiceObject> GetPublishServiceList()
        {
            return this.Channel.GetPublishServiceList();
        }

        public void MNodeState(List<MNodeObject> mnodeList)
        {
            this.Channel.MNodeState(mnodeList);
        }

        public string RootMNodeProcessRequest(string key, string jsonpara)
        {
            return this.Channel.RootMNodeProcessRequest(key, jsonpara);
        }
    }
}
