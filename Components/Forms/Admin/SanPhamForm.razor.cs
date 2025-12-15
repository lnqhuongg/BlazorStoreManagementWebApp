using BlazorStoreManagementWebApp.DTOs.Admin.LoaiSanPham;
using BlazorStoreManagementWebApp.DTOs.Admin.NhaCungCap;
using BlazorStoreManagementWebApp.DTOs.Admin.SanPham;
using BlazorStoreManagementWebApp.Services.Implements;
using BlazorStoreManagementWebApp.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BlazorStoreManagementWebApp.Components.Forms.Admin
{
    public partial class SanPhamForm : ComponentBase
    {
        [Inject] private IJSRuntime JS { get; set; } = default!;
        [Inject] private ISanPhamService SanPhamService { get; set; }
        [Inject] public ITonKhoService TonKhoService { get; set; }

        //[Inject] private ImageService _imageService { get; set; }
        [Parameter] public EventCallback OnSuccess { get; set; }
        private SanPhamDTO productDTO = new();
        private List<LoaiSanPhamDTO> LSPData = new();
        private List<NhaCungCapDTO> NCCData = new();
        // xem(0) sua(1) them(2)
        private int Mode = 1;
        private string ModalTitle;
        private string BtnSubmit;
        private string NameErrorMessage = "";
        private IBrowserFile? selectedFile;
        private string? previewImage;
        protected int? stock = 0;

        private void getModalTitle()
        {
            if(Mode == 0)
            {
                ModalTitle = "Xem sản phẩm";
            } else if(Mode == 1)
            {
                ModalTitle = "Chỉnh sửa sản phẩm";
            } else
            {
                ModalTitle = "Thêm mới sản phẩm";
            }
        }
        private void getBtnSubmit()
        {
            if(Mode == 0)
            {
                BtnSubmit = "";
            } else if(Mode == 1)
            {
                BtnSubmit = "Cập nhật";
            } else
            {
                BtnSubmit = "Thêm mới";
            }
        }
        private async Task loadStock(int productId)
        {
            var tonKho = await TonKhoService.GetByProductID(productId);

            if (tonKho == null)
            {
                stock = 0;
                return;
            }

            stock = tonKho.Quantity;
        }

        private async Task OnImageSelected(InputFileChangeEventArgs e)
        {
            selectedFile = e.File;

            using var stream = selectedFile.OpenReadStream(5 * 1024 * 1024);
            var buffer = new byte[selectedFile.Size];
            await stream.ReadAsync(buffer);

            previewImage = $"data:{selectedFile.ContentType};base64,{Convert.ToBase64String(buffer)}";

            StateHasChanged(); // ⭐ BẮT BUỘC
        }

        
        public async Task OpenCreate(List<LoaiSanPhamDTO> LSPData, List<NhaCungCapDTO> NCCData)
        {
            Mode = 2;
            NameErrorMessage = "";
            getBtnSubmit();
            getModalTitle();
            productDTO = new SanPhamDTO();
            this.LSPData = LSPData;
            this.NCCData = NCCData;
            StateHasChanged(); // Đảm bảo UI cập nhật tiêu đề
            await JS.InvokeVoidAsync("showBootstrapModal", "addProductModal");
            await JS.InvokeAsync<object>("showToast", "success", "Thêm sản phẩm mới thành công!");
        }

        public async Task OpenUpdate(SanPhamDTO dto, List<LoaiSanPhamDTO> LSPData, List<NhaCungCapDTO> NCCData)
        {
            Mode = 1;
            getBtnSubmit();
            getModalTitle();
            productDTO = dto;
            await loadStock(dto.ProductID);
            this.LSPData = LSPData;
            this.NCCData = NCCData;
            NameErrorMessage = "";
            productDTO = dto;
            StateHasChanged(); // Đảm bảo UI cập nhật tiêu đề
            await JS.InvokeVoidAsync("showBootstrapModal", "addProductModal");
            await JS.InvokeAsync<object>("showToast", "success", "Cập nhật sản phẩm thành công!");
        }
        public async Task OpenView(SanPhamDTO dto, List<LoaiSanPhamDTO> LSPData, List<NhaCungCapDTO> NCCData)
        {
            Mode = 0;
            getBtnSubmit();
            getModalTitle();
            productDTO = dto;
            await loadStock(dto.ProductID);
            this.LSPData = LSPData;
            this.NCCData = NCCData;
            NameErrorMessage = "";
            productDTO = dto;
            StateHasChanged(); // Đảm bảo UI cập nhật tiêu đề
            await JS.InvokeVoidAsync("showBootstrapModal", "addProductModal");
        }

        private async Task<bool> ValidateForm()
        {
            NameErrorMessage = "";
            StateHasChanged(); // hiện loading nếu bạn muốn
            bool isValid = true;

            if (string.IsNullOrWhiteSpace(productDTO.ProductName))
            {
                NameErrorMessage = "Tên sản phẩm không được để trống!";
                isValid = false;
            }
            else if (!Regex.IsMatch(productDTO.ProductName, @"^[\p{L}0-9\s]+$"))
            {
                NameErrorMessage = "Tên sản phẩm chỉ được chứa chữ cái, số và khoảng trắng!";
                isValid = false;
            }
            else if (Regex.IsMatch(productDTO.ProductName, @"[!@#$%^&*(),.?""':{}|<>]"))
            {
                NameErrorMessage = "Tên sản phẩm không được chứa ký tự đặc biệt!";
                isValid = false;
            }
            if (productDTO.Price <= 0)
            {
                NameErrorMessage = "Giá sản phẩm phải lớn hơn 0!";
                isValid = false;
            }
            
            if (productDTO.CategoryID <= 0)
            {
                NameErrorMessage = "Vui lòng chọn loại sản phẩm!";
                isValid = false;
            }
            if (productDTO.SupplierID <= 0)
            {
                NameErrorMessage = "Vui lòng chọn nhà cung cấp!";
                isValid = false;
            }
            if(string.IsNullOrEmpty(productDTO.Unit))
            {
                NameErrorMessage = "Vui lòng chọn đơn vị tính!";
                isValid = false;
            }
            //if (!string.IsNullOrWhiteSpace(productDTO.Barcode) && !Regex.IsMatch(productDTO.Barcode, @"^\d+$"))
            //{
            //    NameErrorMessage = "Mã vạch chỉ được chứa chữ số!";
            //    isValid = false;
            //}
            if (isValid)
            {
                if (Mode == 2)
                {

                }
                else if(Mode == 1)
                {
                    bool isExist = await SanPhamService.checkBarcodeExistForOtherProducts(productDTO.ProductID, productDTO.Barcode);
                    if (isExist)
                    {
                        NameErrorMessage = "Mã vạch đã tồn tại cho sản phẩm khác!";
                        isValid = false;
                    }
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
            var requestDTO = new SanPhamRequestDTO
            {
                ProductName = productDTO.ProductName,
                Barcode = productDTO.Barcode,
                Price = productDTO.Price,
                Unit = productDTO.Unit,
                Status = productDTO.Status,
                CategoryID = productDTO.CategoryID,
                SupplierID = productDTO.SupplierID,
                ImageUrl = selectedFile
            };

            if (Mode == 1)
            {
                await SanPhamService.Update(productDTO.ProductID, requestDTO);
                await JS.InvokeAsync<object>("showToast", "success", "Cập nhật sản phẩm thành công!");
            }
            else if(Mode == 2)
            {
                await SanPhamService.Create(requestDTO);
                await JS.InvokeAsync<object>("showToast", "success", "Thêm sản phẩm mới thành công!");
            }

            await JS.InvokeVoidAsync("hideBootstrapModal", "addProductModal");
            await OnSuccess.InvokeAsync();
        }

    }
}