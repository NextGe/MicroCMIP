using EFWCoreLib.CoreFrame.Business.AttributeInfo;
using EFWCoreLib.CoreFrame.Common;
using EFWCoreLib.WcfFrame.DataSerialize;
using EFWCoreLib.WcfFrame.ServerController;
using Microsoft.Scripting.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdapterService.Python.WcfController
{
    [WCFController]
    public class AdapterPythonController : WcfServerController
    {
        [WCFMethod]
        public ServiceResponseData CallAPI()
        {
            string servicecode = requestData.GetData<String>(0);
            string jsonpara = requestData.GetData<String>(1);
            string scriptText;
            MiddlewareLogHelper.WriterLog(LogType.PythonLog, "开始执行Python脚本,业务代码:" + servicecode);
            bool result = EFWCoreLib.WcfFrame.Utility.AdapterScriptHelper.GetAdapterScript(GetPluginName(), out scriptText);
            if (result)
            {
                ScriptEngine engine = IronPython.Hosting.Python.CreateEngine();
                engine.SetSearchPaths(new string[] { EFWCoreLib.CoreFrame.Init.AppGlobal.AppRootPath + "PythonLib" });
                ScriptScope scope = engine.CreateScope();

                //ScriptSource script = engine.CreateScriptSourceFromFile(@"test01.py", Encoding.UTF8);

                //"print 'hello kakake'"
                //ScriptSource script = engine.CreateScriptSourceFromString(@"
                //__author__ = 'kakake'

                //import sys

                //def main(args):
                //    try:
                //		print 'Succeed!!!'
                //		return 'hello %s' %args
                //    except Exception,ex:
                //		print 'Error!!!',Exception,':',ex;
                //		return -1;
                //#main('')
                //");
                scope.SetVariable("adapter", this);
                ScriptSource script = engine.CreateScriptSourceFromString(scriptText);
                script.Execute(scope);
                
                var fun_main = scope.GetVariable<Func<object, object, object>>("main");
                object funResult = fun_main(servicecode, jsonpara);
                if (funResult == null || funResult.ToString() == "-1")
                {
                    throw new Exception("Python脚本执行错误");
                }
                responseData.AddData(funResult.ToString());
            }
            MiddlewareLogHelper.WriterLog(LogType.PythonLog, "Python脚本执行完成,业务代码:" + servicecode);
            return responseData;
        }

        public void printlog(string msg)
        {
            MiddlewareLogHelper.WriterLog(LogType.PythonLog, "Python脚本调试日志:" + msg);
        }
    }
}
