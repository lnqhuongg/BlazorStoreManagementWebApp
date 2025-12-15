using BlazorStoreManagementWebApp.DTOs.Admin.MaGiamGia;
using BlazorStoreManagementWebApp.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorStoreManagementWebApp.Components.Forms.Admin
{
    public partial class MaGiamGiaForm : ComponentBase
    {
        [Inject] private IMaGiamGiaService MaGiamGiaService { get; set; } = default!;
        [Inject] private IJSRuntime JS { get; set; } = default!;

        [Parameter] public EventCallback OnSuccess { get; set; }

        protected MaGiamGiaDTO promoDTO { get; set; } = new();
        protected bool IsEditMode { get; set; }

        // ===== ERROR FIELDS =====
        protected string PromoCodeError { get; set; } = "";
        protected string DiscountTypeError { get; set; } = "";
        protected string DiscountValueError { get; set; } = "";
        protected string StartDateError { get; set; } = "";
        protected string EndDateError { get; set; } = "";
        protected string MinOrderAmountError { get; set; } = "";
        protected string UsageLimitError { get; set; } = "";
        protected string StatusError { get; set; } = "";
        protected string DescriptionError { get; set; } = "";
        public string ModalTitle => IsEditMode ? "Chỉnh sửa mã giảm giá" : "Thêm mới mã giảm giá";

        public async Task OpenCreate()
        {
            promoDTO = new MaGiamGiaDTO
            {
                Status = "active",
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(1)
            };

            IsEditMode = false;
            ClearErrors();
            StateHasChanged();

            await JS.InvokeVoidAsync("showBootstrapModal", "PromoModal");
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
            ClearErrors();
            StateHasChanged();

            await JS.InvokeVoidAsync("showBootstrapModal", "PromoModal");
        }

        private void ClearErrors()
        {
            PromoCodeError =
            DiscountTypeError =
            DiscountValueError =
            StartDateError =
            EndDateError =
            MinOrderAmountError =
            UsageLimitError =
            StatusError = "";
        }

        private bool Validate()
        {
            ClearErrors();
            bool ok = true;

            if (string.IsNullOrWhiteSpace(promoDTO.PromoCode))
            {
                PromoCodeError = "Mã giảm giá không được để trống.";
                ok = false;
            }

            if (string.IsNullOrWhiteSpace(promoDTO.Description))
            {
                DescriptionError = "Mô tả không được để trống.";
                ok = false;
            }

            if (string.IsNullOrWhiteSpace(promoDTO.DiscountType))
            {
                DiscountTypeError = "Vui lòng chọn loại giảm.";
                ok = false;
            }

            if (promoDTO.DiscountValue <= 0)
            {
                DiscountValueError = "Giá trị giảm phải lớn hơn 0.";
                ok = false;
            }

            if (promoDTO.StartDate == default)
            {
                StartDateError = "Vui lòng chọn ngày bắt đầu.";
                ok = false;
            }
            else if (promoDTO.StartDate < DateTime.Today)
            {
                StartDateError = "Ngày bắt đầu phải từ hôm nay trở đi.";
                ok = false;
            }

            if (promoDTO.EndDate == default)
            {
                EndDateError = "Vui lòng chọn ngày kết thúc.";
                ok = false;
            }
            else if (promoDTO.EndDate < promoDTO.StartDate)
            {
                EndDateError = "Ngày kết thúc phải sau ngày bắt đầu.";
                ok = false;
            }

            if (promoDTO.MinOrderAmount < 0)
            {
                MinOrderAmountError = "Giá trị đơn tối thiểu không hợp lệ.";
                ok = false;
            }

            if (promoDTO.UsageLimit <= 0)
            {
                UsageLimitError = "Giới hạn sử dụng phải lớn hơn 0.";
                ok = false;
            }

            if (string.IsNullOrWhiteSpace(promoDTO.Status))
            {
                StatusError = "Vui lòng chọn trạng thái.";
                ok = false;
            }

            return ok;
        }


        protected async Task HandleSubmit()
        {
            if (!Validate()) return;

            bool isDuplicate = await MaGiamGiaService.isPromoCodeExist(
                promoDTO.PromoCode,
                IsEditMode ? promoDTO.PromoId : 0
            );

            if (isDuplicate)
            {
                PromoCodeError = "Mã giảm giá đã tồn tại.";
                return;
            }

            if (IsEditMode)
            {
                await MaGiamGiaService.Update(promoDTO.PromoId, promoDTO);
                await JS.InvokeVoidAsync("showToast", "success", "Cập nhật mã giảm giá thành công!");
            }
            else
            {
                await MaGiamGiaService.Create(promoDTO);
                await JS.InvokeVoidAsync("showToast", "success", "Thêm mã giảm giá mới thành công!");
            }

            await JS.InvokeVoidAsync("hideBootstrapModal", "PromoModal");
            await OnSuccess.InvokeAsync();
        }
    }
}
