using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorStoreManagementWebApp.Models.Entities
{
    [Table("payments")]
    public class ThanhToan
    {
        [Key]
        [Column("payment_id")]
        public int PaymentId { get; set; }

        [Column("order_id")]
        public int OrderId { get; set; }
        [ForeignKey(nameof(OrderId))]
        public DonHang? Order { get; set; }

        [Required]
        [Column("amount")]
        public decimal Amount { get; set; }

        [Required]
        [Column("payment_method")]
        public string PaymentMethod { get; set; } = null!;

        [Required]
        [Column("payment_date", TypeName = "timestamp")]
        public DateTime PaymentDate { get; set; }
    }

}
