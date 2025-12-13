using BlazorStoreManagementWebApp.Components.Forms.Admin;
using BlazorStoreManagementWebApp.DTOs.Admin.DonHang;
using BlazorStoreManagementWebApp.Helpers; // Chứa PagedResult
using BlazorStoreManagementWebApp.Services.Interfaces;
using Microsoft.AspNetCore.Components;

namespace BlazorStoreManagementWebApp.Components.Pages.Admin
{
    public partial class DonHang : ComponentBase
    {
        [Inject] public IDonHangService DonHangService { get; set; } = default!;

        // Dùng PagedResult giống PhieuNhapData
        protected PagedResult<DonHangDTO> DonHangData = new();

        // Các biến phân trang
        protected int Page = 1;
        protected int PageSize = 5; // Giống PageSize bên Phiếu nhập

        // Biến bộ lọc
        private DonHangFilterDTO InputFilter { get; set; } = new DonHangFilterDTO();

        // Modal
        private ChiTietDonHangForm? donHangForm; // Đặt tên Ref giống style của bạn

        protected override async Task OnInitializedAsync()
        {
            await LoadData();
        }

        protected async Task LoadData()
        {
            // Gọi Service lấy dữ liệu
            DonHangData = await DonHangService.GetAll(Page, PageSize, InputFilter);
        }

        protected async Task ChangePage(int newPage)
        {
            Page = newPage;
            await LoadData();
        }

        // --- HÀM SEARCH (Y HỆT PHIẾU NHẬP) ---
        private async Task Search()
        {
            Page = 1; // Reset về trang 1 khi lọc
            await LoadData();
        }

        // --- HÀM CLEAR FILTER (Y HỆT PHIẾU NHẬP) ---
        protected async Task ClearFilter()
        {
            InputFilter = new DonHangFilterDTO(); // Reset object filter
            await LoadData();
        }

        // Mở Modal xem chi tiết
        public async Task OpenDetail(int orderId)
        {
            // Kiểm tra biến donHangForm (chữ d viết thường)
            if (donHangForm != null)
            {
                await donHangForm.Show(orderId);
            }
            else
            {
                Console.WriteLine("Lỗi: Form chưa được khởi tạo (biến donHangForm bị null)");
            }
        }
    }
}