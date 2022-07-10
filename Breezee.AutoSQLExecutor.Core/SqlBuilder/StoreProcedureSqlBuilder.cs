using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace Breezee.AutoSQLExecutor.Core
{
    /// <summary>
    /// 存储过程SQL构造类
    /// </summary>
    public class StoreProcedureSqlBuilder: BaseSqlBuilder
    {
        public override SqlExecuteType SqlExecuteType { get { return SqlExecuteType.CallStoreProcedure; } }

        public StoreProcedureSqlBuilder(IDataAccess iDataAccess, string sProcedureName, ProcedureExeType pExeType = ProcedureExeType.EXECUTENONQUERY, List<ProcedureParam> listPara = null) : base(iDataAccess)
        {
            ProcedureName = sProcedureName;
            ExeType = pExeType;
            if (listPara != null)
            {
                ListPara = listPara;
            }
        }

        public StoreProcedureSqlBuilder New(IDataAccess iDataAccess, string sProcedureName)
        {
            return new StoreProcedureSqlBuilder(iDataAccess, sProcedureName);
        }

        public StoreProcedureSqlBuilder SetExeType(ProcedureExeType pExeType)
        {
            ExeType = pExeType;
            return this;
        }

        public StoreProcedureSqlBuilder SetParam(List<ProcedureParam> listPara)
        {
            ListPara = listPara;
            return this;
        }

        public string ProcedureName;
        public ProcedureExeType ExeType = ProcedureExeType.EXECUTENONQUERY;
        public List<ProcedureParam> ListPara = new List<ProcedureParam>();

        private string sYiHao = "-";

        public string[] ParaValueArray
        {
            get
            {
                string[] paraString = new string[ListPara.Count];
                int i = 0;
                foreach (ProcedureParam pp in ListPara.OrderBy(r => r.Sort))
                {
                    paraString[i] = pp.ParaValue;
                    i++;
                }
                return paraString;
            }
        }

        #region 转换为字符
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(string.Concat(ProcedureName, sYiHao));
            foreach (ProcedureParam pp in ListPara.OrderBy(r => r.Sort))
            {
                sb.Append(pp.ToString());
            }

            sb.Append(string.Concat(sYiHao, ExeType.ToString()));
            return sb.ToString();
        } 
        #endregion

        #region 调用存储过程
        /// <summary>
        /// 调用存储过程
        /// </summary>
        /// <param name="ps"></param>
        /// <param name="con"></param>
        /// <param name="tran"></param>
        /// <returns></returns>
        public object[] CallStoredProcedure(DbConnection con, DbTransaction tran)
        {
            return DataAccess.CallStoredProcedure(ProcedureName, ParaValueArray,con, tran);
        } 
        #endregion
    }
}
