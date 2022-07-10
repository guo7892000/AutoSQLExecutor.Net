using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Breezee.AutoSQLExecutor.Core
{
    /// <summary>
    /// SQL参数类
    /// </summary>
    public class SqlParam
    {
        public SqlParam(string name, DbType dbType, object value)
        {
            this.Name = name;
            this.DataType = dbType;
            this.Value = value;
        }

        public string Name { get; private set; }

        public DbType DataType { get; private set; }

        public object Value { get; private set; }

        

    }

    public enum SqlParamType
    {
        String,
        DateTime,
        Int
    }
}
