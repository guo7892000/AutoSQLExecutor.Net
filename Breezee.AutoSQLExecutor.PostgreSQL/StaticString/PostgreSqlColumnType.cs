using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Breezee.AutoSQLExecutor.PostgreSQL
{
    public class PostgreSqlColumnType
    {
        public static class Text
        {
            public static readonly string character = "character";  //f定长,不足补空白
            public static readonly string characterArr = "character[]";
            public static readonly string characterVarying = "character varying"; //变长，有长度限制
            public static readonly string characterVaryingArr = "character varying[]";
            public static readonly string text = "text";    //变长，无长度限制
            public static readonly string textArr = "text[]";
            //特殊字符类型：不推荐使用
            public static readonly string Char = "char";   //1 字节 单字节内部类型
            public static readonly string charArr = "char[]";
            public static readonly string name = "name";    //64 字节 对象名的内部类型
            public static readonly string nameArr = "name[]";
        }

        public static class Date
        {
            public static readonly string date = "date";    //只用于日期
            public static readonly string dateArr = "date[]";
            public static readonly string interval = "interval";    //时间间隔
            public static readonly string intervalArr = "interval[]";
            public static readonly string timeWithTimeZone = "time with time zone"; //只用于一日内时间，带时区
            public static readonly string timeWithTimeZoneArr = "time with time zone[]";
            public static readonly string timeWithdoutTimeZone = "time withdout time zone"; //只用于一日内时间
            public static readonly string timeWithdoutTimeZoneArr = "time withdout time zone[]";
            public static readonly string timestampWithTimeZone = "timestamp with time zone";   //日期和时间，有时区
            public static readonly string timestampWithTimeZoneArr = "timestamp with time zone[]";
            public static readonly string timestampWithdoutTimeZone = "timestamp withdout time zone";//日期和时间(无时区)
            public static readonly string timestampWithdoutTimeZoneArr = "timestamp withdout time zone[]";
            //不推荐使用
            public static readonly string abstime = "abstime";  //存储日期
            public static readonly string abstimeArr = "abstime[]";
            public static readonly string reltime = "reltime";//存储日期之间的差异
        }

        public static class Integer
        {
            public static readonly string smalltint = "smalltint";  //常用的整数:-32768 到 +32767
            public static readonly string smalltintArr = "smalltint[]";
            public static readonly string integer = "integer";  //常用的整数:	-2147483648 到 +2147483647
            public static readonly string integerArr = "integer[]";           
            public static readonly string bigint = "bigint";    //大范围整数: -9223372036854775808 到 +9223372036854775807
            public static readonly string bigintArr = "bigint[]";
            //序列
            public static readonly string smallserial = "smallserial";  //自增的小范围整数:	1 到 32767
            public static readonly string serial = "serial";    //自增整数:	1 到 2147483647
            public static readonly string bigserial = "bigserial";  //自增的大范围整数:1 到 9223372036854775807
            
            public static readonly string bit = "bit";//固定长度的bit字符串
            public static readonly string bitArr = "bit[]"; //可变长度的bit字符串
            public static readonly string bitVarying = "bit varying";
            public static readonly string bitVaryingArr = "bit varying[]";

            public static readonly string money = "money";//货币金额	-92233720368547758.08 到 +92233720368547758.07
            public static readonly string moneyArr = "money[]";
            //不推荐使用
            public static readonly string int2vector = "int2vector";
            public static readonly string int2vectorArr = "int2vector[]";
        }

        public static class Precision
        {
            public static readonly string numeric = "numeric";  //用户指定的精度，精确：小数点前 131072 位；小数点后 16383 位
            public static readonly string numericArr = "numeric[]";
            public static readonly string doublePrecision = "double precision";//用户指定的精度，精确：小数点前 131072 位；小数点后 16383 位
            public static readonly string doublePrecisionArr = "double precision[]";
            public static readonly string real = "real";    //单精度浮点数（4个字节）
            public static readonly string realArr = "real[]";
        }

        public static class Blob
        {
            public static readonly string bytea = "bytea";  //二进制数据（"字节数组"）
            public static readonly string byteaArr = "bytea[]";
            public static readonly string json = "json";    //JSON 类型
            public static readonly string jsonArr = "json[]";
            public static readonly string jsonb = "jsonb";  //二进制JSON数据
            public static readonly string xml = "xml";  //xml类型
            public static readonly string xmlArr = "xml[]";

        }

        public static class Special
        {
            public static readonly string boolean = "boolean";  //布尔类型:	true/false
            public static readonly string booleanArr = "boolean[]";
            public static readonly string uuid = "uuid";    //UUID 类型:通用唯一标识符（UUID）。一些系统认为这个数据类型为全球唯一标识符，或GUID。 
            public static readonly string uuidArr = "uuid[]";
        }

        public static class Network
        {
            public static readonly string cidr = "cidr";    //IPv4 或 IPv6 网络
            public static readonly string cidrArr = "cidr[]";
            public static readonly string inet = "inet";    //IPv4 或 IPv6 主机和网络
            public static readonly string inetArr = "inet[]";
            public static readonly string macaddr = "macaddr";  //MAC 地址
            public static readonly string macaddrArr = "macaddr[]";
            
        }
        public static class Shape
        {
            public static readonly string box = "box";  //	矩形
            public static readonly string boxArr = "box[]";
            public static readonly string circle = "circle";
            public static readonly string circleArr = "circle[]";
            public static readonly string line = "line";    //(无穷)直线(未完全实现)
            public static readonly string lineArr = "line[]";   
            public static readonly string lseg = "lseg";    //(有限)线段
            public static readonly string lsegArr = "lseg[]";
            public static readonly string path = "path";    //闭合路径(与多边形类似)
            public static readonly string pathArr = "path[]";
            public static readonly string point = "point";
            public static readonly string pointArr = "point[]";
            public static readonly string polygon = "polygon";  //多边形(与闭合路径相似)
            public static readonly string polygonArr = "polygon[]";
        }

        public static class Range
        {
            public static readonly string int4range = "int4range";  //integer的范围
            public static readonly string int4rangeArr = "int4range[]";
            public static readonly string int8range = "int8range"; //bigint的范围
            public static readonly string int8rangeArr = "int8range[]";
            public static readonly string numrange = "numrange";    //numeric的范围
            public static readonly string numrangeArr = "numrange[]";
            public static readonly string dateRange = "date range"; //date的范围
            public static readonly string dateRangeArr = "date range[]";
            public static readonly string tsrange = "tsrange";  //timestamp without time zone的范围
            public static readonly string tsrangeArr = "tsrange[]";
            public static readonly string tstzrange = "tstzrange";  //timestamp with time zone的范围
            public static readonly string tstzrangeArr = "tstzrange[]";
        }

        public static class OjectType
        {
            public static readonly string oid = "oid";  //数字化的对象标识符
            public static readonly string oidArr = "oid[]";
            public static readonly string refcursor = "refcursor";
            public static readonly string refcursorArr = "refcursor[]";
            public static readonly string regclass = "regclass";    //关系名
            public static readonly string regclassArr = "regclass[]";
            public static readonly string regconfig = "regconfig";  //文本搜索配置
            public static readonly string regconfigArr = "regconfig[]";
            public static readonly string regdictionay = "regdictionay";    //文本搜索字典
            public static readonly string regdictionayArr = "regdictionay[]";
            public static readonly string regnamespace = "regnamespace";
            public static readonly string regnamespaceArr = "regnamespace[]";
            public static readonly string regoper = "regoper";  //操作符名
            public static readonly string regoperArr = "regoper[]";
            public static readonly string regoperate = "regoperate";    //带参数类型的操作符
            public static readonly string regoperateArr = "regoperate[]";
            public static readonly string regproc = "regproc";  //函数名字
            public static readonly string regprocArr = "regproc[]";
            public static readonly string regprocedure = "regprocedure";    //带参数类型的函数
            public static readonly string regprocedureArr = "regprocedure[]";
            public static readonly string regrole = "regrole";
            public static readonly string regroleArr = "regrole[]";
            public static readonly string regtype = "regtype";  //数据类型名
            public static readonly string regtypeArr = "regtype[]";
        }
        public static class Other
        {
            //文本搜索类型
            public static readonly string tsquery = "tsquery";  //存储用于检索的词汇
            public static readonly string tsqueryArr = "tsquery[]";
            public static readonly string tsvactor = "tsvactor";    //无重复值的 lexemes 排序列表
            public static readonly string tsvactorArr = "tsvactor[]";

            public static readonly string pg_lsn = "pg_lsn";//PostgreSQL日志序列号
            public static readonly string pg_lsnArr = "pg_lsn[]";
            public static readonly string txid_snapshort = "txid_snapshort";//用户级事务ID快照
            public static readonly string txid_snapshortArr = "txid_snapshort[]";
            //不推荐使用
            public static readonly string tinteval = "tinteval";
            public static readonly string tintevalArr = "tinteval[]";
            public static readonly string tid = "tid";  //基于行号(tid)的快速扫描
            public static readonly string tidArr = "tid[]";
            public static readonly string smgr = "smgr";    //存储管理器类型
            public static readonly string cstringArr = "cstring[]";
            public static readonly string cid = "cid";
            public static readonly string cidArr = "cid[]";
            public static readonly string aclitem = "aclitem";
            public static readonly string aclitemArr = "aclitem[]";
            public static readonly string gtsvector = "gtsvector";
            public static readonly string gtsvectorArr = "gtsvector[]";
            public static readonly string oidvactor = "oidvactor";
            public static readonly string oidvactorArr = "oidvactor[]";
            public static readonly string xid = "xid";
            public static readonly string xidArr = "xid[]";
            public static readonly string pg_node_tree = "pg_node_tree";
        }

        public class DefaultValue
        {
            public static string Now = "now()";
        }
    }
}
