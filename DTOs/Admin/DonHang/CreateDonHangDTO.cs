using BlazorStoreManagementWebApp.DTOs.Admin.ThanhToanDTO;

namespace BlazorStoreManagementWebApp.DTOs.Admin.DonHang
{
    public class CreateDonHangDTO
    {
        public int? CustomerId { get; set; }
        public int? UserId { get; set; }
        public int? PromoId { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public List<ChiTietDonHangDTO>? Items { get; set; }
        public List<CreateThanhToanDTO>? Payments { get; set; }

        public int? rewardPoints { get; set; }
    }
}
