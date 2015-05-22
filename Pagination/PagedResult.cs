using System;
using System.Collections.Generic;

namespace Gorilla.DDD.Pagination
{
    public class PagedResult<U>
    {

        public PagedResult(int page, int totalRecords, int pageSize, List<U> data)
        {
            this.Page = page;
            this.TotalRecords = totalRecords;
            this.PageSize = pageSize;
            this.Data = data;
        }

        public int TotalRecords { get; set; }

        public int Records
        {
            get
            {
                return Data.Count;
            }
        }

        public int Page { get; set; }

        public int PageSize { get; set; }

        public int Pages
        {
            get
            {
                return (int)Math.Ceiling((double)(TotalRecords / PageSize));
            }
        }

        public List<U> Data { get; set; }


    }
}
