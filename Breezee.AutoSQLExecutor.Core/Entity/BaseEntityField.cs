using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Breezee.AutoSQLExecutor.Core
{
    /// <summary>
    /// 实体字段基类
    /// 对应于表或视图中的一个列。
    /// </summary>
    public class BaseEntityField
    {
        /// <summary>
        /// 生成一个实例
        /// </summary>
        /// <returns></returns>
        public static BaseEntityField New()
        {
            return new BaseEntityField();
        }

        /// <summary>
        /// 字段类型：数据库列（默认）和其他
        /// </summary>
        public EntityFieldType Field_Type { get; private set; } = EntityFieldType.DbColumn;
        public BaseEntityField FieldType(EntityFieldType sValue= EntityFieldType.DbColumn)
        {
            Field_Type = sValue;
            return this;
        }

        #region 数据库相关
        /// <summary>
        /// 数据库列名
        /// </summary>
        public string DbColumnName { get; private set; }
        public BaseEntityField DBColName(string sValue)
        {
            DbColumnName = sValue;
            return this;
        }

        /// <summary>
        /// 数据库列类型字符
        /// </summary>
        public string DbColumnType { get; private set; }
        public BaseEntityField DBColType(string sValue)
        {
            DbColumnType = sValue;
            return this;
        }

        /// <summary>
        /// 类型和大小
        /// </summary>
        public string DbColumnTypeSize { get; private set; }
        public BaseEntityField DBTypeSize(string sValue)
        {
            DbColumnTypeSize = sValue;
            return this;
        }

        /// <summary>
        /// 对应.Net的数据类型
        /// </summary>
        public DbType DbType { get; private set; }
        public BaseEntityField NetType(DbType sValue)
        {
            DbType = sValue;
            return this;
        }

        /// <summary>
        /// 长度
        /// </summary>
        public int DbLength { get; private set; }
        public BaseEntityField DBLen(int sValue)
        {
            DbLength = sValue;
            return this;
        }

        /// <summary>
        /// 精度：默认为-1
        /// </summary>
        public int DBNumberPrecision { get; private set; } = -1;
        public BaseEntityField DBPrecision(int sValue)
        {
            DBNumberPrecision = sValue;
            return this;
        }

        /// <summary>
        /// 小数位数：默认为-1
        /// </summary>
        public int DBNumberScale { get; private set; } = -1;
        public BaseEntityField DBScale(int sValue)
        {
            DBNumberScale = sValue;
            return this;
        }

        /// <summary>
        /// 是否可空（默认true可空）
        /// </summary>
        public bool DbIsNullable { get; private set; } = true;
        public BaseEntityField Nullable(bool sValue= true)
        {
            DbIsNullable = sValue;
            DbIsNotNull = !sValue;
            return this;
        }

        /// <summary>
        /// 是否非空（默认false可空）
        /// </summary>
        public bool DbIsNotNull { get; private set; } = false;
        public BaseEntityField NotNull(bool sValue = true)
        {
            DbIsNotNull = sValue;
            DbIsNullable = !sValue;
            return this;
        }


        /// <summary>
        /// 是否主键（默认否）
        /// </summary>
        public bool DbIsPK { get; private set; } = false;
        public BaseEntityField PK(bool sValue=true)
        {
            DbIsPK = sValue;
            return this;
        }

        /// <summary>
        /// 是否并发更新ID字段（默认否）
        /// </summary>
        public bool IsUpdateControlColumn { get; private set; } = false;
        public BaseEntityField UpdateControl(bool sValue=true)
        {
            IsUpdateControlColumn = sValue;
            return this;
        }

        /// <summary>
        /// 数据库中的备注信息
        /// </summary>
        public string DbComment { get; protected set; }
        public BaseEntityField DBComment(string sValue)
        {
            DbComment = sValue;
            return this;
        }

        /// <summary>
        /// 数据库中的默认值字符
        /// </summary>
        public string DbDefault { get; protected set; }
        public BaseEntityField DBDefault(string sValue)
        {
            DbDefault = sValue;
            return this;
        }
        #endregion

        #region 验证相关
        /// <summary>
        /// 字段名称
        /// </summary>
        public string Field_Name { get; protected set; }
        public BaseEntityField FieldName(string sValue)
        {
            Field_Name = sValue;
            return this;
        }

        /// <summary>
        /// 错误消息
        /// </summary>
        public string ErrorMessage;

        #endregion

        #region 使用相关
        /// <summary>
        /// 是否更新
        /// </summary>
        public bool IsUpdate { get; set; }

        /// <summary>
        /// 默认值类型
        /// </summary>
        public TableCoulnmDefaultType DefaultValueType { get; set; }
        public BaseEntityField DefaultType(TableCoulnmDefaultType sValue)
        {
            DefaultValueType = sValue;
            return this;
        }

        /// <summary>
        /// 对象值
        /// </summary>
        public object ObjValue { get; set; }


        public IDictionary<string,Object> DicObject { get; set; }
        #endregion
    }

    



}
