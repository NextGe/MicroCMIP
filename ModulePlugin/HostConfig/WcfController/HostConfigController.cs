using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EFWCoreLib.CoreFrame.Business.AttributeInfo;
using EFWCoreLib.CoreFrame.Init;
using EFWCoreLib.WcfFrame.DataSerialize;
using EFWCoreLib.WcfFrame.ServerController;

namespace HostConfig.WcfController
{
    [WCFController]
    public class HostConfigController : WcfServerController
    {
        [WCFMethod]
        public ServiceResponseData GetConfigSubject()
        {
            List<HostRunConfigSubject> subjectList = HostRunConfigInfo.GetConfigSubject();
            responseData.AddData(subjectList);
            return responseData;
        }
    }
}
