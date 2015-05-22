namespace Gorilla.DDD.Pagination
{
    public class PaginationSettings
    {

        public enum enSortOrder
        {
            Ascending,
            Descending
        }

        public PaginationSettings(string orderColumn)
        {
            Skip = 0;
            Take = 10;
            OrderDirection = enSortOrder.Ascending;
            OrderColumn = orderColumn;
        }

        public PaginationSettings(string orderColumn, int pageSize, int page)
        {
            Take = pageSize;
            Skip = (page - 1) * pageSize;
            Page = page;
            OrderColumn = orderColumn;
            OrderDirection = enSortOrder.Ascending;
        }

        public int Page { get; set; }
       
        public int Skip { get; set; }

        /// <summary>
        /// Take
        /// </summary>
        public int Take { get; set; }

        /// <summary>
        /// Search
        /// </summary>
        public string Search { get; set; }

        /// <summary>
        /// Order Column
        /// </summary>
        public string OrderColumn { get; set; }

        /// <summary>
        /// Order Direction
        /// </summary>
        public enSortOrder OrderDirection { get; set; }
    }
}
