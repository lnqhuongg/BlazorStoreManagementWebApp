using System.ComponentModel.DataAnnotations;

namespace BlazorStoreManagementWebApp.DTOs.Admin.KhachHang
{
    public class KhachHangDTO
    {
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Tên khách hàng là bắt buộc")]
        public string Name { get; set; } = "";

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string? Phone { get; set; }

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string? Email { get; set; }

        public string? Address { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int RewardPoints { get; set; }
    }
}
