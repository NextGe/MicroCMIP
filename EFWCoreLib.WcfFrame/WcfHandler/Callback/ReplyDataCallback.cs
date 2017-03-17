using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EFWCoreLib.WcfFrame.DataSerialize;
using EFWCoreLib.WcfFrame.SDMessageHeader;
using EFWCoreLib.WcfFrame.ServerManage;
using EFWCoreLib.WcfFrame.Utility;

namespace EFWCoreLib.WcfFrame.WcfHandler
{
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    public class ReplyDataCallback : IDataReply
    {
        private ClientLink clientLink;
        public ReplyDataCallback(ClientLink _clientLink)
        {
            clientLink = _clientLink;
        }

        public string ReplyProcessRequest(HeaderParameter para, string plugin, string controller, string method, string jsondata)
        {
            return DataManage.ReplyProcessRequest(plugin, controller, method, jsondata, para);
        }

        public void Notify(string publishServiceName)
        {
            SubscriberManager.ReceiveNotify(publishServiceName, clientLink);
        }

        public string CreateDataClient()
        {
            ClientLink cl = SuperClient.CreateDataClient();
            while (cl.ClientObj.ClientID == null)//解决并发问题，因为创建连接是一个异步过程,等创建成功后再返回ClientID
            {
                Thread.Sleep(400);
            }

            return cl.ClientObj.ClientID;
        }

        public string ReplyRemoteCommand(string eprocess, string method, string arg, MNodePath NodePath)
        {
            return DataManage.ReplyRemoteCommand(eprocess, method, arg, NodePath);
        }
    }
}
