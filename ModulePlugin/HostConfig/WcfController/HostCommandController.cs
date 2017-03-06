using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EFWCoreLib.CoreFrame.Business.AttributeInfo;
using EFWCoreLib.CoreFrame.Common;
using EFWCoreLib.WcfFrame;
using EFWCoreLib.WcfFrame.DataSerialize;
using EFWCoreLib.WcfFrame.ServerController;
using EFWCoreLib.WebApiFrame;

namespace HostConfig.WcfController
{
    [WCFController]
    public class HostCommandController : WcfServerController
    {
        [WCFMethod]
        public ServiceResponseData StopHost()
        {
            MiddlewareLogHelper.WriterLog("正在准备关闭中间件服务，请等待...");

            WcfGlobal.Exit();
            WebApiGlobal.Exit();

            responseData.AddData(true);
            return responseData;
        }
    }
}
