using BlazorStoreManagementWebApp.DTOs.Admin.MaGiamGia;
using BlazorStoreManagementWebApp.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Text.RegularExpressions;

namespace BlazorStoreManagementWebApp.Components.Forms.Admin
{
    public partial class MaGiamGiaForm : ComponentBase
    {
        [Inject] private IMaGiamGiaService MaGiamGiaService { get; set; } = default!;
        [Inject] private IJSRuntime JS { get; set; } = default!;
        [Parameter] public EventCallback OnSuccess { get; set; }

        private MaGiamGiaDTO promoDTO = new();
        private bool IsEditMode = false;
        private string ModalTitle => IsEditMode ? "Chỉnh sửa mã giảm giá" : "Thêm mới mã giảm giá";
        private string CodeErrorMessage = "";

        public async Task OpenCreate()
        {
            promoDTO = new MaGiamGiaDTO();
            IsEditMode = false;
            CodeErrorMessage = "";
            StateHasChanged();
            await JS.InvokeVoidAsync("showBootstrapModal", "PromoModal");
            await JS.InvokeAsync<object>("showToast", "success", "Thêm mã giảm giá mới thành công!");
        }

        public async Task OpenUpdate(MaGiamGiaDTO dto)
        {
            promoDTO = new MaGiamGiaDTO
            {
                PromoId = dto.PromoId,
                PromoCode = dto.PromoCode,
                Description = dto.Description,
                DiscountType = dto.DiscountType,
                DiscountValue = dto.DiscountValue,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                MinOrderAmount = dto.MinOrderAmount,
                UsageLimit = dto.UsageLimit,
                UsedCount = dto.UsedCount,
                Status = dto.Status
            };
            IsEditMode = true;
            CodeErrorMessage = "";
            StateHasChanged();
            await JS.InvokeVoidAsync("showBootstrapModal", "PromoModal");
            await JS.InvokeAsync<object>("showToast", "success", "Cập nhật mã giảm giá thành công!");
        }

        private async Task<bool> ValidateForm()
        {
            CodeErrorMessage = "";
            StateHasChanged();

            bool isValid = true;

            if (string.IsNullOrWhiteSpace(promoDTO.PromoCode))
            {
                CodeErrorMessage = "Mã giảm giá không được để trống!";
                isValid = false;
            }
            else if (!Regex.IsMatch(promoDTO.PromoCode, @"^[A-Za-z0-9]+$"))
            {
                CodeErrorMessage = "Mã giảm giá chỉ được chứa chữ và số!";
                isValid = false;
            }

            if (isValid)
            {
                var promos = await MaGiamGiaService.SearchByKeyword(promoDTO.PromoCode.Trim());
                bool codeExists = promos.Any(x => x.PromoCode == promoDTO.PromoCode && (!IsEditMode || x.PromoId != promoDTO.PromoId));
                if (codeExists)
                {
                    CodeErrorMessage = "Mã giảm giá này đã tồn tại!";
                    isValid = false;
                }
            }

            StateHasChanged();
            return isValid;
        }

        private async Task HandleSubmit()
        {
            if (!await ValidateForm())
            {
                return;
            }

            try
            {
                if (IsEditMode)
                {
                    await MaGiamGiaService.Update(promoDTO.PromoId, promoDTO);
                }
                else
                {
                    await MaGiamGiaService.Create(promoDTO);
                }

                await JS.InvokeVoidAsync("hideBootstrapModal", "PromoModal");
                await OnSuccess.InvokeAsync();
            }
            catch (Exception ex)
            {
                await JS.InvokeVoidAsync("alert", $"Lỗi: {ex.Message}");
            }
        }
    }
}
