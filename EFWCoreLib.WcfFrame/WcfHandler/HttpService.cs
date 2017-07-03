using EFWCoreLib.CoreFrame.Common;
using EFWCoreLib.WcfFrame.DataSerialize;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;

namespace EFWCoreLib.WcfFrame.WcfHandler
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false, ValidateMustUnderstand = false, IncludeExceptionDetailInFaults = false)]
    public class HttpService : IHttpDataHandler
    {
        public string ProcessHttpRequest(RequestArgs requestArgs)
        {
            MiddlewareLogHelper.WriterLog(LogType.MidLog, true, Color.Blue, "接受Http请求:" + requestArgs.ToString());
            //获取的池子索引
            int? index = null;
            ClientLink clientlink = null;
            ClientLinkPool pool = fromPoolGetClientLink(requestArgs.plugin, out clientlink, out index);
            ServiceResponseData retData = new ServiceResponseData();

            try
            {
                //绑定LoginRight
                Action<ClientRequestData> _requestAction = ((ClientRequestData request) =>
                {
                    request.LoginRight = requestArgs.sysright;
                    request.SetJsonData(requestArgs.jsondata);
                });
                retData = clientlink.Request(requestArgs.controller, requestArgs.method, _requestAction);
            }
            catch (Exception ex) { throw ex; }
            finally
            {
                if (index != null)
                    pool.ReturnPool(requestArgs.plugin, (int)index);
            }
            return retData.GetJsonData();
        }

        private ClientLinkPool fromPoolGetClientLink(string wcfpluginname, out ClientLink clientlink, out int? index)
        {
            ClientLinkPool pool = ClientLinkPoolCache.GetClientPool("httpserver");
            //获取的池子索引
            index = null;
            clientlink = null;
            //是否超时
            bool isouttime = false;
            //超时计时器
            Stopwatch sw = new Stopwatch();
            sw.Start();
            while (true)
            {
                bool isReap = true;
                //先判断池子中是否有此空闲连接
                if (pool.GetFreePoolNums(wcfpluginname) > 0)
                {
                    isReap = false;
                    clientlink = pool.GetClientLink(wcfpluginname);
                    if (clientlink != null)
                    {
                        index = clientlink.Index;
                    }
                }
                //如果没有空闲连接判断是否池子是否已满，未满，则创建新连接并装入连接池
                if (clientlink == null && !pool.IsPoolFull)
                {
                    //装入连接池
                    bool flag = pool.AddPool(wcfpluginname, "localendpoint", out clientlink, out index);
                }

                //如果当前契约无空闲连接，并且队列已满，并且非当前契约有空闲，则踢掉一个非当前契约
                if (clientlink == null && pool.IsPoolFull && pool.GetFreePoolNums(wcfpluginname) == 0 && pool.GetUsedPoolNums(wcfpluginname) != 500)
                {
                    //创建新连接
                    pool.RemovePoolOneNotAt(wcfpluginname, "localendpoint", out clientlink, out index);
                }

                if (clientlink != null)
                    break;

                //如果还未获取连接判断是否超时30秒，如果超时抛异常
                if (sw.Elapsed >= new TimeSpan(30 * 1000 * 10000))
                {
                    isouttime = true;
                    break;
                }
                else
                {
                    Thread.Sleep(100);
                }
            }
            sw.Stop();
            sw = null;

            if (isouttime)
            {
                throw new Exception("获取连接池中的连接超时");
            }

            return pool;
        }
    }
}
