using System;

namespace Breezee.AutoSQLExecutor.Core
{
    /// <summary>
    /// 数据库服务器信息
    /// </summary>
    public class DbServerInfo
    {
        #region 变量
        private string _database = "";
        private DataBaseType _databaseType = DataBaseType.MySql;
        private string _password = "";
        private string _serverName = ".";
        private string _userName = "";
        private string _portNo = "";
        private string _schemaName = "";
        #endregion

        #region 构造函数
        public DbServerInfo()
        {

        }

        public DbServerInfo(DataBaseType databaseType, string serverName, string userName, string password, string database, string portNo, string schemaName)
        {
            _databaseType = databaseType;
            _serverName = serverName;
            _userName = userName;
            _password = password;
            _database = database;
            _portNo = portNo;
            _schemaName = schemaName;

        }
        #endregion

        #region 属性
        public string Database
        {
            get
            {
                return _database;
            }
            set
            {
                _database = value;
            }
        }

        public DataBaseType DatabaseType
        {
            get
            {
                return _databaseType;
            }
            set
            {
                _databaseType = value;
            }
        }

        public string Password
        {
            get
            {
                return _password;
            }
            set
            {
                _password = value;
            }
        }

        public string ServerName
        {
            get
            {
                return _serverName;
            }
            set
            {
                _serverName = value;
            }
        }

        public string UserName
        {
            get
            {
                return _userName;
            }
            set
            {
                _userName = value;
            }
        }
        public string PortNo
        {
            get
            {
                return _portNo;
            }
            set
            {
                _portNo = value;
            }
        }
        public string SchemaName
        {
            get
            {
                return _schemaName;
            }
            set
            {
                _schemaName = value;
            }
        }
        #endregion

        #region 链式调用方法
        public DbServerInfo DbType(DataBaseType databaseType)
        {
            DatabaseType = databaseType;
            return this;
        }
        public DbServerInfo Server(string serverName)
        {
            ServerName = serverName;
            return this;
        }
        public DbServerInfo User(string userName)
        {
            UserName = userName;
            return this;
        }
        public DbServerInfo Pwd(string password)
        {
            Password = password;
            return this;
        }
        public DbServerInfo Db(string database)
        {
            Database = database;
            return this;
        }
        public DbServerInfo Port(string portNo)
        {
            PortNo = portNo;
            return this;
        }
        public DbServerInfo Schema(string schemaName)
        {
            SchemaName = schemaName;
            return this;
        } 
        #endregion
    }
}

