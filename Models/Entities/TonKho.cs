using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorStoreManagementWebApp.Models.Entities
{
    [Table("inventory")]
    public class TonKho
    {
        [Key]
        [Column("inventory_id")]
        public int InventoryId { get; set; }

        [Required]
        [Column("product_id")]
        public int ProductId { get; set; }
        [ForeignKey(nameof(ProductId))]
        public SanPham? Product { get; set; }

        [Required]
        [Column("quantity")]
        public int Quantity { get; set; }

        [Required]
        [Column("updated_at", TypeName = "timestamp")]
        public DateTime UpdatedAt { get; set; }
    }

}
