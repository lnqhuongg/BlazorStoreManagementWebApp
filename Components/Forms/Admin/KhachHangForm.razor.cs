using BlazorStoreManagementWebApp.DTOs.Admin.KhachHang;
using BlazorStoreManagementWebApp.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Text.RegularExpressions;

namespace BlazorStoreManagementWebApp.Components.Forms.Admin
{
    public partial class KhachHangForm : ComponentBase
    {
        [Inject] private IKhachHangService KhachHangService { get; set; } = default!;
        [Inject] private IJSRuntime JS { get; set; } = default!;

        [Parameter] public EventCallback OnSuccess { get; set; }

        private KhachHangDTO customerDTO = new();
        private string NameErrorMessage = "";
        private string AddressErrorMessage = "";

        public async Task OpenUpdate(KhachHangDTO dto)
        {
            customerDTO = new KhachHangDTO
            {
                CustomerId = dto.CustomerId,
                Name = dto.Name?.Trim() ?? "",
                Phone = dto.Phone,
                Email = dto.Email,
                Address = dto.Address?.Trim(),
                RewardPoints = dto.RewardPoints
            };

            // Reset lỗi khi mở modal
            NameErrorMessage = "";
            AddressErrorMessage = "";
            StateHasChanged();

            await JS.InvokeVoidAsync("showBootstrapModal", "KhachHangModal");
        }

        private async Task<bool> ValidateForm()
        {
            NameErrorMessage = "";
            AddressErrorMessage = "";
            bool isValid = true;

            // 1. Validate Tên khách hàng
            if (string.IsNullOrWhiteSpace(customerDTO.Name))
            {
                NameErrorMessage = "Tên khách hàng không được để trống!";
                isValid = false;
            }
            else if (customerDTO.Name.Length > 100)
            {
                NameErrorMessage = "Tên khách hàng không được quá 100 ký tự!";
                isValid = false;
            }
            else if (!Regex.IsMatch(customerDTO.Name, @"^[\p{L}\s0-9'\-\.\(\)]+$"))
            {
                NameErrorMessage = "Tên khách hàng chỉ được chứa chữ cái, số và một số ký tự cơ bản!";
                isValid = false;
            }

            // 2. Validate Địa chỉ (không bắt buộc nhưng nếu nhập thì giới hạn)
            if (!string.IsNullOrWhiteSpace(customerDTO.Address))
            {
                if (customerDTO.Address.Length > 500)
                {
                    AddressErrorMessage = "Địa chỉ không được vượt quá 500 ký tự!";
                    isValid = false;
                }
            }

            StateHasChanged();
            return isValid;
        }

        private async Task HandleSubmit()
        {
            if (!await ValidateForm())
                return;

            try
            {
                await KhachHangService.Update(customerDTO.CustomerId, customerDTO);
                await JS.InvokeVoidAsync("hideBootstrapModal", "KhachHangModal");
                await JS.InvokeAsync<object>("showToast", "success", "Cập nhật khách hàng thành công!");
                await OnSuccess.InvokeAsync();
            }
            catch (Exception ex)
            {
                NameErrorMessage = "Cập nhật thất bại: " + ex.Message;
                StateHasChanged();
            }
        }
    }
}