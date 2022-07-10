using Breezee.AutoSQLExecutor.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Breezee.AutoSQLExecutor.SQLite
{
    /// <summary>
    /// 类说明：SQLite数据库的SQL差异
    /// 备注：SQL支持表、视图、触发器。但不支持函数、存储过程。
    /// </summary>
    public class BSQLiteSqlDifferent : ISqlDifferent
    {
        public override string Combine { get { return "||"; } }
        public override string ParamPrefix { get { return "@"; } }
        /// <summary>
        /// 当前日期字符
        /// </summary>
        public override string Now { get { return "(datetime('now','localtime'))"; } }
        /// <summary>
        /// 提交字符
        /// </summary>
        public override string Commit { get { return String.Empty; } }
        /// <summary>
        /// 自增长字符
        /// </summary>
        public override string AutoNum { get { return "AUTOINCREMENT"; } }

        public override string GetDropFunctionSql(string funcName)
        {
            //return string.Format(@"DROP FUNCTION IF EXISTS {0}; \n", funcName);
            throw new NotSupportedException();
        }

        public override string GetDropProcedureSql(string procName)
        {
            //return string.Format(@"DROP PROCEDURE IF EXISTS {0}; \n", procName);
            throw new NotSupportedException();
        }

        public override string GetDropTableSql(string tableName)
        {
            return string.Format(@"DROP TABLE IF EXISTS {0}; \n", tableName);
        }
        public override string GetDropViewSql(string viewName)
        {
            return string.Format(@"DROP VIEW IF EXISTS {0}; \n", viewName);
        }
    }
}
