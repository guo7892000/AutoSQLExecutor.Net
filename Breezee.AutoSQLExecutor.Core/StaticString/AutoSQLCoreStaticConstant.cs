﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Breezee.AutoSQLExecutor.Core
{
    public static partial class AutoSQLCoreStaticConstant
    {
        /// <summary>
        /// Dictionary唯一ID关键字
        /// </summary>
        public static readonly string UNIQUE_FLAG = "LIFECYCLE_UNID";

        #region 分页相关
        /// <summary>
        /// 每页记录数常量
        /// </summary>
        public static readonly string PAGE_SIZE = "PAGE_SIZE";

        /// <summary>
        /// 页号常量
        /// </summary>
        public static readonly string PAGE_NO = "PAGE_NO";
        /// <summary>
        /// 总记录数常量
        /// </summary>
        public static readonly string TOTAL_COUNT = "#TOTAL_COUNT#";

        /// <summary>
        /// 是否分页
        /// </summary>
        public static readonly string IS_PAGE = "IS_PAGE";
        #endregion

        /// <summary>
        /// 判断是否用户在线时间间隔
        /// </summary>
        public static readonly int LIVE_INTERVAL = 5;

        /// <summary>
        /// 编辑下拉框中第一项的显示文本
        /// </summary>
        public static readonly string DDL_Text_Choose = "--请选择--";

        /// <summary>
        /// 下拉框全部的显示文本
        /// </summary>
        public static readonly string DDL_Text_All = "全部";

        /// <summary>
        /// Excel_1997-2003 文件类型过滤字符串
        /// </summary>
        public static readonly string EXCEL_FILTER_03 = "Excel 97-2003(*.xls)|*.xls";

        /// <summary>
        /// Excel_1997-2012 文件类型过滤字符串
        /// </summary>
        public static readonly string EXCEL_FILTER_0307 = "Excel 97-2003(*.xls)|*.xls|Excel 工作薄(*.xlsx)|*.xlsx";

        /// <summary>
        /// Excel_2003-2012 文件类型过滤字符串
        /// </summary>
        public static readonly string EXCEL_FILTER_0710 = "Excel 工作薄(*.xlsx)|*.xlsx";

        public static readonly string Dictionary_Key = "VALUE_CODE";
        public static readonly string Dictionary_Value = "VALUE_NAME";

        #region 返回消息类别定义
        /// <summary>
        /// 返回标记，0失败，1成功
        /// </summary>
        public static readonly string FRA_RETURN_FLAG = "#FRA_RETURN_FLAG#";

        /// <summary>
        /// 用于界面显示给用户的界面友好提示信息
        /// </summary>
        public static readonly string FRA_USER_MSG = "#FRA_USER_MSG#";

        /// <summary>
        /// 应用程序捕获的原始异常信息
        /// </summary>
        public static readonly string FRA_EXCEPTION = "#FRA_EXCEPTION#";

        /// <summary>
        /// 应用程序捕获的原始异常类型
        /// </summary>
        public static readonly string FRA_ERR_TYPE = "#FRA_ERR_TYPE#";

        /// <summary>
        /// 应用程序捕获的经过转换的异常内容
        /// </summary>
        public static readonly string FRA_ERR_MSG = "#FRA_ERR_MSG#";

        /// <summary>
        /// 查询中返回的表结果键
        /// </summary>
        public static readonly string FRA_QUERY_RESULT = "#FRA_DT_RESULT#";

        /// <summary>
        /// 表更新中的列动态固定值键
        /// </summary>
        public static readonly string FRA_TABLE_EXTEND_PROPERTY_COLUMNS_FIX_VALUE = "动态固定值";

        #endregion


    }
}
