using EFWCoreLib.CoreFrame.Mongodb;
using EFWCoreLib.WcfFrame.ServerManage;
using EFWCoreLib.WcfFrame.Utility.MonitorPlatform;
using MongoDB.Driver.Builders;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EFWCoreLib.WcfFrame.Utility
{
    public static class AdapterScriptHelper
    {
        public static bool GetAdapterScript(string pluginname, out string scriptText)
        {
            try
            {
                if (WcfGlobal.IsRootMNode)
                {
                    MongoHelper<PluginServiceScript> pshelper = new MongoHelper<PluginServiceScript>(WcfGlobal.MongoConnStr, MonitorPlatformManage.dbName);
                    PluginServiceScript psScript = pshelper.Find(Query.EQ("pluginname", pluginname));
                    scriptText = psScript.script;
                    return true;
                }
                else
                {
                    string val = SuperClient.CreateDataClient().RootRequest("adapter_getscript", JsonConvert.SerializeObject(pluginname));
                    scriptText = JsonConvert.DeserializeObject<String>(val);
                    return true;
                }
            }
            catch(Exception err)
            {
                CoreFrame.Common.MiddlewareLogHelper.WriterLog(err.Message+err.StackTrace);
                scriptText = null;
                return false;
            }
        }

        //转发数据
        public static string ForwardData(string key, string jsonpara)
        {
            switch (key)
            {
                case "adapter_getscript":
                    string pluginname = JsonConvert.DeserializeObject<String>(jsonpara);
                    string scriptText;
                    AdapterScriptHelper.GetAdapterScript(pluginname, out scriptText);
                    return JsonConvert.SerializeObject(scriptText);
            }
            return null;
        }
    }
}
