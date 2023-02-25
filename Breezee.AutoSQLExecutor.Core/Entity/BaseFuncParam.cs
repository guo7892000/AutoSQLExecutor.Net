using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Breezee.AutoSQLExecutor.Core
{
    /// <summary>
    /// 方法参数基类
    /// 单个参数，主要提供给UI使用
    /// </summary>
    public class BaseFuncParam
    {
        public string Code;//参数编码
        public FuncParamType FuncParamType = FuncParamType.String;//参数类型
        public object Value;//参数值
        public FuncParamInputType InputType = FuncParamInputType.Option;//是否必须的参数

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="code"></param>
        /// <param name="funcParamType"></param>
        /// <param name="inputType"></param>
        /// <param name="value">参数值：因为创建该类在接口层，而赋值在UI层，所以值参数放最后，可用于设默认值</param>
        public BaseFuncParam(string code, FuncParamType funcParamType = FuncParamType.String, FuncParamInputType inputType = FuncParamInputType.Option, object value = null)
        {
            Code = code;
            FuncParamType = funcParamType;
            InputType = inputType;
            Value = value;
        }
    }
}
