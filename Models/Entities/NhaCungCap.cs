using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorStoreManagementWebApp.Models.Entities
{
    [Table("suppliers")]
    public class NhaCungCap
    {
        [Key] // khóa chính
        [Column("supplier_id")]
        public int SupplierId { get; set; }

        [Required]
        [Column("name", TypeName = "varchar(100)")]
        public string Name { get; set; } = "";

        [Required]
        [Column("phone", TypeName = "varchar(20)")]
        public string Phone { get; set; } = "";

        [Required]
        [Column("email", TypeName = "varchar(100)")]
        public string Email { get; set; } = "";

        [Required]
        [Column("address", TypeName = "text")]
        public string Address { get; set; } = "";

        [Required]
        [Column("status", TypeName = "bit(1)")]
        public int Status { get; set; }
        public ICollection<SanPham> SanPhams { get; set; } = new List<SanPham>();
    }

}
