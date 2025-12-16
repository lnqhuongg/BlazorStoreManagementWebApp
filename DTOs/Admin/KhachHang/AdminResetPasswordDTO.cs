using System.ComponentModel.DataAnnotations;

namespace BlazorStoreManagementWebApp.DTOs.Admin.KhachHang
{
    public class AdminResetPasswordDTO
    {
        [Required(ErrorMessage = "Mật khẩu mới là bắt buộc")]
        [MinLength(6, ErrorMessage = "Mật khẩu mới phải có ít nhất 6 ký tự")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu mới")]
        [Compare("NewPassword", ErrorMessage = "Xác nhận mật khẩu không khớp")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}