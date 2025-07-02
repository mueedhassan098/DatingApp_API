using Microsoft.EntityFrameworkCore;

namespace APIGateWay.Helpers
{
    public class PagedList<T>:List<T>
    {
        public PagedList(IEnumerable<T> items, int Count, int pageNumber, int pageSize)
        {
            PageSize = pageSize;
            CurrentPage = pageNumber;
            TotalCount = Count;
            TotalPages = (int)Math.Ceiling(Count / (double)PageSize);
            AddRange(items);
        }

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }

        public static async Task<PagedList<T>> CreateAsync(IQueryable<T> source,
            int pageNumber, int pageSize)
        {
            var count=await source.CountAsync();
            var item = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PagedList<T>(item, count,pageNumber,pageSize);
        }
    }
}
