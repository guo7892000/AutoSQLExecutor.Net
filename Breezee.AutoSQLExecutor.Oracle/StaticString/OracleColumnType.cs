using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Breezee.AutoSQLExecutor.Oracle
{
    public class OracleColumnType
    {
        public static class Text
        {
            public static readonly string varchar2 = "varchar2";    //可变长的字符串，具体定义时指明最大长度n。一般适用于英文和数字
            public static readonly string nvarchar2 = "nvarchar2";  //适用中文和其他字符，其中N表示Unicode常量，可以解决多语言字符集之间的转换问题
            public static readonly string Char = "char";//有固定长度和最大长度的字符串。存储在数据类型为CHAR字段中的数据将以空格的形式补到最大长度。长度定义在1——2000字节之间。
            //不推荐使用
            public static readonly string Long = "long";//可变长字符列，最大长度限制是2GB，用于不需要作字符串搜索的长串数据
        }

        public static class Date
        {
            public static readonly string date = "date";//从公元前4712年1月1日到公元4712年12月31日的所有合法日期
            public static readonly string timestamp = "timestamp";
            public static readonly string timestampWithLocalTimeZone = "timestamp with local time zone";
            public static readonly string timestampWithTimeZone = "timestamp with time zone";
            public static readonly string intervalDayToSecond = "interval day to second";
            public static readonly string intervalYearToMonth = "interval year to month";
        }

        public static class Integer
        {
            public static readonly string integer = "integer";
            
        }

        public static class Precision
        {
            public static readonly string number = "number";//number(m,n):可变长的数值列，允许0、正值及负值，m是所有有效数字的位数，n是小数点以后的位数。
        }

        public static class Blob
        {
            public static readonly string binary_double = "binary_double";
            public static readonly string binary_float = "binary_float";
            //三种大型对象(LOB)，用来保存较大的图形文件或带格式的文本文件
            public static readonly string blob = "blob";//二进制大型对象（Binary Large Object)
            public static readonly string clob = "clob";//字符大型对象（Character Large Object)
            public static readonly string nclob = "nclob";//基于国家语言字符集的NCLOB数据类型用于存储数据库中的固定宽度单字节或多字节字符的大型数据块，不支持宽度不等的字符集。
            //不推荐使用
            public static readonly string raw = "raw";//可变长二进制数据，在具体定义字段的时候必须指明最大长度n，raw是一种较老的数据类型。
            public static readonly string longRaw = "long raw";//可变长二进制数据，最大长度是2GB。
        }

        public static class Other
        {

        }

        public class DefaultValue
        {
            public static string Now = "sysdate";
        }
    }
}
