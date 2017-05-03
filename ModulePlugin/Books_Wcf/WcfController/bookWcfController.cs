using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EFWCoreLib.CoreFrame.Business.AttributeInfo;
using EFWCoreLib.WcfFrame.ServerController;
using Books_Wcf.Entity;
using System.Data;
using Books_Wcf.Dao;
using EFWCoreLib.WcfFrame.DataSerialize;
using EFWCoreLib.WcfFrame.ServerManage;

namespace Books_Wcf.WcfController
{
    [WCFController]
    public class bookWcfController : WcfServerController
    {
        [WCFMethod]
        public ServiceResponseData SaveBook()
        {
            Books book = requestData.GetData<Books>(0);
            book.BindDb(oleDb, _container,_cache,_pluginName);//反序列化的对象，必须绑定数据库操作对象
            book.save();
            responseData.AddData(true);
            return responseData;
        }

        [WCFMethod]
        public ServiceResponseData GetBooks()
        {
            //DataTable dt = NewDao<IBookDao>().GetBooks("", 0);
            //responseData.AddData(dt);

            DataTable dt = oleDb.GetDataTable(@"select * from BOOKS t");
            responseData.AddData(dt);
            return responseData;
        }
        [WCFMethod]
        public ServiceResponseData Test()
        {
            //测试Oracle数据库
            //1.实体新增
            Books booknew = NewObject<Books>();
            booknew.BookName = "人月神话";
            booknew.BuyPrice = 20.10M;
            booknew.BuyDate = DateTime.Now;
            booknew.Flag = 0;
            booknew.save();
            //2.实体更新
            booknew.Flag = 1;
            booknew.save();
            //3.实体获取
            booknew = NewObject<Books>().getmodel(booknew.Id) as Books;
            //4.实体删除
            booknew.Id = 0;
            booknew.save();
            NewObject<Books>().delete(booknew.Id);
            //5.GetList
            responseData.AddData(NewObject<Books>().getlist<Books>());
            //6.Query<T>
            responseData.AddData(oleDb.Query<Books>("select * from BOOKS t", String.Empty).ToList());
            //7.DataTable
            DataTable dt = oleDb.GetDataTable(@"select * from BOOKS t");
            responseData.AddData(dt);
            return responseData;
        }
    }
}

