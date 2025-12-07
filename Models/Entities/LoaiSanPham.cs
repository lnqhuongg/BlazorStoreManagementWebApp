using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorStoreManagementWebApp.Models.Entities
{
    [Table("categories")]
    public class LoaiSanPham
    {
        [Key] // khóa chính
        [Column("category_id")]
        public int CategoryId { get; set; }

        [Column("category_name", TypeName = "varchar(100")]
        public string CategoryName { get; set; } = "";

        public ICollection<SanPham> SanPhams { get; set; } = new List<SanPham>();
    }
}
