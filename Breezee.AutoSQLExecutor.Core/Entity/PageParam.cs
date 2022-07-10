using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Breezee.AutoSQLExecutor.Core
{
    public class PageParam
    {
        //针对分页的设置
        public int PageSize = 50;//每页大小
        public int PageNO = 1;//页码，即查哪一页
        public string PageOrderString = ""; //注：分页SQL的排序字段，对SQL SERVER是必填
    }
}
