namespace BlazorStoreManagementWebApp.Models.Momo
{
    public class MomoExecuteResponseModel
    {
        public string OrderId { get; set; }
        public string Amount { get; set; }
        public string FullName { get; set; }
        public string OrderInfo { get; set; }

        public string ErrorCode { get; set; }
        public string TransId { get; set; }
        public string Signature { get; set; } // Chữ ký MoMo gửi về
        public bool IsVerified { get; set; } // Trạng thái xác minh
    }
}
