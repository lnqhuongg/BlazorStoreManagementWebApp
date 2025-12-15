using BlazorStoreManagementWebApp.DTOs.Admin.NhanVien;
using BlazorStoreManagementWebApp.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Text.RegularExpressions;

namespace BlazorStoreManagementWebApp.Components.Forms.Admin
{
    public partial class NhanVienForm : ComponentBase
    {
        [Inject] private INhanVienService NhanVienService { get; set; } = default!;
        [Inject] private IJSRuntime JS { get; set; } = default!;

        [Parameter] public EventCallback OnSuccess { get; set; }

        private NhanVienDTO Model = new();
        private bool IsEditMode = false;

        private string UsernameError = "";
        private string PasswordError = "";
        private string FullNameError = "";
        private string GeneralError = "";

        public async Task OpenCreate()
        {
            Model = new NhanVienDTO { Role = "staff", Status = 1 };
            IsEditMode = false;
            ClearErrors();
            StateHasChanged();
            await JS.InvokeVoidAsync("showBootstrapModal", "NhanVienModal");
           
        }

        public async Task OpenUpdate(NhanVienDTO dto)
        {
            Model = new NhanVienDTO
            {
                UserId = dto.UserId,
                Username = dto.Username,
                FullName = dto.FullName,
                Role = dto.Role,
                Status = dto.Status,
                CreatedAt = dto.CreatedAt
            };
            IsEditMode = true;
            ClearErrors();
            StateHasChanged();
            await JS.InvokeVoidAsync("showBootstrapModal", "NhanVienModal");
            
        }

        private void ClearErrors()
        {
            UsernameError = PasswordError = FullNameError = GeneralError = "";
        }

        private async Task<bool> ValidateForm()
        {
            ClearErrors();
            bool isValid = true;

            // Username
            if (string.IsNullOrWhiteSpace(Model.Username))
            {
                UsernameError = "Tên đăng nhập không được để trống!";
                isValid = false;
            }
            else if (Model.Username.Length < 4)
            {
                UsernameError = "Tên đăng nhập phải ít nhất 4 ký tự!";
                isValid = false;
            }
            else if (!Regex.IsMatch(Model.Username, @"^[a-zA-Z0-9_]+$"))
            {
                UsernameError = "Tên đăng nhập chỉ được chứa chữ cái, số và dấu gạch dưới!";
                isValid = false;
            }

            // Password (only for create)
            if (!IsEditMode)
            {
                if (string.IsNullOrWhiteSpace(Model.Password))
                {
                    PasswordError = "Mật khẩu không được để trống!";
                    isValid = false;
                }
                else if (Model.Password.Length < 6)
                {
                    PasswordError = "Mật khẩu phải ít nhất 6 ký tự!";
                    isValid = false;
                }
            }

            // FullName
            if (string.IsNullOrWhiteSpace(Model.FullName))
            {
                FullNameError = "Họ và tên không được để trống!";
                isValid = false;
            }

            // Check duplicate username
            if (isValid)
            {
                bool usernameExists = await NhanVienService.isUsernameExist(Model.Username);
                if (usernameExists && (!IsEditMode || Model.Username != await GetOriginalUsername()))
                {
                    UsernameError = "Tên đăng nhập đã tồn tại!";
                    isValid = false;
                }
            }

            StateHasChanged();
            return isValid;
        }

        private async Task<string> GetOriginalUsername()
        {
            if (IsEditMode && Model.UserId > 0)
            {
                var original = await NhanVienService.GetById(Model.UserId);
                return original?.Username ?? "";
            }
            return "";
        }

        private async Task HandleSubmit()
        {
            if (!await ValidateForm()) return;

            try
            {
                if (IsEditMode)
                {
                    await NhanVienService.Update(Model.UserId, Model);
                    await JS.InvokeAsync<object>("showToast", "success", "Cập nhật nhân viên thành công!");
                }
                else
                {
                    await NhanVienService.Create(Model);
                    await JS.InvokeAsync<object>("showToast", "success", "Thêm nhân viên mới thành công!");
                }

                await JS.InvokeVoidAsync("hideBootstrapModal", "NhanVienModal");
                await OnSuccess.InvokeAsync();
            }
            catch (Exception ex)
            {
                GeneralError = ex.Message;
                StateHasChanged();
            }
        }
    }
}