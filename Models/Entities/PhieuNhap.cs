using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorStoreManagementWebApp.Models.Entities
{
    [Table("imports")]
    public class PhieuNhap
    {
        [Key]
        [Column("import_id")]
        public int ImportId { get; set; }

        [Required]
        [Column("import_date")]
        public DateTime ImportDate { get; set; }

        [Required]
        [Column("supplier_id")]
        public int SupplierId { get; set; }

        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        [Required]
        [Column("total_amount", TypeName = "decimal(10,2)")]
        public decimal TotalAmount { get; set; }

        [ForeignKey(nameof(UserId))]
        public NhanVien? Staff { get; set; }

        [ForeignKey(nameof(SupplierId))]
        public NhaCungCap? Supplier { get; set; }

        public ICollection<ChiTietPhieuNhap>? ImportDetails { get; set; }
    }

}
