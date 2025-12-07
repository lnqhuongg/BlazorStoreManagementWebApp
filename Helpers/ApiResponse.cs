namespace BlazorStoreManagementWebApp.Helpers
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T? DataDTO { get; set; } // <--- T ở đây chính là kiểu dữ liệu bạn muốn chứa
    }
}
