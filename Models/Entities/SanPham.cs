using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorStoreManagementWebApp.Models.Entities
{
    [Table("products")]
    public class SanPham
    {
        [Key]
        [Column("product_id")]
        public int ProductID { get; set; }

        [Column("category_id")]
        public int? CategoryID { get; set; }
        [ForeignKey(nameof(CategoryID))]
        public LoaiSanPham? Category { get; set; }

        [Column("supplier_id")]
        public int? SupplierID { get; set; }
        [ForeignKey(nameof(SupplierID))]
        public NhaCungCap? Supplier { get; set; }

        [Required(ErrorMessage = "Tên sản phẩm là bắt buộc!")]
        [Column("product_name", TypeName = "varchar(100)")]
        [MaxLength(100, ErrorMessage = "Tên sản phẩm tối đa 100 kí tự!")]
        public string ProductName { get; set; }

        [Required(ErrorMessage = "Mã vạch là bắt buộc!")]
        [Column("barcode", TypeName = "varchar(50)")]
        [MaxLength(ErrorMessage = "Mã vạch tối đa 50 kí tự!")]
        public string Barcode { get; set; }

        [Required(ErrorMessage = "Đơn giá là bắt buộc!")]
        [Column("price", TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        [Column("unit", TypeName = "varchar(20)")]
        public string Unit { get; set; }

        [Column("created_at", TypeName = "timestamp")]
        public DateTime CreatedAt { get; set; }

        [Column("image_url", TypeName = "varchar(500)")] // Thêm trường này
        public string? ImageUrl { get; set; }

        //[Column("stock", TypeName = "int")]
        //public int? stock { get; set; }

        [Column("status", TypeName = "bit(1)")]
        public int Status { get; set; }

        public SanPham() { }
    }

}
