using Breezee.AutoSQLExecutor.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Breezee.AutoSQLExecutor.Oracle
{
    public class BOracleSqlDifferent : ISqlDifferent
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
        public override string Now { get { return "sysdate"; } }
        /// <summary>
        /// 提交字符
        /// </summary>
        public override string Commit { get { return "/"; } }
        /// <summary>
        /// 自增长字符：不使用，通过序列实现
        /// </summary>
        public override string AutoNum { get { return String.Empty; } }

        public override string GetDropFunctionSql(string funcName)
        {
            return GetCommonDropSql(OracleObjectTypeString.Function, funcName);
        }

        public override string GetDropProcedureSql(string procName)
        {
            return GetCommonDropSql(OracleObjectTypeString.Procedure, procName);
        }

        public override string GetDropTableSql(string tableName)
        {
            return GetCommonDropSql(OracleObjectTypeString.Table, tableName);
        }

        public override string GetDropViewSql(string viewName)
        {
            return GetCommonDropSql(OracleObjectTypeString.View, viewName);
        }

        private string GetCommonDropSql(string sObjectType,string sObjectName)
        {
            string sSql = string.Format(@"declare  nCount  number;
            begin
              nCount:=0;
              select count(1) into nCount from user_objects
              where upper(object_type) = '{0}' and upper(object_name) = '{1}' ;
              if nCount = 1 then
                begin 
                  execute immediate 'drop {0} {1}';
                end;
              end if;
            end;
            /\n", sObjectType,sObjectName.ToUpper());
            return sSql;
        }
    }

    class OracleObjectTypeString
    {
        public static string Table = "TABLE";
        public static string View = "VIEW";
        public static string Procedure = "PROCEDURE";
        public static string Function = "FUNCTION";
        public static string Index = "INDEX";
        public static string Trigger = "TRIGGER";
        public static string Sequence = "SEQUENCE";
    }

}
