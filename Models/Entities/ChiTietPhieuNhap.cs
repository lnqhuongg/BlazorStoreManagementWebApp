using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace BlazorStoreManagementWebApp.Models.Entities
{
    [Table("import_details")]
    public class ChiTietPhieuNhap
    {
        [Key]
        [Column("import_detail_id")]
        public int ImportDetailId { get; set; }

        [Required]
        [Column("import_id")]
        public int ImportId { get; set; }

        [Required]
        [Column("product_id")]
        public int ProductId { get; set; }

        [Required]
        [Column("quantity")]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Subtotal { get; set; }

        [Required]
        [ForeignKey(nameof(ProductId))]
        public SanPham Product { get; set; }

        [JsonIgnore]
        [Required]
        [ForeignKey(nameof(ImportId))]
        public PhieuNhap Import { get; set; }

    }
}
