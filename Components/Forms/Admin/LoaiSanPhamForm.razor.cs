using BlazorStoreManagementWebApp.DTOs.Admin.LoaiSanPham;
using BlazorStoreManagementWebApp.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Text.RegularExpressions;

namespace BlazorStoreManagementWebApp.Components.Forms.Admin
{
    public partial class LoaiSanPhamForm : ComponentBase
    {
        //tiêm vào interface service để gọi service
        [Inject] private ILoaiSanPhamService LoaiSanPhamService { get; set; } = default!;
        // tiêm vào JSRuntime để gọi modal
        [Inject] private IJSRuntime JS { get; set; } = default!;
        [Parameter] public EventCallback OnSuccess { get; set; } // Gọi lại LoadData ở trang cha

        private LoaiSanPhamDTO categoryDTO = new();
        private bool IsEditMode = false;
        private string ModalTitle => IsEditMode ? "Chỉnh sửa loại sản phẩm" : "Thêm mới loại sản phẩm";

        // này để báo lôĩ message validate
        private string NameErrorMessage = "";

        // Hàm mở modal Thêm
        public async Task OpenCreate()
        {
            categoryDTO = new LoaiSanPhamDTO();
            IsEditMode = false;
            NameErrorMessage = "";
            StateHasChanged(); // Đảm bảo UI cập nhật tiêu đề
            await JS.InvokeVoidAsync("showBootstrapModal", "CategoryModal");
        }

        public async Task OpenUpdate(LoaiSanPhamDTO dto)
        {
            categoryDTO = new LoaiSanPhamDTO
            {
                CategoryId = dto.CategoryId,
                CategoryName = dto.CategoryName
            };
            IsEditMode = true;
            NameErrorMessage = "";
            StateHasChanged(); // Cập nhật tiêu đề + hiện phần trạng thái
            await JS.InvokeVoidAsync("showBootstrapModal", "CategoryModal");
        }

        private async Task<bool> ValidateForm()
        {
            // Reset lỗi cũ
            NameErrorMessage = "";
            StateHasChanged(); // hiện loading nếu bạn muốn

            bool isValid = true;

            // 1. Validate cơ bản (trống, độ dài, ký tự đặc biệt)
            if (string.IsNullOrWhiteSpace(categoryDTO.CategoryName))
            {
                NameErrorMessage = "Tên loại sản phẩm không được để trống!";
                isValid = false;
            }
            else if (!Regex.IsMatch(categoryDTO.CategoryName, @"^[a-zA-Z0-9ÀÁÂÃÈÉÊÌÍÒÓÔÕÙÚĂĐĨŨƠàáâãèéêìíòóôõùúăđĩũơƯĂẠẢẤẦẨẪẬẮẰẲẴẶẸẺẼỀỀỂưăạảấầẩẫậắằẳẵặẹẻẽềềểỄỆỈỊỌỎỐỒỔỖỘỚỜỞỠỢỤỦỨỪỬỮỰỲỴÝỶỸÝ\s]+$"))
            {
                NameErrorMessage = "Tên loại sản phẩm không được chứa ký tự đặc biệt!";
                isValid = false;
            }

            // check trùng tên (viết ở service nha)
            if (isValid)
            {
                // gọi hàm để kiểm tra tên đã tồn tại chưa
                bool nameExists = await LoaiSanPhamService.isCategoryNameExist(
                    categoryDTO.CategoryName.Trim(),
                    IsEditMode ? categoryDTO.CategoryId : 0
                );

                if (nameExists)
                {
                    NameErrorMessage = "Tên loại sản phẩm này đã tồn tại!";
                    isValid = false;
                }
            }

            StateHasChanged(); // cập nhật UI (hiện lỗi đỏ)
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
                    await LoaiSanPhamService.Update(categoryDTO.CategoryId, categoryDTO);
                }
                else
                {
                    await LoaiSanPhamService.Create(categoryDTO);
                }

                await JS.InvokeVoidAsync("hideBootstrapModal", "CategoryModal");
                await OnSuccess.InvokeAsync();
            }
            catch (Exception ex)
            {
                await JS.InvokeVoidAsync("alert", $"Lỗi: {ex.Message}");
            }
        }
    }
}
