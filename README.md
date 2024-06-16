# AutoSQLExecutor.Net

#### 介绍
以MyPeach.Net（个人开源的项目）和各数据库提供的类库为基础，简化SQL的解析和执行，目前仅支持SqlServer、Oracle、MySql、PostgreSQL、SQLite五种数据库！

#### 软件架构
使用基于.Net Framework 4.8的C#语言编写的类库。


#### 调试说明
1. 下载目录：https://gitee.com/breezee2000/WorkHelper/tree/master/20_AutoSQLExcutor
2. 打开解决方案：AutoSQLExecutor.Net4.sln
3. 设置Breezee.AutoSQLExecutor.Test为启动项目。
4. 我们可以尝试修改其下的Program.cs文件，以测试五种数据库的访问。

#### 使用说明
## 1.创建数据库服务对象：
DbServerInfo server = new DbServerInfo(DataBaseType.SqlServer,"localhost","sa","sa123456", "PeachBase","1433",null,null);
DbServerInfo server = new DbServerInfo(DataBaseType.MySql, "localhost", "root", "root", "aprilspring", "3306", null, null);
## 2.连接服务器得到数据库访问对象
IDataAccess dataAccess = Common.AutoSQLExecutors.Connect(server);
## 3.构造SQL：
要键化条件，参数配置示例：#MDLIST:M:R:LS:F:D-now()-r-n:N#。 其中第一项必须是为参数名（本例为MDLIST）；M（Must）是必填；R（Replace）是替换值；
LS是字符列表（List String），LI是指整型列表； F是优化配置项（First）； D为默认值，其支持-&,;，；六种字符作为分隔符的更多属性配置（不区分大小写），
其中第二个必须是默认值（注：如包括R配置，则会去掉值中的单引号），后面的R、N分别是值替换、不加引号配置。示例：
string sql = "select * from bas_city where city_id in (#CITY_ID:LI#)"; //这里必须加上LI，表示是整型的列表
## 4.构造条件字典：
IDictionary<string, object> dicCond = new Dictionary<string, object>();
dicCond["CITY_ID"] = new List<int> { 1, 100 }; 
## 5、调用数据库访问对象的查询数据方法：
DataTable dtQuery = dataAccess.QueryData(sql, dicCond);

#### 新项目需要引入的DLL包括：
1.  Breezee.AutoSQLExecutor.Common.dll
2.  Breezee.AutoSQLExecutor.Core.dll
3.  Breezee.Core.Interface.dll
4.  其他可能会用到的程序集：
	Breezee.Core.dll、Breezee.Core.Entity.dll、Breezee.Core.Tool.dll


## 问题和BUG提交
提交BUG问题时，请提供详细描述或截图说明，谢谢！    
[邮件反馈建议或问题](guo7892000@126.com)    
[微信号] BreezeeHui    