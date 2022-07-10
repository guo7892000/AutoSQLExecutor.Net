using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Breezee.AutoSQLExecutor.Core
{
    /// <summary>
    /// 存储过程参数构造类
    /// </summary>
    public class ProcedureParam
    {
        public int Sort;
        public string ParaCode;
        public SqlDbType DBType;
        public int Length;
        public ProcedureParaInOutType InOutType;
        public string ParaValue;

        private string sFenHao = ":";
        private string sDouHao = ",";

        public ProcedureParam(int iSort, string sParaCode,string sParaValue,SqlDbType db, int iLength, ProcedureParaInOutType inOutType = ProcedureParaInOutType.InPut)
        {
            Sort = iSort;
            ParaCode = sParaCode;
            ParaValue = sParaValue;
            DBType = db;
            Length = iLength;
            InOutType = inOutType;
        }

        /// <summary>
        /// 重写转换为字符方法
        /// 分号分隔每个字符，逗号分隔每个参数
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Concat(ParaCode, sFenHao, InOutType.ToString().ToUpper(), sFenHao, DBType.ToString(), sFenHao, Length.ToString(), sDouHao);
        }
    }

    public enum ProcedureParaInOutType
    { 
        InPut=1,
        OutPut =2,
        InOutPut =3,
        ReturnValue = 4
    }

    public enum ProcedureExeType
    {
        EXECUTENONQUERY = 1,
        Fill = 2,
    }

}
