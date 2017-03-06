using System;
using EFWCoreLib.WcfFrame;
using EFWCoreLib.WcfFrame.ServerManage;
using Newtonsoft.Json;

namespace EFWCoreLib.CoreFrame.SSO
{
    /// <summary>
    /// 单点登录辅助类
    /// 登录令牌由根节点生成，然后同步到各子节点
    /// 令牌在子节点验证的时候，如果子节点没有此令牌则马上向根节点拉取所有令牌，如果拉取后还是没有，则验证失败
    /// 
    /// </summary>
    public class SsoHelper
    {        

        public static void Start()
        {
            TokenManager.Init();
        }

        /// <summary>
        /// 登录
        /// 由中间件根节点进行登录
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="tokenid"></param>
        /// <returns></returns>
        public static bool SignIn(UserInfo userInfo, out string tokenid)
        {
            if (WcfGlobal.IsRootMNode)
            {
                TokenInfo token = new TokenInfo()
                {
                    tokenId = Guid.NewGuid(),
                    IsValid = true,
                    CreateTime = DateTime.Now,
                    ActivityTime = DateTime.Now,
                    UserId = userInfo.UserId,
                    userinfo = userInfo
                };
                tokenid = token.tokenId.ToString();
                return TokenManager.AddToken(token);
            }
            else
            {
                string val= SuperClient.superClientLink.RootRequest("sso_signin", JsonConvert.SerializeObject(userInfo));
                tokenid = JsonConvert.DeserializeObject<String>(val);
                return true;
            }
        }
        /// <summary>
        /// 注销
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static bool SignOut(string token)
        {
            if (WcfGlobal.IsRootMNode)
            {
                return TokenManager.RemoveToken(token);
            }
            else
            {
                string val = SuperClient.superClientLink.RootRequest("sso_signout", token);
                return JsonConvert.DeserializeObject<Boolean>(val);
            }
        }


        /// <summary>
        /// 定时触发登录码的活动时间，频率必须小于4分钟
        /// </summary>
        /// <param name="token"></param>
        public static void UserActivity(string token)
        {
            if (WcfGlobal.IsRootMNode)
            {
                TokenInfo existToken = TokenManager.GetToken(token);
                existToken.ActivityTime = DateTime.Now;
            }
            else
            {
                SuperClient.superClientLink.RootRequest("sso_useractivity", token);
            }
        }

        /// <summary>
        /// 是否有效登录
        /// 直接验证当前中间件节点
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static AuthResult ValidateToken(string token)
        {
            UserActivity(token);
            return TokenManager.ValidateToken(token);
        }

        //转发数据
        public static string ForwardData(string key, string jsonpara)
        {
            switch (key)
            {
                case "sso_signin":
                    UserInfo userInfo = JsonConvert.DeserializeObject<UserInfo>(jsonpara);
                    string tokenid;
                    SsoHelper.SignIn(userInfo, out tokenid);
                    return JsonConvert.SerializeObject(tokenid);
                case "sso_signout":
                    bool ret= SsoHelper.SignOut(jsonpara);
                    return JsonConvert.SerializeObject(ret);
                case "sso_useractivity":
                    SsoHelper.UserActivity(jsonpara);
                    return "";
            }
            return null;
        }

        /*
        public static AuthResult ValidateUserId(string userCode)
        {
            AuthResult result = new AuthResult() { ErrorMsg = "Token不存在" };
            TokenInfo existToken = TokenManager.GetToken(userCode);

            if (existToken != null)
            {
                #region 客户端IP不一致
                //if (existToken.RemoteIp != entity.RemoteIp)
                //{
                //    result.ErrorMsg = "客户端IP不一致";
                //}
                #endregion

                if (existToken.IsValid == false)
                {
                    result.ErrorMsg = "Token已过期" + existToken.ActivityTime.ToLongTimeString() + ":" + DateTime.Now.ToLocalTime();
                    TokenManager.RemoveToken(existToken.tokenId);//移除
                }
                else
                {
                    result.token = existToken.tokenId.ToString();
                    result.User = existToken.userinfo;
                    result.ErrorMsg = string.Empty;
                }
            }

            return result;
        }
        /// <summary>
        /// 用户是否在线
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static bool IsUserOnline(string userCode)
        {
            TokenInfo existToken = TokenManager.GetToken(userCode);
            if (existToken != null) return true;
            return false;
        }
        */
    }
}
