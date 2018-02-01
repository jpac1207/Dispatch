using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Expedicao.Models
{
    public class QueryOptions
    {
        public QueryOptions()
        {
            CurrentPage = 1;
            PageSize = 10;
            SortField = "Id";
            SortOrder = SortOrder.ASC;
        }

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public string SortField { get; set; }
        public SortOrder SortOrder { get; set; }

        public string Sort
        {
            get
            {
                return string.Format("{0} {1}",
                SortField, SortOrder.ToString());
            }
        }

        public override string ToString()
        {
            return "SortOrder=" + this.SortOrder + "&SortField=" + this.SortField + "&CurrentPage=2&PageSize=" + this.PageSize;
        }
    }
}