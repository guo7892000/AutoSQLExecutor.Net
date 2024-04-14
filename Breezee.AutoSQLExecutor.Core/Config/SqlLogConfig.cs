using Breezee.Core.Interface;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Breezee.AutoSQLExecutor.Core
{
    /// <summary>
    /// SQL日志配置
    /// </summary>
    public static class SqlLogConfig
    {
        /// <summary>
        /// 是否启用正确SQL日志
        /// </summary>
        public static bool IsEnableRigthSqlLog = false;
        public static bool IsEnableErrorSqlLog = false;
        /// <summary>
        /// SQL日志路径
        /// </summary>
        public static string RigthSqlLogPath = @"\SqlLog\ok";
        public static string ErrorSqlLogPath = @"\SqlLog\err";
        /// <summary>
        /// SQL日志保留天数
        /// </summary>
        public static int RightSqlLogKeepDays = 1;
        public static int ErrorSqlLogKeepDays = 1;
        /// <summary>
        /// SQL日志增加方式
        /// </summary>
        public static SqlLogAddType RightSqlLogAddType = SqlLogAddType.InsertBegin;
        public static SqlLogAddType ErrorSqlLogAddType = SqlLogAddType.InsertBegin;

        static SqlLogConfig()
        {
            InitByConfig(); //根据配置初始化
        }

        public static void InitByConfig()
        {
            //获取DLL路径
            string sDllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Breezee.AutoSQLExecutor.Core.dll");
            //打开配置
            Configuration _AppConfig = ConfigurationManager.OpenExeConfiguration(sDllPath); 
            var appCfg = _AppConfig.AppSettings.Settings; //得到配置
            if (appCfg != null && appCfg.Count>0) 
            {
                foreach (var key in appCfg.AllKeys)
                {
                    if(SqlLogConfigString.RightSqlLogEnable.Equals(key, StringComparison.InvariantCultureIgnoreCase))
                    {
                        IsEnableRigthSqlLog = (appCfg[key].Value.Equals("1") || appCfg[key].Value.Equals("true", StringComparison.InvariantCultureIgnoreCase)) ? true : false;
                    }
                    else if (SqlLogConfigString.ErrorSqlLogEnable.Equals(key, StringComparison.InvariantCultureIgnoreCase))
                    {
                        IsEnableErrorSqlLog = (appCfg[key].Value.Equals("1") || appCfg[key].Value.Equals("true", StringComparison.InvariantCultureIgnoreCase)) ? true : false;
                    }
                    else if (SqlLogConfigString.RightSqlLogPath.Equals(key, StringComparison.InvariantCultureIgnoreCase))
                    {
                        string sPathValue = appCfg[key].Value;
                        if (sPathValue.Contains("://"))
                        {
                            RigthSqlLogPath = sPathValue;
                        }
                        else
                        {
                            while (sPathValue.StartsWith("\\"))
                            {
                                sPathValue= sPathValue.Substring(1); //注：这里要去掉前面的斜杆，要不Combine的路径会不正确
                            }
                            RigthSqlLogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, sPathValue);
                        }
                        if(!Directory.Exists(RigthSqlLogPath))
                        {
                            Directory.CreateDirectory(RigthSqlLogPath);
                        }
                    }
                    else if (SqlLogConfigString.ErrorSqlLogPath.Equals(key, StringComparison.InvariantCultureIgnoreCase))
                    {
                        string sPathValue = appCfg[key].Value;
                        if (sPathValue.Contains("://"))
                        {
                            ErrorSqlLogPath = sPathValue;
                        }
                        else
                        {
                            while (sPathValue.StartsWith("\\"))
                            {
                                sPathValue = sPathValue.Substring(1);//注：这里要去掉前面的斜杆，要不Combine的路径会不正确
                            }
                            ErrorSqlLogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, sPathValue);
                        }
                        if (!Directory.Exists(RigthSqlLogPath))
                        {
                            Directory.CreateDirectory(RigthSqlLogPath);
                        }
                    }
                    else if (SqlLogConfigString.RightSqlLogKeepDays.Equals(key, StringComparison.InvariantCultureIgnoreCase))
                    {
                        string sPathValue = appCfg[key].Value;
                        int iKeepDay;
                        if(int.TryParse(sPathValue, out iKeepDay))
                        {
                            RightSqlLogKeepDays = iKeepDay <= 0 ? 0 : iKeepDay;
                        }
                    }
                    else if (SqlLogConfigString.ErrorSqlLogKeepDays.Equals(key, StringComparison.InvariantCultureIgnoreCase))
                    {
                        string sPathValue = appCfg[key].Value;
                        int iKeepDay;
                        if (int.TryParse(sPathValue, out iKeepDay))
                        {
                            ErrorSqlLogKeepDays = iKeepDay <= 0 ? 0 : iKeepDay;
                        }
                    }
                    else if (SqlLogConfigString.RightSqlLogAddType.Equals(key, StringComparison.InvariantCultureIgnoreCase))
                    {
                        RightSqlLogAddType = "1".Equals(appCfg[key].Value) ? SqlLogAddType.InsertBegin : SqlLogAddType.AppendEnd;
                    }
                    else if (SqlLogConfigString.ErrorSqlLogAddType.Equals(key, StringComparison.InvariantCultureIgnoreCase))
                    {
                        ErrorSqlLogAddType = "1".Equals(appCfg[key].Value) ? SqlLogAddType.InsertBegin : SqlLogAddType.AppendEnd;
                    }
                    else
                    {
                        //其他配置忽略
                    }
                }
            }
        }

        /// <summary>
        /// SQL日志配置项
        /// </summary>
        public static class SqlLogConfigString
        {
            //OK的SQL日志
            public static string RightSqlLogEnable = "RightSqlLogEnable";
            public static string RightSqlLogPath = "RightSqlLogPath";
            public static string RightSqlLogKeepDays = "RightSqlLogKeepDays";
            public static string RightSqlLogAddType = "RightSqlLogAddType";
            //Error的SQL日志
            public static string ErrorSqlLogEnable = "ErrorSqlLogEnable";
            public static string ErrorSqlLogPath = "ErrorSqlLogPath";
            public static string ErrorSqlLogKeepDays = "ErrorSqlLogKeepDays";
            public static string ErrorSqlLogAddType = "ErrorSqlLogAddType";
        }
    }
}
