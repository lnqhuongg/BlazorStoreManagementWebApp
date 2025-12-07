namespace BlazorStoreManagementWebApp.Helpers
{
    public class PagedResult<T>
    {
        public List<T> Data { get; set; }
        public int Total { get; set; } // tổng số bản ghi 
        public int Page { get; set; } // trang hiện tại
        public int PageSize { get; set; } // số bản ghi trên mỗi trang
        public int TotalPages => (int)Math.Ceiling(Total / (double)PageSize); // tổng số trang
    }
}
