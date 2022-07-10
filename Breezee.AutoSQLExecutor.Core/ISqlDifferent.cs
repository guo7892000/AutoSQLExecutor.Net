using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Breezee.AutoSQLExecutor.Core
{
    /// <summary>
    /// SQL差异抽象类
    /// </summary>
    public abstract class ISqlDifferent
    {
        /// <summary>
        /// 字符串连接
        /// </summary>
        public abstract string Combine { get; }
        /// <summary>
        /// 参数前缀
        /// </summary>
        public abstract string ParamPrefix { get; }
        /// <summary>
        /// 当前日期
        /// </summary>
        public abstract string Now { get; }
        /// <summary>
        /// 提交字符
        /// </summary>
        public abstract string Commit { get; }
        /// <summary>
        /// 自增长字符
        /// </summary>
        public abstract string AutoNum { get; }

        /// <summary>
        /// 获取删除表SQL
        /// </summary>
        /// <param name="tableCode"></param>
        /// <returns></returns>
        public abstract string GetDropTableSql(string tableName);
        public abstract string GetDropViewSql(string viewName);
        public abstract string GetDropFunctionSql(string funcName);
        public abstract string GetDropProcedureSql(string procName);

    }
}
