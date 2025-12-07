using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorStoreManagementWebApp.Models.Entities
{
    [Table("users")]
    public class NhanVien
    {
        [Key]
        [Column("user_id")]
        public int UserId { get; set; }

        [Required]
        [Column("username", TypeName = "varchar(50)")]
        public string Username { get; set; } = "";

        [Required]
        [Column("password", TypeName = "varchar(255)")]
        public string Password { get; set; } = "";

        [Required]
        [Column("full_name", TypeName = "nvarchar(100)")]
        public string FullName { get; set; } = "";

        [Required]
        [Column("role", TypeName = "varchar(20)")]
        public string Role { get; set; } = "staff"; // admin, staff

        [Column("created_at", TypeName = "datetime")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("status", TypeName = "tinyint")]
        public byte Status { get; set; } = 1; // 0: khóa, 1: hoạt động
    }

}
