using AutoMapper;
using BlazorStoreManagementWebApp.DTOs.Admin.DonHang;
using BlazorStoreManagementWebApp.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorStoreManagementWebApp.Components.Pages.Admin
{
    public partial class ThongKe : ComponentBase
    {
        [Inject] private IDonHangService DonHangService { get; set; } = default!;
        [Inject] private IPhieuNhapService PhieuNhapService { get; set; } = default!;
        [Inject] private IJSRuntime JS { get; set; } = default!;

        private string SelectedMode = "Month";
        private int SelectedMonth = DateTime.Now.Month;
        private int SelectedYear = DateTime.Now.Year;

        private List<int> Months = Enumerable.Range(1, 12).ToList();
        private List<int> Years = Enumerable.Range(2022, 5).ToList();

        private List<DonHangDTO> TodayOrderList = new();
        private long TotalRevenue = 0;
        private long TotalCapital = 0;
        private long TotalProfit = 0;

        public List<long> RevenueData { get; set; } = new();
        public List<long> CapitalData { get; set; } = new();
        public List<long> ProfitData { get; set; } = new();
        public List<string> Labels { get; set; } = new();

        protected override async Task OnInitializedAsync()
        {
            TodayOrderList = await DonHangService.GetTodayOrders();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                // Load dữ liệu tháng hiện tại ngay khi mở trang
                await ApplyFilter();
            }
        }

        private void OnModeChanged(ChangeEventArgs e)
        {
            SelectedMode = e.Value?.ToString() ?? "Month";
        }

        private async Task ApplyFilter()
        {
            Console.WriteLine($"Mode: {SelectedMode}, Month: {SelectedMonth}, Year: {SelectedYear}");

            // Tổng doanh thu / vốn / lãi
            TotalRevenue = await DonHangService.TinhTongDoanhThu(SelectedMode, SelectedMonth, SelectedYear);
            TotalCapital = PhieuNhapService.TinhTongTienNhap(SelectedMode, SelectedMonth, SelectedYear);
            TotalProfit = TotalRevenue - TotalCapital;

            // --- Biểu đồ ---
            if (SelectedMode == "Month")
            {
                Labels = Enumerable.Range(1, DateTime.DaysInMonth(SelectedYear, SelectedMonth))
                                   .Select(d => d.ToString())
                                   .ToList();

                RevenueData = DonHangService.GetRevenueByMonth(SelectedMonth, SelectedYear);
                CapitalData = PhieuNhapService.GetCapitalByMonth(SelectedMonth, SelectedYear);
            }
            else // Year
            {
                Labels = new List<string>
                {
                    "Jan","Feb","Mar","Apr","May","Jun",
                    "Jul","Aug","Sep","Oct","Nov","Dec"
                };

                RevenueData = DonHangService.GetRevenueByYear(SelectedYear);
                CapitalData = PhieuNhapService.GetCapitalByYear(SelectedYear);
            }

            ProfitData = RevenueData.Zip(CapitalData, (r, c) => r - c).ToList();

            // Vẽ chart (Chart.js sẽ tự destroy chart cũ trong JS)
            await JS.InvokeVoidAsync("renderRevenueChart",
                Labels,
                RevenueData,
                CapitalData,
                ProfitData
            );
        }
    }
}
