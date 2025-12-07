using BlazorStoreManagementWebApp.DTOs.Admin.MaGiamGia;
using BlazorStoreManagementWebApp.DTOs.Admin.ThanhToanDTO;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorStoreManagementWebApp.DTOs.Admin.DonHang
{
    public class DonHangDTO
    {
        public int OrderId { get; set; }

        public int? CustomerId { get; set; }

        public string? CustomerName { get; set; }
        public string? UserName { get; set; }

        //public NhanVienDTO User { get; set; }
        //public KhachHangDTO Customer { get; set; }

        public int? UserId { get; set; }

        public int? PromoId { get; set; }

        public MaGiamGiaDTO Promotion { get; set; }

        public DateTime? OrderDate { get; set; }

        public string? Status { get; set; }

        [Column("total_amount", TypeName = "decimal(18,2)")]
        public decimal? TotalAmount { get; set; }

        [Column("discount_amount", TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; }

        public List<ChiTietDonHangDTO>? Items { get; set; }

        public List<ThanhToanReturnDTO>? Payments { get; set; }
    }
}