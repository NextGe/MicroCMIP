using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using EFWCoreLib.CoreFrame.Common;
using EFWCoreLib.CoreFrame.Init;
using EFWCoreLib.WcfFrame.DataSerialize;
using EFWCoreLib.WcfFrame.WcfHandler;
using Microsoft.Practices.EnterpriseLibrary.Caching;

namespace EFWCoreLib.WcfFrame.ServerManage
{
    /// <summary>
    /// 分布式缓存服务端管理
    /// 上级缓存变更通知下级获取最新的缓存并同步到本地
    /// </summary>
    public class DistributedCacheManage
    {
        //使用企业库中的缓存对象来进行分布式缓存管理
        public static ICacheManager LocalCache
        {
            get { return AppGlobal.cache; }
        }

        public static List<string> CacheNameList;//缓存列表

        public static Action CacheChangeEvent;//缓存改变触发事件

        /// <summary>
        /// 开始缓存
        /// </summary>
        public static void Start()
        {
            CacheNameList = new List<string>();//缓存列表

            string publishServiceName= "DistributedCache";
            //将分布式缓存作为服务添加到发布订阅服务列表
            PublishServiceObject pso = new PublishServiceObject();
            pso.publishServiceName = publishServiceName;
            pso.explain = "分布式缓存";
            PublisherManage.AddPublishService(pso);
            //触发通知
            DistributedCacheManage.CacheChangeEvent = (()=>
            {
                PublisherManage.SendNotify(publishServiceName);//订阅服务发送通知
            });
        }
        /// <summary>
        /// 停止分布式缓存
        /// </summary>
        public static void Stop()
        {
            foreach (string name in CacheNameList)
            {
                LocalCache.Remove(name);
            }
        }
        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <param name="cacheName"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void SetCache(string cacheName, string key, string value)
        {
            SetCache(cacheName, key, value, true);
        }

        /// <summary>
        /// 设置缓存,提供给服务控制器调用
        /// 单个值的设置
        /// </summary>
        /// <param name="cacheName"></param>
        /// <param name="key"></param>
        /// <param name="value">Json字符串</param>
        public static void SetCache(string cacheName, string key, string value,bool IsTrigger)
        {
            lock (LocalCache)
            {
                if (LocalCache.Contains(cacheName))
                {
                    CacheObject co = LocalCache.GetData(cacheName) as CacheObject;

                    co.identify = DateTimeToTimestamp(DateTime.Now);
                    if (co.cacheValue.FindIndex(x => x.key.Equals(key)) == -1)
                    {
                        CacheData cd = new CacheData();
                        cd.timestamp = DateTimeToTimestamp(DateTime.Now);
                        cd.key = key;
                        cd.value = value;
                        co.cacheValue.Add(cd);
                    }
                    else
                    {
                        CacheData _cd = co.cacheValue.Find(x => x.key.Equals(key));
                        co.cacheValue.Remove(_cd);
                        CacheData cd = new CacheData();
                        cd.timestamp = DateTimeToTimestamp(DateTime.Now);
                        cd.key = key;
                        cd.value = value;
                        co.cacheValue.Add(cd);
                    }
                }
                else
                {
                    CacheObject co = new CacheObject();
                    co.ServerIdentify = WcfGlobal.Identify;
                    co.cachename = cacheName;
                    co.identify = DateTimeToTimestamp(DateTime.Now);
                    co.cacheValue = new List<CacheData>();
                    CacheData cd = new CacheData();
                    cd.timestamp = DateTimeToTimestamp(DateTime.Now);
                    cd.key = key;
                    cd.value = value;
                    co.cacheValue.Add(cd);
                    LocalCache.Add(cacheName, co);
                    if (CacheNameList.Contains(cacheName) == false)
                        CacheNameList.Add(cacheName);
                }

                //PublishServiceManage.SendNotify("DistributedCache");//订阅服务发送通知
                if (IsTrigger && CacheChangeEvent != null)
                {
                    CacheChangeEvent();
                }
            }
        }
        /// <summary>
        /// 批量数据设置
        /// </summary>
        /// <param name="cacheName"></param>
        /// <param name="data"></param>
        public static void SetCache(string cacheName, Dictionary<string, string> adddata, Dictionary<string, string> updatedata, List<string> deldata)
        {
            bool isChanged = false;
            if (adddata != null && adddata.Count > 0)
            {
                foreach (var i in adddata)
                {
                    SetCache(cacheName, i.Key, i.Value, false);
                }
                isChanged = true;
            }
            if (deldata != null && deldata.Count > 0)
            {
                foreach (var i in deldata)
                {
                    RemoveCache(cacheName, i, false);
                }
                isChanged = true;
            }

            if(updatedata!=null && updatedata.Count>0)
            {
                foreach (var i in updatedata)
                {
                    SetCache(cacheName, i.Key, i.Value, false);
                }
                isChanged = true;
            }

            if (CacheChangeEvent != null && isChanged == true)
            {
                CacheChangeEvent();
            }
        }
        /// <summary>
        /// 移除缓存
        /// </summary>
        /// <param name="cacheName"></param>
        /// <param name="key"></param>
        public static void RemoveCache(string cacheName, string key)
        {
            RemoveCache(cacheName, key, true);
        }
        /// <summary>
        /// 移除缓存，提供给服务控制器调用
        /// </summary>
        /// <param name="cacheName"></param>
        /// <param name="key"></param>
        public static void RemoveCache(string cacheName, string key, bool IsTrigger)
        {
            lock (LocalCache)
            {
                if (LocalCache.Contains(cacheName))
                {
                    CacheObject co = LocalCache.GetData(cacheName) as CacheObject;
                    if (co.cacheValue.FindIndex(x => x.key == key) > -1)
                    {
                        CacheData cd = co.cacheValue.Find(x => x.key == key);
                        cd.deleteflag = true;//缓存移除
                        cd.timestamp = DateTimeToTimestamp(DateTime.Now);
                        co.identify = DateTimeToTimestamp(DateTime.Now);

                        //PublishServiceManage.SendNotify("DistributedCache");//订阅服务发送通知
                        if (IsTrigger && CacheChangeEvent != null)
                        {
                            CacheChangeEvent();
                        }
                    }
                }
            }
        }

        public static CacheObject GetLocalCache(string cacheName)
        {
            CacheObject _co = null;
            if (LocalCache.Contains(cacheName))
            {
                _co = LocalCache.GetData(cacheName) as CacheObject;
            }
            return _co;
        }

        public static List<CacheObject> GetCacheObjectList(List<CacheIdentify> ciList)
        {
            List<CacheObject> coList = new List<CacheObject>();
            //有变动的缓存对象
            foreach (var ci in ciList)
            {
                coList.Add(GetStayLocalCache(CompareCache(ci)));
            }
            //新增的缓存对象
            foreach (var _name in CacheNameList)
            {
                if (ciList.FindIndex(x => x.cachename == _name) == -1)
                {
                    coList.Add(GetLocalCache(_name));
                }
            }
            return coList;
        }

       
        /// <summary>
        /// 比较后不同的标识（下级的CacheIdentify与上级的对比）
        /// </summary>
        /// <returns></returns>
        private static CacheIdentify CompareCache(CacheIdentify _cacheId)
        {

            CacheIdentify cacheId = new CacheIdentify();
            cacheId.ServerIdentify = _cacheId.ServerIdentify;
            cacheId.cachename = _cacheId.cachename;
            cacheId.identify = _cacheId.identify;
            cacheId.keytimestamps = new Dictionary<string, double>();
            //自己跟自己比较返回空
            if (_cacheId.ServerIdentify == WcfGlobal.Identify) return cacheId;

            if (LocalCache.Contains(_cacheId.cachename))
            {
                CacheIdentify ci = GetCacheIdentify(_cacheId.cachename);
                if (_cacheId.identify != ci.identify)
                {
                    //循环判断待同步的新增和修改，本地时间搓小于远程时间搓就修改
                    foreach (var t in ci.keytimestamps)
                    {
                        //新增的
                        if (_cacheId.keytimestamps.ContainsKey(t.Key) == false)
                        {
                            cacheId.keytimestamps.Add(t);
                            continue;
                        }
                        //修改的
                        if (_cacheId.keytimestamps[t.Key] < t.Value)
                        {
                            cacheId.keytimestamps.Add(t);
                            continue;
                        }
                    }
                    //循环判断本地的删除，本地时间搓小于远程identify的就会删除
                    //删除是打删除标记，所以不存在移除，都进入修改列表
                }
                return cacheId;
            }
            else
            {
                return cacheId;
            }
        }
        /// <summary>
        /// 获取待同步的缓存
        /// </summary>
        /// <returns></returns>
        private static CacheObject GetStayLocalCache(CacheIdentify identify)
        {
            CacheObject _co = new CacheObject();
            if (LocalCache.Contains(identify.cachename))
            {
                CacheObject co = LocalCache.GetData(identify.cachename) as CacheObject;
                _co.ServerIdentify = co.ServerIdentify;
                _co.cachename = co.cachename;
                _co.identify = co.identify;
                _co.cacheValue = new List<CacheData>();
                foreach (var kt in identify.keytimestamps)
                {
                    CacheData cd = co.cacheValue.Find(x => x.timestamp == kt.Value);
                    if (cd != null)
                        _co.cacheValue.Add(cd);
                }
            }
            return _co;
        }

        /// <summary>
        /// 获取本地缓存的标识
        /// </summary>
        private static CacheIdentify GetCacheIdentify(string cacheName)
        {
            CacheIdentify cacheId = new CacheIdentify();
            if (LocalCache.Contains(cacheName))
            {
                CacheObject co = LocalCache.GetData(cacheName) as CacheObject;
                cacheId.ServerIdentify = co.ServerIdentify;
                cacheId.cachename = co.cachename;
                cacheId.identify = co.identify;
                cacheId.keytimestamps = new Dictionary<string, double>();
                foreach (var cd in co.cacheValue)
                {
                    cacheId.keytimestamps.Add(cd.key, cd.timestamp);
                }
            }
            return cacheId;
        }

        /// <summary>
        /// 日期转换成时间戳
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        private static double DateTimeToTimestamp(DateTime dateTime)
        {
            var start = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToDouble((dateTime - start).TotalMilliseconds);
        }

        /// <summary>
        /// 时间戳转换成日期
        /// </summary>
        /// <param name="timestamp">时间戳（秒）</param>
        /// <returns></returns>
        private static DateTime TimestampToDateTime(double timestamp)
        {
            var start = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return start.AddMilliseconds(timestamp);
        }

        private static void ShowHostMsg(Color clr, DateTime time, string text)
        {
            //hostwcfMsg.BeginInvoke(clr, time, text, null, null);//异步方式不影响后台数据请求
            //hostwcfMsg(time, text);
            MiddlewareLogHelper.WriterLog(LogType.MidLog, true, clr, text);
        }
    }

    /// <summary>
    /// 分布式缓存客户端
    /// </summary>
    public class DistributedCacheClient
    {
        //使用企业库中的缓存对象来进行分布式缓存管理
        //private static ICacheManager _localCache
        //{
        //    get { return AppGlobal.cache; }
        //}
        //private static List<string> _cacheNameList;//缓存列表
        private static string publishServiceName = "DistributedCache";

        /// <summary>
        /// 分布式缓存客户端开启
        /// </summary>
        public static void Start()
        {
            //_cacheNameList = DistributedCacheManage.GetCacheNameList();//本节点的客户端缓存名列表和服务端缓存列表必须一致

            //客户端订阅分布式缓存
            SubscribeServiceObject ssObject = new SubscribeServiceObject();
            ssObject.publishServiceName = publishServiceName;
            ssObject.ProcessService=((ClientLink _clientLink)=>
            {
                List<CacheIdentify> ciList = DistributedCacheClient.GetCacheIdentifyList();//获取本地缓存索引
                List<CacheObject> coList = _clientLink.GetDistributedCacheData(ciList);//比对上级中间件缓存，返回差异的缓存对象
                if (coList.Count > 0)
                {
                    DistributedCacheClient.SetCacheObjectList(coList);//将差异的缓存对象同步到本节点中间件
                }
            });
            SubscriberManager.Subscribe(ssObject);

        }

        public static void Stop()
        {
            //客户端取消订阅分布式缓存
            SubscriberManager.UnSubscribe(publishServiceName);
        }


        public static List<CacheIdentify> GetCacheIdentifyList()
        {
            List<CacheIdentify> ciList = new List<CacheIdentify>();

            foreach (string cn in DistributedCacheManage.CacheNameList)
            {
                ciList.Add(GetCacheIdentify(cn));
            }

            return ciList;
        }


        public static void SetCacheObjectList(List<CacheObject> coList)
        {
            foreach (var co in coList)
            {
                SyncLocalCache(co);
            }
        }


        /// <summary>
        /// 获取本地缓存的标识
        /// </summary>
        private static CacheIdentify GetCacheIdentify(string cacheName)
        {
            CacheIdentify cacheId = new CacheIdentify();
            if (DistributedCacheManage.LocalCache.Contains(cacheName))
            {
                CacheObject co = DistributedCacheManage.LocalCache.GetData(cacheName) as CacheObject;
                cacheId.ServerIdentify = co.ServerIdentify;
                cacheId.cachename = co.cachename;
                cacheId.identify = co.identify;
                cacheId.keytimestamps = new Dictionary<string, double>();
                foreach (var cd in co.cacheValue)
                {
                    cacheId.keytimestamps.Add(cd.key, cd.timestamp);
                }
            }
            return cacheId;
        }

        /// <summary>
        /// 同步本地缓存
        /// </summary>
        /// <param name="cache"></param>
        /// <returns></returns>
        private static bool SyncLocalCache(CacheObject cache)
        {

            lock (DistributedCacheManage.LocalCache)
            {
                bool isChanged = false;
                if (DistributedCacheManage.LocalCache.Contains(cache.cachename))
                {
                    CacheObject co = DistributedCacheManage.LocalCache.GetData(cache.cachename) as CacheObject;
                    if (cache.identify != co.identify)
                    {
                        //循环判断待同步的新增和修改，本地时间搓小于远程时间搓就修改
                        foreach (var t in cache.cacheValue)
                        {
                            //新增的
                            if (co.cacheValue.FindIndex(x => (x.key == t.key)) == -1)
                            {
                                co.cacheValue.Add(t);
                                isChanged = true;
                            }
                            //修改的
                            if (co.cacheValue.FindIndex(x => (x.key == t.key && t.timestamp > x.timestamp)) > -1)
                            {
                                CacheData cd = co.cacheValue.Find(x => x.key == t.key);
                                co.cacheValue.Remove(cd);
                                co.cacheValue.Add(t);
                                isChanged = true;
                            }
                        }
                    }
                }
                else
                {
                    cache.ServerIdentify = WcfGlobal.Identify;
                    DistributedCacheManage.LocalCache.Add(cache.cachename, cache);
                    if (DistributedCacheManage.CacheNameList.Contains(cache.cachename) == false)
                        DistributedCacheManage.CacheNameList.Add(cache.cachename);
                    isChanged = true;
                }

                if (isChanged == true)//同步缓存到下级中间件
                {
                    if (DistributedCacheManage.CacheChangeEvent != null)
                    {
                        DistributedCacheManage.CacheChangeEvent();
                    }
                    if (WcfGlobal.IsDebug)
                        ShowHostMsg(Color.Black, DateTime.Now, String.Format("分布式缓存同步完成，缓存名称：【{0}】，缓存记录：【{1}】", cache.cachename, (DistributedCacheManage.LocalCache.GetData(cache.cachename) as CacheObject).cacheValue.Count));
                }
            }
            return true;
        }


        private static void ShowHostMsg(Color clr, DateTime time, string text)
        {
            //hostwcfMsg.BeginInvoke(clr, time, text, null, null);//异步方式不影响后台数据请求
            //hostwcfMsg(time, text);
            MiddlewareLogHelper.WriterLog(LogType.MidLog, true, clr, text);
        }
    }
}
