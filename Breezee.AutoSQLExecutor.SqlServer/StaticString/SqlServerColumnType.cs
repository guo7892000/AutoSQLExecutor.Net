using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Breezee.AutoSQLExecutor.SqlServer
{
    public class SqlServerColumnType
    {
        public static class Text
        {
            //Character 字符串
            public static readonly string Char = "char";
            public static readonly string varchar = "varchar";
            public static readonly string varcharMax = "varchar(max)";
            public static readonly string text = "text";
            //Unicode 字符串
            public static readonly string nchar = "nchar";
            public static readonly string nvarchar = "nvarchar";
            public static readonly string nvarcharMax = "nvarchar(max)";
            public static readonly string ntext = "ntext";
        }

        public static class Date
        {
            public static readonly string smalldatetime = "smalldatetime";
            public static readonly string datetime = "datetime";//从 1753 年 1 月 1 日 到 9999 年 12 月 31 日，精度为 3.33 毫秒。
            public static readonly string timestamp = "timestamp";//基于内部时钟，不对应真实时间。每个表只能有一个 timestamp 变量。
            public static readonly string datetime2 = "datetime2";//从 1753 年 1 月 1 日 到 9999 年 12 月 31 日，精度为 100 纳秒。
            public static readonly string date = "date";//仅存储日期，从 0001 年 1 月 1 日 到 9999 年 12 月 31 日。
            public static readonly string time = "time";//仅存储时间，精度为 100 纳秒。
        }

        public static class Integer
        {
            public static readonly string smallint = "smallint";
            public static readonly string tinyint = "tinyint";
            public static readonly string Int = "int";
            public static readonly string bigint = "bigint";
            public static readonly string Float = "float";//从 -1.79E + 308 到 1.79E + 308 的浮动精度数字数据。 参数 n 指示该字段保存 4 字节还是 8 字节。
            public static readonly string money = "money";//介于 -922,337,203,685,477.5808 和 922,337,203,685,477.5807 之间的货币数据。
            public static readonly string smallmoney = "smallmoney";
            public static readonly string real = "real";//从 -3.40E + 38 到 3.40E + 38 的浮动精度数字数据。
        }

        public static class Precision
        {
            public static readonly string Decimal = "decimal";//(限制小数位数)，储存精确数值。
            public static readonly string numeric = "numeric";
        }

        public static class Blob
        {
            //二进制
            public static readonly string binary = "binary";
            public static readonly string varbinary = "varbinary";
            public static readonly string varbinaryMax = "varbinary(max)";
            //特殊
            public static readonly string bit = "bit";
            public static readonly string image = "image";//长度可变的二进制数据，范围为:0~2的31次方-1个字节。用于存储照片、目录图片或者图画，容量也是2147 483 647个字节，由系统根据数据的长度自动分配空间。
            public static readonly string xml = "xml"; //存储 XML 格式化数据。最多 2GB。
        }

        public static class Other
        {
            public static readonly string sql_variant = "sql_variant";//存储最多 8,000 字节不同数据类型的数据，除了 text、ntext 以及 timestamp。
            public static readonly string uniqueidentifier = "uniqueidentifier";//16字节的GUID(Globally Unique Identifier,全球唯一标识符)，是Sql Server根据网络适配器地址和主机CPU时钟产生的唯一号码，其中，每个为都是0~9或a~f范围内的十六进制数字。

        }

        public class DefaultValue
        {
            public static string Now = "getdate()";
        }
    }
}
