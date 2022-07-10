using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Breezee.AutoSQLExecutor.MySql
{
    public class MySqlColumnType
    {
        public static class Text
        {
            //最常用字符类型
            public static readonly string Char = "char";//固定长度字符串，其长度范围为0~255且与编码方式无关，无论字符实际长度是多少，都会按照指定长度存储，不够的用空格补足
            public static readonly string varchar = "varchar";//可变长度字符串，在utf8编码的数据库中其长度范围为0~21844
            //大字符串
            public static readonly string tinytext = "tinytext";
            public static readonly string text = "text";
            public static readonly string mediumtext = "mediumtext";
            public static readonly string longtext = "longtext";
            //特殊字符类型
            public static readonly string set = "set";//一个设置，字符串对象可以有零个或 多个SET成员	1、2、3、4或8个字节，取决于集合 成员的数量（最多64个成员）
            public static readonly string Enum = "enum"; //枚举类型，只能有一个枚举字符串值	1或2个字节，取决于枚举值的数目(最大值为65535)
        }

        public static class Date
        {
            public static readonly string date = "date";          
            public static readonly string time = "time";
            public static readonly string year = "year";
            public static readonly string datetime = "datetime";
            public static readonly string timestamp = "timestamp";
        }

        public static class Integer
        {
            public static readonly string tinyint = "tinyint";
            public static readonly string smallint = "smallint";
            public static readonly string Int = "int";
            public static readonly string mediumint = "mediumint";
            public static readonly string bigint = "bigint";
        }

        public static class Precision
        {
            /*float/double在db中存储的是近似值，而decimal则是以字符串形式进行保存的。
              decimal(M,D)的规则和float/double相同，但区别在float/double在不指定M、D时默认按照实际精度来处理而decimal在不指定M、D时默认为decimal(10, 0)。
            */
            public static readonly string Decimal = "decimal";//decimal无论写入数据中的数据是多少，都不会存在精度丢失问题
            public static readonly string numeric = "numeric";
            //float、double类型存在精度丢失问题
            public static readonly string Float = "float";
            public static readonly string Double = "double";
            public static readonly string real = "real";
        }

        public static class Blob
        {
            public static readonly string bit = "bit";//位字段类型，大约 (M+7)/8 字节
            public static readonly string binary = "binary";//固定长度二进制字符串，M 字节
            public static readonly string varbinary = "varbinary";//可变长度二进制字符串
            //二进制大对象
            public static readonly string tinyblob = "tinyblob";
            public static readonly string blob = "blob";
            public static readonly string mediumblob = "mediumblob";//中等大小的BLOB
            public static readonly string longblob = "longblob"; //非常大的BLOB，L+4 字节，在此，L<2^32
        }

        public static class Other
        {
            public static readonly string Bool = "bool";
            public static readonly string boolean = "boolean";           
        }

        public class DefaultValue
        {
            public static string Now = "now()";
        }
    }
}
