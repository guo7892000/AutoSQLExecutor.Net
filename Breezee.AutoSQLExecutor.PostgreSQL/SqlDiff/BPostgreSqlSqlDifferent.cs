using Breezee.AutoSQLExecutor.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Breezee.AutoSQLExecutor.PostgreSQL
{
    public class BPostgreSqlSqlDifferent : ISqlDifferent
    {
        public override string Combine
        {
            get { return "||"; }
        }
        public override string ParamPrefix
        {
            get { return ":"; }
        }
        /// <summary>
        /// 当前日期字符
        /// </summary>
        public override string Now { get { return "now()"; } }
        /// <summary>
        /// 提交字符
        /// </summary>
        public override string Commit { get { return String.Empty; } }
        /// <summary>
        /// 自增长字符
        /// </summary>
        public override string AutoNum { get { return "AUTO_INCREMENT"; } }

        public override string GetDropFunctionSql(string funcName)
        {
            return string.Format(@"DROP FUNCTION IF EXISTS {0};\n", funcName);
        }

        public override string GetDropProcedureSql(string procName)
        {
            return string.Format(@"DROP PROCEDURE IF EXISTS {0};\n", procName);
        }

        public override string GetDropTableSql(string tableName)
        {
            return string.Format(@"DROP TABLE IF EXISTS {0};\n", tableName);
        }

        public override string GetDropViewSql(string viewName)
        {
            return string.Format(@"DROP VIEW IF EXISTS {0};\n", viewName);
        }
    }
}
