namespace Library.API.Domains
{
    public class PagedList<T>
    {
        private PagedList(List<T> items, int currentPage, int pageSize, int count)
        {
            Items = items;
            PageSize = pageSize;
            Count = count;
            HasNextPage = currentPage * pageSize < count ? true : false;
            HasPrevPage = currentPage == 1 ? false : true;
        }

        public List<T> Items { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int Count { get; set; }
        public int TotalCount => Items.Count;
        public int TotalPage => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasNextPage { get; set; }
        public bool HasPrevPage { get; set; }

        public static PagedList<T> Create(IQueryable<T> query, int page, int pageSize) {
            var count = query.Count();
            var  pageItems = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            return new (pageItems, page, pageSize, count);
        }
    }
}
