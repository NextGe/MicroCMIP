using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EFWCoreLib.CoreFrame.Common;
using EFWCoreLib.CoreFrame.Mongodb;
using MongoDB.Driver.Builders;

namespace EFWCoreLib.WcfFrame.Utility.MonitorPlatform
{
    /// <summary>
    /// 监控平台管理
    /// </summary>
    public class MonitorPlatformManage
    {
        public static string dbName = "MonitorPlatform";
        /// <summary>
        /// 监控平台初始化
        /// </summary>
        public static void Init()
        {
            //

            //1.初始化超级用户
            User user;
            MongoHelper<User> helperUser = new MongoHelper<User>(WcfGlobal.MongoConnStr, MonitorPlatformManage.dbName);
            user= helperUser.Find(Query.EQ("usercode", new MongoDB.Bson.BsonString("admin")));
            if (user != null)
            {
                //如果存在先删除
                helperUser.Delete(user);
            }
            user = new User();
            user.usercode = "admin";
            user.pwd = DESEncryptor.DesEncrypt("123456");
            user.email = "343588387@qq.com";
            user.username = "卡卡棵";
            helperUser.Insert(user);
            //2.初始化中间件根节点
            MongoHelper<MidNode> helperNode = new MongoHelper<MidNode>(WcfGlobal.MongoConnStr, MonitorPlatformManage.dbName);
            MidNode node;
            node = helperNode.Find(Query.EQ("identify", new MongoDB.Bson.BsonString(WcfGlobal.Identify)));
            if (node != null)
            {
                //如果存在先删除
                helperNode.Delete(node);
            }
            node = new MidNode();
            node.nodename = "根节点";
            node.machinecode = "";
            node.regcode = CoreFrame.Init.HostSettingConfig.GetValue("cdkey");
            node.identify = WcfGlobal.Identify;
            node.createdate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            node.delflag = 0;
            helperNode.Insert(node);

        }
    }
}
