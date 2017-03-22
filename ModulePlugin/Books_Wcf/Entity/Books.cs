using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EFWCoreLib.CoreFrame.Orm;
using EFWCoreLib.CoreFrame.Business;
using ProtoBuf;

namespace Books_Wcf.Entity
{
    [Table(TableName = "Books", EntityType = EntityType.Table, IsGB = false)]
    public class Books:AbstractEntity
    {
        private int  _id;
        /// <summary>
        /// 
        /// </summary>
        [Column(FieldName = "Id", DataKey = true, Match = "", IsInsert = false)]
        public int Id
        {
            get { return  _id; }
            set {  _id = value; }
        }

        private string  _bookname;
        /// <summary>
        /// 
        /// </summary>
        [Column(FieldName = "BookName", DataKey = false, Match = "", IsInsert = true)]
        public string BookName
        {
            get { return  _bookname; }
            set {  _bookname = value; }
        }

        private Decimal  _buyprice;
        /// <summary>
        /// 
        /// </summary>
        [Column(FieldName = "BuyPrice", DataKey = false, Match = "", IsInsert = true)]
        public Decimal BuyPrice
        {
            get { return  _buyprice; }
            set {  _buyprice = value; }
        }

        private DateTime  _buydate;
        /// <summary>
        /// 
        /// </summary>
        [Column(FieldName = "BuyDate", DataKey = false, Match = "", IsInsert = true)]
        public DateTime BuyDate
        {
            get { return  _buydate; }
            set {  _buydate = value; }
        }

        private int  _flag;
        /// <summary>
        /// 
        /// </summary>
        [Column(FieldName = "Flag", DataKey = false, Match = "", IsInsert = true)]
        public int Flag
        {
            get { return  _flag; }
            set {  _flag = value; }
        }
    }
}
