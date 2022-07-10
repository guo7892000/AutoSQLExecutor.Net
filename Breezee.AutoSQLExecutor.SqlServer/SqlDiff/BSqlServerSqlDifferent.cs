using Breezee.AutoSQLExecutor.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Breezee.AutoSQLExecutor.SqlServer
{
    public class BSqlServerSqlDifferent : ISqlDifferent
    {
        public override string Combine { get { return "+"; } }
        public override string ParamPrefix { get { return "@"; } }
        /// <summary>
        /// 当前日期字符
        /// </summary>
        public override string Now { get { return "getdate()"; } }
        /// <summary>
        /// 提交字符
        /// </summary>
        public override string Commit { get { return "GO"; } }
        /// <summary>
        /// 自增长字符
        /// </summary>
        public override string AutoNum { get { return "IDENTITY"; } }

        public override string GetDropFunctionSql(string funcName)
        {
            return string.Format(@"IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[{0}]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[{0}]
n", funcName);
        }

        public override string GetDropProcedureSql(string procName)
        {
            return string.Format(@"IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[{0}]') AND type in (N'P', N'PC'))
DROP PROCEDURE[dbo].[{0}]
GO\n", procName);
        }

        public override string GetDropTableSql(string tableName)
        {
            return string.Format(@"IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[{0}]') AND type in (N'U'))
DROP TABLE [dbo].[{0}]
GO\n", tableName);
        }
        public override string GetDropViewSql(string viewName)
        {
            return string.Format(@"IF  EXISTS (SELECT 1 FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[{0}]'))
DROP VIEW [dbo].[{0}]
GO\n", viewName);
        }
    }
}