// Models/Entities/DonHang.cs
using BlazorStoreManagementWebApp.Models.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorStoreManagementWebApp.Models.Entities
{
    [Table("orders")]
    public class DonHang
    {
        [Key]
        [Column("order_id")]
        public int OrderId { get; set; }

        [Column("customer_id")]
        public int? CustomerId { get; set; }

        [ForeignKey(nameof(CustomerId))]
        public KhachHang? Customer { get; set; }

        [Column("user_id")]
        public int? UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public NhanVien? User { get; set; }

        [Column("promo_id")]
        public int? PromoId { get; set; }

        [ForeignKey(nameof(PromoId))]
        public MaGiamGia? Promotion { get; set; }

        [Column("order_date")]
        public DateTime? OrderDate { get; set; }

        [Column("status")]
        public string? Status { get; set; }

        [Column("total_amount")]
        public decimal? TotalAmount { get; set; }

        [Column("discount_amount")]
        public decimal DiscountAmount { get; set; }

        public List<ChiTietDonHang> Items { get; set; } = new();
        public List<ThanhToan> Payments { get; set; } = new();
    }
}
