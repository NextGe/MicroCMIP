using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EFWCoreLib.CoreFrame.Common;
using EFWCoreLib.CoreFrame.ProcessManage;
using EFWCoreLib.WcfFrame;
using Newtonsoft.Json;

namespace efwplusBase
{
    static class Program
    {
        static NormalIPCManager normalIPC;
        /// <summary>
        /// 启动状态
        /// </summary>
        static HostState RunState { get; set; }

        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        static void Main(string[] args)
        {
            try
            {
                setprivatepath();
                RunState = HostState.NoOpen;

                Func<string, Dictionary<string,string>, string> _funcExecCmd = ExecuteCmd;
                Action<string> _actionReceiveData = ((string data) =>
                {
                    string text = ("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] : " + data);
                    Console.WriteLine(text);
                });
                normalIPC=new NormalIPCManager(IPCType.efwplusBase,_funcExecCmd, _actionReceiveData);

                btnStart();
                while (true)
                {
                    System.Threading.Thread.Sleep(30 * 1000);
                }
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
            }
        }

        static void btnStart()
        {
            string identify;
            string expireDate;
            MiddlewareLogHelper.hostwcfMsg = new MiddlewareMsgHandler(ShowMsg);
            MiddlewareLogHelper.StartWriteFileLog();//开放中间件日志
            int res = efwplus.configuration.TimeCDKEY.InitRegedit(out expireDate, out identify);
            if (res == 0)
            {
                MiddlewareLogHelper.WriterLog("软件已注册，到期时间【" + expireDate + "】");
                //WcfGlobal.Identify = identify;
                //EFWCoreLib.WcfFrame.ServerManage.ClientManage.clientinfoList = new ClientInfoListHandler(BindGridClient);
                //EFWCoreLib.WcfFrame.ServerManage.RouterManage.hostwcfRouter = new HostWCFRouterListHandler(BindGridRouter);
                WcfGlobal.MainBase(identify);

                RunState = HostState.Opened;
            }
            else if (res == 1)
            {
                MiddlewareLogHelper.WriterLog("软件尚未注册，请注册软件");
            }
            else if (res == 2)
            {
                MiddlewareLogHelper.WriterLog("注册机器与本机不一致,请联系管理员");
            }
            else if (res == 3)
            {
                MiddlewareLogHelper.WriterLog("软件试用已到期");
            }
            else
            {
                MiddlewareLogHelper.WriterLog("软件运行出错，请重新启动");
            }
        }

        static void btnStop()
        {
            WcfGlobal.ExitBase();

            RunState = HostState.NoOpen;
        }

        static void ShowMsg(Color clr, DateTime time, string msg)
        {
            normalIPC.ShowMsg(msg);
        }

        /// <summary>
        /// 获取或设置应用程序基目录下的目录列表
        /// </summary>
        static void setprivatepath()
        {
            //AppDomain.CurrentDomain.SetupInformation.PrivateBinPath = @"Component;ModulePlugin\Books_Wcf\dll;ModulePlugin\WcfMainUIFrame\dll";
            string privatepath = @"Component";

            foreach (var p in efwplus.configuration.PluginSysManage.GetAllPlugin())
            {
                privatepath += ";" + p.path.Replace("plugin.xml", "dll");
            }

            AppDomain.CurrentDomain.SetData("PRIVATE_BINPATH", privatepath);
            AppDomain.CurrentDomain.SetData("BINPATH_PROBE_ONLY", privatepath);
            var m = typeof(AppDomainSetup).GetMethod("UpdateContextProperty", BindingFlags.NonPublic | BindingFlags.Static);
            var funsion = typeof(AppDomain).GetMethod("GetFusionContext", BindingFlags.NonPublic | BindingFlags.Instance);
            m.Invoke(null, new object[] { funsion.Invoke(AppDomain.CurrentDomain, null), "PRIVATE_BINPATH", privatepath });

        }

        static string ExecuteCmd(string m, Dictionary<string,string> a)
        {
            string retData = "succeed";
            try
            {
                switch (m)
                {
                    case "stop":
                        WcfGlobal.ExitBase();
                        break;
                    case "start":
                        WcfGlobal.MainBase(WcfGlobal.Identify);
                        break;
                    case "close":
                        Environment.Exit(0);
                        break;
                    case "getmnodeconfig"://获取节点配置对象
                        retData = EFWCoreLib.CoreFrame.Init.HostRunConfigInfo.GetConfigSubject();
                        break;
                    case "getmnodetext"://获取节点配置文本信息
                        retData = EFWCoreLib.CoreFrame.Init.HostRunConfigInfo.GetConfigText();
                        break;
                    case "initmonitor"://初始化监控平台的默认登录用户
                        EFWCoreLib.WcfFrame.Utility.MonitorPlatform.MonitorPlatformManage.Init();
                        break;
                    case "ssosignin"://单点登录
                        EFWCoreLib.CoreFrame.SSO.UserInfo userInfo = new EFWCoreLib.CoreFrame.SSO.UserInfo();
                        userInfo.UserCode = a["usercode"];
                        userInfo.EmpName = a["username"];
                        string token;
                        EFWCoreLib.CoreFrame.SSO.SsoHelper.SignIn(userInfo, out token);
                        retData = token;
                        break;
                    case "ssosignout"://单点登录退出
                        EFWCoreLib.CoreFrame.SSO.SsoHelper.SignOut(a["token"]);
                        break;
                    case "ssovalidatetoken"://单点登录验证
                        EFWCoreLib.CoreFrame.SSO.AuthResult ar = EFWCoreLib.CoreFrame.SSO.SsoHelper.ValidateToken(a["token"]);
                        retData = JsonConvert.SerializeObject(ar);
                        break;
                    case "clientlist"://获取节点客户端列表
                        retData = JsonConvert.SerializeObject(EFWCoreLib.WcfFrame.ServerManage.ClientManage.ClientDic.Values.ToList());
                        break;
                    case "sevicelist"://本地插件
                        retData = JsonConvert.SerializeObject(EFWCoreLib.WcfFrame.ServerManage.RemotePluginManage.GetLocalPlugin());
                        break;
                    case "debuglog":
                        retData = MiddlewareLogHelper.ReadFile(a["logtype"], a["date"]);
                        break;
                }
                ShowMsg(Color.Black, DateTime.Now, "efwplusBase命令执行完成：" + m);
                retData = retData.Substring(0, retData.Length > 5000 ? 5000 : retData.Length);
                return retData;
            }
            catch (Exception e)
            {
                retData = e.Message;
                return retData;
            }
        }
    }

    public enum HostState
    {
        NoOpen, Opened
    }
}
