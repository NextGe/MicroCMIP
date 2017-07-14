using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using EFWCoreLib.WcfFrame.DataSerialize;
using EFWCoreLib.WcfFrame.ServerManage;

namespace EFWCoreLib.CoreFrame.SSO
{
    /// <summary>
    /// 身份验证令牌管理
    /// </summary>
    public class TokenManager
    {
        private const int _TimerPeriod = 60000*20;//20分钟
        private static Timer thTimer;
        private static List<TokenInfo> tokenList = null;//令牌集合
        private static Object syncObj = new Object();//定义一个静态对象用于线程部份代码块的锁定，用于lock操作

        //private static Action<string, string, string> AddCache;//增加分布式缓存
        //private static Action<string, string> RemoveCache;//移除分布式缓存

        public static void Init()
        {

            //AddCache = _addCache;
            //RemoveCache = _removeCache;
            tokenList = new List<TokenInfo>();
            //60秒失效
            thTimer = new Timer(threadTimerCallback, null, _TimerPeriod, _TimerPeriod);
        }

        public static bool AddToken(TokenInfo entity)
        {
            lock (syncObj)
            {
                tokenList.Add(entity);
                DistributedCacheManage.SetCache("tokenList", entity.tokenId.ToString(), JsonConvert.SerializeObject(entity.userinfo));
                //if (AddCache != null)
                //{
                //    AddCache("tokenList", entity.tokenId.ToString(), JsonConvert.SerializeObject(entity.userinfo));
                //}
            }
            return true;
        }



        public static bool RemoveToken(string token)
        {
            TokenInfo tinfo = GetToken(token);
            if (tinfo != null)
            {
                lock (syncObj)
                {
                    tokenList.Remove(tinfo);
                    //if (RemoveCache != null)
                    //{
                    //    RemoveCache("tokenList", tinfo.tokenId.ToString());
                    //}
                    DistributedCacheManage.RemoveCache("tokenList", tinfo.tokenId.ToString());
                }
            }
            return true;
        }

        public static AuthResult ValidateToken(string token)
        {
            AuthResult result = new AuthResult() { ErrorMsg = "Token不存在或已过期" };
            CacheObject co= DistributedCacheManage.GetLocalCache("tokenList");
            if (co != null)
            {
                CacheData cd= co.cacheValue.Find(x => x.key == token && x.deleteflag == false);
                if (cd != null)
                {
                    result.token = token;
                    result.User = JsonConvert.DeserializeObject<UserInfo>(cd.value);
                    result.ErrorMsg = string.Empty;
                }
            }

            return result;
        }


        public static TokenInfo GetToken(string token)
        {
            TokenInfo existToken = null;
            try
            {
                existToken = tokenList.Find(x => x.tokenId.ToString() == token);
            }
            catch (Exception err){
                CoreFrame.Common.MiddlewareLogHelper.WriterLog(err.Message+err.StackTrace);
            }
            return existToken;
        }

        private static void threadTimerCallback(Object state)
        {
            try
            {
                DateTime now = DateTime.Now;

                foreach (TokenInfo t in tokenList)
                {
                    if (((TimeSpan)(now - t.ActivityTime)).TotalMilliseconds > _TimerPeriod)
                    {
                        RemoveToken(t.tokenId.ToString());
                    }
                }
            }
            catch (Exception err){ CoreFrame.Common.MiddlewareLogHelper.WriterLog(err.Message+err.StackTrace); }
        }
    }
}
