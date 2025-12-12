using BlazorStoreManagementWebApp.Components.Forms.Admin;
using BlazorStoreManagementWebApp.DTOs.Admin.DonHang;
using BlazorStoreManagementWebApp.Services.Interfaces;
using Microsoft.AspNetCore.Components;

namespace BlazorStoreManagementWebApp.Components.Pages.Admin
{
    public partial class DonHang : ComponentBase
    {
        [Inject]
        public IDonHangService _donHangService { get; set; } = default!;

        // 1. Biến dữ liệu danh sách
        public List<DonHangDTO> DonHangs { get; set; } = new List<DonHangDTO>();

        // 2. Biến tham chiếu Modal
        private ChiTietDonHangForm? donHangForm;

        // 3. CÁC BIẾN PHÂN TRANG & TÌM KIẾM
        public string Keyword { get; set; } = "";
        public string StatusFilter { get; set; } = "";

        // Các biến dùng cho PaginationAdmin
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalItems { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await LoadData();
        }

        // --- HÀM LOAD DATA ---
        public async Task LoadData()
        {
            // Gọi Service với các tham số tìm kiếm, lọc và phân trang
            var result = await _donHangService.GetAll(CurrentPage, PageSize, Keyword, StatusFilter);

            if (result != null)
            {
                DonHangs = result.Data;
                TotalItems = result.Total; // Cập nhật tổng số để PaginationAdmin tính toán số trang
            }
        }

        // --- XỬ LÝ SỰ KIỆN TÌM KIẾM ---
        public async Task HandleSearch()
        {
            // Reset về trang 1 khi tìm kiếm mới
            CurrentPage = 1;
            await LoadData();
        }

        // --- XỬ LÝ SỰ KIỆN LỌC TRẠNG THÁI ---
        public async Task HandleStatusFilter(ChangeEventArgs e)
        {
            StatusFilter = e.Value?.ToString() ?? "";
            CurrentPage = 1; // Reset về trang 1 khi đổi bộ lọc
            await LoadData();
        }

        // --- XỬ LÝ CHUYỂN TRANG (Callback cho PaginationAdmin) ---
        public async Task ChangePage(int page)
        {
            CurrentPage = page;
            await LoadData();
        }

        // --- MỞ MODAL ---
        public async Task OpenModal(int orderId)
        {
            if (donHangForm != null)
            {
                await donHangForm.Show(orderId);
            }
        }
    }
}