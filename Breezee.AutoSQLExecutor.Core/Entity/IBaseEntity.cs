using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Breezee.AutoSQLExecutor.Core
{
    /// <summary>
    /// 实体基类
    /// </summary>
    public abstract class IBaseEntity
    {
        #region 数据库相关
        /// <summary>
        /// 实体类型
        /// </summary>
        public abstract EntityType EntType { get; }
        /// <summary>
        /// 数据库表名
        /// </summary>
        public abstract string DBTableName { get; }

        /// <summary>
        /// 表中文名
        /// </summary>
        public virtual string DBTableNameCN { get; }
        /// <summary>
        /// 备注
        /// </summary>
        public virtual string DBTableComment { get; }

        /// <summary>
        /// 数据库列清单
        /// </summary>
        public abstract List<BaseEntityField> DbColumnList { get; }

        public abstract List<string> ColumnStringList { get; }
        #endregion

        #region 使用相关
        /// <summary>
        /// 并发更新ID值：当需要使用并发更新字段检查并发时，并给该字段赋值
        /// </summary>
        public string UpdateControlIdValue { get; set; }

        /// <summary>
        /// 编辑类型
        /// </summary>
        public EntityEditType EditType { get; set; }

        public List<BaseEntityField> GetUpdateColumnList()
        {
            if(EntType== EntityType.Table && EditType== EntityEditType.Update)
            {
                return DbColumnList.Where(r => r.IsUpdate = true).ToList();
            }
            return new List<BaseEntityField>();
        }

        /// <summary>
        /// 错误消息
        /// </summary>
        public string ErrorMessage;

        /// <summary>
        /// 验证方法
        /// </summary>
        /// <returns></returns>
        public virtual string Validate()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
