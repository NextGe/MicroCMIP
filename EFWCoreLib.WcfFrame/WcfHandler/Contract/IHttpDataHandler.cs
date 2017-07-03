using EFWCoreLib.CoreFrame.Business;
using EFWCoreLib.WcfFrame.SDMessageHeader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;

namespace EFWCoreLib.WcfFrame.WcfHandler
{

    /// <summary>
    /// 处理Http请求
    /// </summary>
    [ServiceKnownType(typeof(DBNull))]
    [ServiceContract(Namespace = "http://www.efwplus.cn/", Name = "HttpService", SessionMode = SessionMode.NotAllowed)]
    public interface IHttpDataHandler
    {
        /// <summary>
        /// 执行请求
        /// </summary>
        /// <returns></returns>
        [OperationContract(IsOneWay = false)]
        [WebInvoke(Method = "POST",
           //RequestFormat = WebMessageFormat.Json,
           //ResponseFormat = WebMessageFormat.Json,
           UriTemplate = "/"
         )]
        string ProcessHttpRequest(RequestArgs requestArgs);
    }

    [DataContract(Namespace = "http://www.efwplus.cn/")]
    public class RequestArgs
    {
        [DataMember]
        public SysLoginRight sysright { get; set; }
        [DataMember]
        public string plugin { get; set; }
        [DataMember]
        public string controller { get; set; }
        [DataMember]
        public string method { get; set; }
        [DataMember]
        public string jsondata { get; set; }

        public override string ToString()
        {
            return string.Format("令牌={0} 插件名={1} 控制器={2} 方法={3}", sysright.token, plugin, controller, method);
        }
    }
}
