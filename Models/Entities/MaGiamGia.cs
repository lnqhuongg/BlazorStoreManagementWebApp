using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorStoreManagementWebApp.Models.Entities
{
    [Table("promotions")]
    public class MaGiamGia
    {
        [Key]
        [Column("promo_id")]
        public int PromoId { get; set; }

        [Required]
        [Column("promo_code", TypeName = "varchar(50)")]
        public string PromoCode { get; set; } = "";

        [Column("description", TypeName = "varchar(255)")]
        public string? Description { get; set; }

        [Required]
        [Column("discount_type", TypeName = "enum('percent','fixed')")]
        public string DiscountType { get; set; } = "";

        [Required]
        [Column("discount_value", TypeName = "decimal(10,2)")]
        public decimal DiscountValue { get; set; }

        [Required]
        [Column("start_date", TypeName = "date")]
        public DateTime StartDate { get; set; }

        [Required]
        [Column("end_date", TypeName = "date")]
        public DateTime EndDate { get; set; }

        [Column("min_order_amount", TypeName = "decimal(10,2)")]
        public decimal MinOrderAmount { get; set; } = 0;

        [Column("usage_limit")]
        public int UsageLimit { get; set; } = 0;

        [Column("used_count")]
        public int UsedCount { get; set; } = 0;

        [Column("status", TypeName = "enum('active','inactive')")]
        public string Status { get; set; } = "active";
    }

}
