using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Gorilla.DDD.Pagination
{
    [DataContract(Name = "paged_result")]
    public class PagedResult<U>
    {

        public PagedResult(int page, int totalRecords, int pageSize, List<U> data)
        {
            this.Page = page;
            this.TotalRecords = totalRecords;
            this.PageSize = pageSize;
            this.Data = data;
        }

        [DataMember(Name = "total_records")]
        public int TotalRecords { get; set; }

        [DataMember(Name = "records")]
        public int Records
        {
            get
            {
                return Data.Count;
            }
            protected set { /*nothing*/ }
        }

        [DataMember(Name = "page")]
        public int Page { get; set; }

        [DataMember(Name = "page_size")]
        public int PageSize { get; set; }

        [DataMember(Name = "pages")]
        public int Pages
        {
            get
            {
                return (int)Math.Ceiling((double)TotalRecords / (double)PageSize);
            }
            protected set { /*nothing*/ }
        }

        [DataMember(Name = "data")]
        public List<U> Data { get; set; }


    }
}
