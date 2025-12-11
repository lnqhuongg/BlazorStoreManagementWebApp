using BlazorStoreManagementWebApp.DTOs.Admin.ChiTietPhieuNhap;
using BlazorStoreManagementWebApp.DTOs.Admin.NhaCungCap;
using BlazorStoreManagementWebApp.DTOs.Admin.PhieuNhap;
using BlazorStoreManagementWebApp.DTOs.Admin.SanPham;
using BlazorStoreManagementWebApp.Services.Implements;
using BlazorStoreManagementWebApp.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorStoreManagementWebApp.Components.Forms.Admin
{
    public partial class PhieuNhapForm : ComponentBase
    {
        [Inject] public IPhieuNhapService PhieuNhapService { get; set; }
        [Inject] public INhaCungCapService NhaCungCapService { get; set; }
        [Inject] public ISanPhamService SanPhamService { get; set; }

        [Inject] private IJSRuntime JS { get; set; } = default!;
        [Parameter] public EventCallback OnSuccess { get; set; } // Gọi lại LoadData ở trang cha

        private PhieuNhapDTO phieuNhapDTO = new();
        private List<NhaCungCapDTO> suppliers = new();
        private List<SanPhamDTO> products = new();
        private List<ChiTietPhieuNhapDTO> details = new();


        public PhieuNhapDTO FormModel { get; set; } = new();

        private int selectedSupplierId;
        private NhaCungCapDTO selectedSupplier = new NhaCungCapDTO();
        private int selectedProductId;
        private int quantity = 1;
        private decimal price = 0;
        private decimal TotalAmount => details.Sum(d => d.Quantity * d.Price);



        // này để báo lôĩ message validate
        private Dictionary<string, string> Errors = new([]);

        private bool IsAddMode = false;
        private bool IsViewMode = false;
        private string ModalTitle => IsAddMode ? "Chi tiết phiếu nhập" : "Thêm mới phiếu nhập";
        public async Task OpenCreate()
        {
            IsAddMode = true;
            IsViewMode = false;

            phieuNhapDTO = new PhieuNhapDTO();
            details = new List<ChiTietPhieuNhapDTO>();

            selectedSupplierId = 0;
            selectedProductId = 0;
            quantity = 1;
            price = 0;
            phieuNhapDTO = new PhieuNhapDTO();
            // load NCC
            suppliers = await NhaCungCapService.GetAllNCC();

            products.Clear();
            selectedSupplierId = 0;
            selectedProductId = 0;

            StateHasChanged(); // Đảm bảo UI cập nhật tiêu đề
            await JS.InvokeVoidAsync("showBootstrapModal", "PhieuNhapModal");

        }

        public async Task OpenDetail(PhieuNhapDTO input)
        {
            IsAddMode = false;
            IsViewMode = true;

            // Copy dữ liệu phiếu nhập
            phieuNhapDTO = new PhieuNhapDTO
            {
                ImportId = input.ImportId,
                Supplier = input.Supplier,
                ImportDate = input.ImportDate,
                Staff = input.Staff,
                TotalAmount = input.TotalAmount,
                ImportDetails = input.ImportDetails
                    .Select(x => new ChiTietPhieuNhapDTO
                    {
                        Product = x.Product,
                        Quantity = x.Quantity,
                        Price = x.Price
                    }).ToList()
            };

            // Copy qua danh sách chi tiết
            details = phieuNhapDTO.ImportDetails;

            selectedSupplier = input.Supplier;

            selectedSupplierId = input.Supplier.SupplierId;

            selectedProductId = 0;
            quantity = 1;
            price = 0;

            StateHasChanged();
            await JS.InvokeVoidAsync("showBootstrapModal", "PhieuNhapModal");
        }



        private async Task SupplierChanged(ChangeEventArgs e)
        {
            selectedSupplierId = int.Parse(e.Value.ToString());
            details.Clear();
            products = await SanPhamService.getBySupplierID(selectedSupplierId);
            selectedProductId = 0;

            StateHasChanged();
        }

        // them 1 sp vao details
        private void AddProduct()
        {
            Errors.Remove("Supplier");
            Errors.Remove("AddProduct");  // clear lỗi cũ
            Console.WriteLine("supplier id: " + selectedSupplierId);
            if (selectedSupplierId == 0)
            {
                Errors["Supplier"] = "Vui lòng chọn nhà cung cấp trước.";
                return;
            }
            if (selectedProductId == 0)
            {
                Errors["AddProduct"] = "Vui lòng chọn sản phẩm.";
                return;
            }
            if (quantity <= 0)
            {
                Errors["Quantity"] = "Số lượng phải lớn hơn 0.";
                return;
            }
            if (price <= 0)
            {
                Errors["Price"] = "Giá phải lớn hơn 0.";
                return;
            }

            var product = products.FirstOrDefault(x => x.ProductID == selectedProductId);
            if (product == null)
            {
                Errors["AddProduct"] = "Sản phẩm không hợp lệ.";
                return;
            }

            ChiTietPhieuNhapDTO ctpnDTO = new ChiTietPhieuNhapDTO();
            ctpnDTO.Product = product;
            ctpnDTO.Quantity = quantity;
            ctpnDTO.Price = price;
            ctpnDTO.Subtotal = quantity * price;

            details.Add(ctpnDTO);
            // reset form
            quantity = 1;
            price = 0;
            selectedProductId = 0;
        }

        // luu phieu nhap
        public async Task Save()
        {
            if (!ValidateForm())
            {
                Console.WriteLine("Validation failed:");
                StateHasChanged();
                return;
            }
            Console.WriteLine("Validation succeeded.");

            // Đúng form → submit
            CreatePhieuNhapDTO phieuNhapDTO = new CreatePhieuNhapDTO();
            phieuNhapDTO.UserId = 1; // TODO: Lấy user hiện tại đăng nhập
            phieuNhapDTO.SupplierId = selectedSupplierId;
            phieuNhapDTO.ImportDetails = details.Select(d => new CreateChiTietPNDTO
            {
                ProductId = d.Product.ProductID,
                Quantity = d.Quantity,
                Price = (int)d.Price
            }).ToList();
            await PhieuNhapService.CreateWithDetails(phieuNhapDTO);

            await JS.InvokeVoidAsync("hideBootstrapModal", "PhieuNhapModal");
            await OnSuccess.InvokeAsync();
            ShowSuccess("Lưu phiếu nhập thành công!");
        }


        private void RemoveImportDetail(int id)
        {
            details.RemoveAll(x => x.Product.ProductID == id);
        }

        private bool ValidateForm()
        {
            Errors.Clear();
            // Chi tiết rỗng
            if (details.Count == 0)
                Errors["Details"] = "Vui lòng thêm ít nhất một sản phẩm vào phiếu nhập.";

            // Kiểm tra từng detail
            foreach (var item in details)
            {
                if (item.Quantity <= 0)
                {
                    Errors["DetailQuantity"] = "Số lượng phải lớn hơn 0.";
                    break;
                }
                if (item.Price <= 0)
                {
                    Errors["DetailPrice"] = "Giá phải lớn hơn 0.";
                    break;
                }
            }

            return Errors.Count == 0;
        }

        protected bool ShowToast = false;
        protected string ToastMessage = "";

        protected void ShowSuccess(string message)
        {
            ToastMessage = message;
            ShowToast = true;
            InvokeAsync(StateHasChanged);

            // Auto hide 3s
            _ = Task.Run(async () =>
            {
                await Task.Delay(3000);
                ShowToast = false;
                await InvokeAsync(StateHasChanged);
            });
        }

        protected void HideToast()
        {
            ShowToast = false;
            StateHasChanged();
        }
    }
}
