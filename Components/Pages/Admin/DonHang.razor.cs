using BlazorStoreManagementWebApp.Components.Forms.Admin;
using BlazorStoreManagementWebApp.DTOs.Admin.DonHang;
using BlazorStoreManagementWebApp.DTOs.Admin.PhieuNhap;
using BlazorStoreManagementWebApp.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorStoreManagementWebApp.Components.Pages.Admin
{
    public partial class DonHang : ComponentBase
    {
        [Inject] private IJSRuntime JS { get; set; } = default!;
        [Inject] private PdfService PdfService { get; set; } = default!;
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

        private DonHangDTO selected;
        private bool ShowPdfModal = false;
        private string Base64Pdf = "";

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

        private async Task PreviewPdf(DonHangDTO dh)
        {
            // 1. GỌI LẠI API LẤY FULL DATA
            var fullDonHang = await _donHangService.GetById(dh.OrderId);

            if (fullDonHang == null) return;

            selected = fullDonHang;

            // 2. Export PDF
            byte[] pdfBytes = PdfService.ExportDonHang(fullDonHang);

            Base64Pdf = Convert.ToBase64String(pdfBytes);
            ShowPdfModal = true;
        }

        private async Task PrintPdf()
        {
            await JS.InvokeVoidAsync("downloadFileFromBase64",
                $"DonHang_{selected.OrderId}.pdf", Base64Pdf);
        }


        private void ClosePreview()
        {
            ShowPdfModal = false;
        }
    }
}

