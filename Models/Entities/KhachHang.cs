using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorStoreManagementWebApp.Models.Entities
{
    [Table("customers")]
    public class KhachHang
    {
        [Key] // khóa chính
        [Column("customer_id")]
        public int CustomerId { get; set; }

        [Required]
        [Column("name", TypeName = "varchar(100)")]
        public string Name { get; set; } = "";

        [Column("phone", TypeName = "varchar(20)")]
        public string? Phone { get; set; }

        [Column("email", TypeName = "varchar(100)")]
        public string? Email { get; set; }

        [Column("address")]
        public string? Address { get; set; }

        [Column("created_at", TypeName = "datetime")]
        public DateTime? CreatedAt { get; set; } = DateTime.Now;

        [Column("reward_points", TypeName = "int")]
        public int? RewardPoints { get; set; } = 0;
    }

}
