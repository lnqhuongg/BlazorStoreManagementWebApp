using BlazorStoreManagementWebApp.DTOs.Admin.NhanVien;
using BlazorStoreManagementWebApp.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Blazored.SessionStorage;

namespace BlazorStoreManagementWebApp.Components.Forms.Admin
{
    public partial class ThongTinCaNhanForm
    {
        [Parameter] public EventCallback OnSaved { get; set; }

        private NhanVienDTO Model { get; set; } = new();
        private string NewPassword { get; set; } = "";
        private string ConfirmPassword { get; set; } = "";
        private string PasswordError { get; set; } = "";
        private string SuccessMessage { get; set; } = "";
        private string ErrorMessage { get; set; } = "";

        private int CurrentUserId;
        private bool IsCurrentUserAdmin = false;

        public async Task OpenModal()
        {
            SuccessMessage = "";
            ErrorMessage = "";
            PasswordError = "";

            try
            {
                var idStr = await SessionStorage.GetItemAsync<string>("adminId");
                if (!int.TryParse(idStr, out CurrentUserId))
                {
                    ErrorMessage = "Không thể lấy thông tin người dùng hiện tại.";
                    StateHasChanged();
                    return;
                }

                var role = await SessionStorage.GetItemAsync<string>("adminRole");
                IsCurrentUserAdmin = string.Equals(role, "admin", StringComparison.OrdinalIgnoreCase);

                var user = await NhanVienService.GetById(CurrentUserId);
                if (user == null)
                {
                    ErrorMessage = "Không tìm thấy thông tin tài khoản.";
                    StateHasChanged();
                    return;
                }

                Model = user;
                NewPassword = "";
                ConfirmPassword = "";

                StateHasChanged();

                // Quan trọng: KHÔNG có dấu # ở đây
                await JS.InvokeVoidAsync("showBootstrapModal", "ThongTinCaNhanModal");
            }
            catch (Exception ex)
            {
                ErrorMessage = "Lỗi khi tải thông tin: " + ex.Message;
                StateHasChanged();
            }
        }

        private async Task HandleSubmit()
        {
            PasswordError = "";
            SuccessMessage = "";
            ErrorMessage = "";

            if (!string.IsNullOrEmpty(NewPassword))
            {
                if (NewPassword.Length < 6)
                {
                    PasswordError = "Mật khẩu phải có ít nhất 6 ký tự.";
                    return;
                }
                if (NewPassword != ConfirmPassword)
                {
                    PasswordError = "Xác nhận mật khẩu không khớp.";
                    return;
                }

                // GÁN MẬT KHẨU MỚI VÀO DTO QUA TRƯỜNG NewPassword
                Model.NewPassword = NewPassword.Trim();
            }
            else
            {
                Model.NewPassword = null; // Không đổi
            }

            try
            {
                var updated = await NhanVienService.Update(CurrentUserId, Model);
                if (updated == null)
                {
                    ErrorMessage = "Cập nhật thất bại.";
                    return;
                }

                await SessionStorage.SetItemAsync("adminName", Model.FullName);

                SuccessMessage = "Cập nhật thông tin cá nhân thành công!";
                if (!string.IsNullOrEmpty(NewPassword))
                {
                    SuccessMessage += " Mật khẩu đã được thay đổi thành công!";
                }

                await OnSaved.InvokeAsync();

                // Reset ô mật khẩu
                NewPassword = "";
                ConfirmPassword = "";
                StateHasChanged();

                // Đóng modal
                await Task.Delay(1500);
                await JS.InvokeVoidAsync("hideBootstrapModal", "ThongTinCaNhanModal");
            }
            catch (Exception ex)
            {
                ErrorMessage = "Lỗi: " + (ex.Message.Contains("!") ? ex.Message : "Có lỗi xảy ra khi lưu.");
            }
        }
    }
}