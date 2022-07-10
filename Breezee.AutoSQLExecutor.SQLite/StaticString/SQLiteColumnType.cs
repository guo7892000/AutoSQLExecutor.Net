using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Breezee.AutoSQLExecutor.SQLite
{
    /// <summary>
    /// 由于SQLite采用的是动态数据类型，而其他传统的关系型数据库使用的是静态数据类型。
    /// 动态数据类型：数据库存储的数据类型和数据输入的类型是动态匹配的，简言之，定义了一个数据库字段为字符串TEXT类型，你也可以存入整型INTEGER的数据。
    /// 静态数据类型：数据库存储的数据类型和数据输入的类型要求是一致的。简言之，字段可以存储的数据类型是在表声明时即以确定的。
    /// </summary>
    public class SQLiteColumnType
    {
        /*SQLite的存储类型有下面五种：
        存储类	描述
        NULL	值是一个 NULL 值。
        INTEGER	值是一个带符号的整数，根据值的大小存储在 1、2、3、4、6 或 8 字节中。
        REAL	值是一个浮点值，存储为 8 字节的 IEEE 浮点数字。
        TEXT	值是一个文本字符串，使用数据库编码（UTF-8、UTF-16BE 或 UTF-16LE）存储。
        BLOB	值是一个 blob 数据，完全根据它的输入存储。
         */
        public static class Text
        {
            //CHARACTER(20)、VARCHAR(255)、VARYING CHARACTER(255)、NCHAR(55)、NATIVE CHARACTER(70)、NVARCHAR(100)、TEXT、CLOB
            public static readonly string character = "character";
            public static readonly string nvarchar = "nvarchar";
            public static readonly string varchar = "varchar";
            public static readonly string varyingCharacter = "varying character";
            public static readonly string nchar = "nchar";
            public static readonly string nativeCharacter = "native character";
            public static readonly string text = "text";
            public static readonly string Clob = "clob";
        }

        public static class Integer
        {
            //INT、INTEGER、TINYINT、SMALLINT、MEDIUMINT、BIGINT、UNSIGNED BIG INT、INT2、INT8
            public static readonly string int2 = "int2";
            public static readonly string int8 = "int8";
            public static readonly string integer = "integer";
            public static readonly string Int = "int";
            public static readonly string tinyint = "tinyint";
            public static readonly string smallint = "smallint";
            public static readonly string mediumint = "mediumint";
            public static readonly string bigint = "bigint";
            public static readonly string unsignedBigInt = "unsigned big int";
        }

        public static class Real
        {
            //REAL、DOUBLE、DOUBLE PRECISION、FLOAT
            public static readonly string Float = "float";
            public static readonly string real = "real";
            public static readonly string Double = "double";
            public static readonly string doublePrecision = "double precision";
        }
        public static class Numeric
        {
            //NUMERIC、DECIMAL(10,5)、BOOLEAN、DATE、DATETIME
            public static readonly string boolean = "boolean";
            public static readonly string numeric = "numeric";
            public static readonly string Decimal = "decimal";
            public static readonly string date = "date";
            public static readonly string datetime = "datetime";
        }

        public static class None
        {
            public static readonly string blob = "blob";
            
        }

        
        public class DefaultValue
        {
            public static string Now = "(datetime('now','localtime'))"; //注：在设置列默认值时，最外面的括号是必须的，否则会报错！
        }

    }
}
