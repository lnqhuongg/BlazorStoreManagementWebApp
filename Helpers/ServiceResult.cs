namespace BlazorStoreManagementWebApp.Helpers
{
    public class ServiceResult<T>
    {
        public T? Data { get; set; }
        public string Type { get; set; } // loại kết quả: "success", "error", v.v.
        public string Message { get; set; } // tin nhắn mô tả kết quả
    }
}
