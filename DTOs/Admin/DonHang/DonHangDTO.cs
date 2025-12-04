namespace BlazorStoreManagementWebApp.DTOs.Admin.DonHang
{
    public class DonHangDTO
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; } = "Khách vãng lai";
        public DateTime? OrderDate { get; set; }
        public decimal? TotalAmount { get; set; }
        public string PaymentStatus { get; set; } = "Chưa thanh toán";
    }
}