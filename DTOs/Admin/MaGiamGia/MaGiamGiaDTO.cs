namespace BlazorStoreManagementWebApp.DTOs.Admin.MaGiamGia
{
    public class MaGiamGiaDTO
    {
        public int PromoId { get; set; }
        public string PromoCode { get; set; } = "";
        public string? Description { get; set; }
        public string DiscountType { get; set; } = "";
        public decimal DiscountValue { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal MinOrderAmount { get; set; } = 0;
        public int UsageLimit { get; set; } = 0;
        public int UsedCount { get; set; } = 0;
        public string Status { get; set; } = "active";
    }
}
