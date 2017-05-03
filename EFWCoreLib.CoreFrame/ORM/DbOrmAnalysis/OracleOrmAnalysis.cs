
//==================================================
// 作 者：曾浩
// 日 期：2011/03/06
// 描 述：介绍本文件所要完成的功能以及背景信息等等
//==================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using EFWCoreLib.CoreFrame.Init.AttributeManager;

namespace EFWCoreLib.CoreFrame.Orm
{
    public class OracleOrmAnalysis:OrmAnalysis
    {
        public override string GetInsertSQL(object model)
        {
            string strsql = "";
            try
            {
                Dictionary<string, object> dicsql = new Dictionary<string, object>();

                TableAttributeInfo tableAttribute = GetTableAttributeInfo(model);
                List<ColumnAttributeInfo> columnAttributeCollection = tableAttribute.ColumnAttributeInfoList;

                for (int i = 0; i < columnAttributeCollection.Count; i++)
                {

                    ColumnAttributeInfo columnAttributeInfo = columnAttributeCollection[i];

                    if (columnAttributeInfo.DataKey == true && columnAttributeInfo.Match == "Custom:Guid")//赋值给自增长ID
                    {
                        object obj = GetEntityValue(columnAttributeInfo.PropertyName, model);
                        obj = obj == null ? Guid.NewGuid().ToString() : obj;

                        SetEntityValue(columnAttributeInfo.PropertyName, model, obj);

                        dicsql.Add(columnAttributeInfo.FieldName, obj);
                    }
                    else
                    {

                        if (columnAttributeInfo.IsInsert == true)
                        {
                            object obj = GetEntityValue(columnAttributeInfo.PropertyName, model);
                            obj = obj == null ? "" : obj;
                            dicsql.Add(columnAttributeInfo.FieldName, obj);
                        }
                    }
                }

                string fields = "";
                string values = "";
                strsql = "insert into {0} ({1}) values({2})";

                if (IsJoinWorkId(tableAttribute.IsGB))
                {
                    dicsql.Add("WorkId", Db.WorkId);
                }

                foreach (KeyValuePair<string, object> val in dicsql)
                {
                    fields += (fields == "" ? "" : ",") + val.Key;
                    values += (values == "" ? "" : ",") + ConvertDBValue(val.Value);
                }

                string SQL = string.Format(strsql, tableAttribute.TableName, fields, values);
                //获取自增长ID
                return SQL + ";" + string.Format("select {0}.currval from dual", "S_" + tableAttribute.TableName);
            }
            catch (Exception err)
            {
                throw new Exception(err.Message + "SQL:" + strsql);
            }
        }
    }
}
