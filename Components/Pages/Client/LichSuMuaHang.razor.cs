using Blazored.SessionStorage;
using BlazorStoreManagementWebApp.Components.Forms.Client;
using BlazorStoreManagementWebApp.DTOs.Admin.DonHang;
using BlazorStoreManagementWebApp.Helpers;
using BlazorStoreManagementWebApp.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorStoreManagementWebApp.Components.Pages.Client
{
    public partial class LichSuMuaHang : ComponentBase
    {
        [Inject] public IDonHangService donHangService { get; set; }
        [Inject] public ISessionStorageService SessionStorage { get; set; }
        [Inject] private IJSRuntime JS { get; set; } = default!;

        protected PagedResult<DonHangDTO> DonHangData = new();
        protected int Page = 1;
        protected int PageSize = 2;
        protected string Keyword = "";

        private ChiTietDH ChiTietDHRef;

        // New filter state
        protected string StatusFilter = "all";
        protected DateTime? FilterDate = null;
        protected override async Task OnInitializedAsync()
        {
            await LoadData();
        }

        protected async Task LoadData()
        {
            // Load without filters by default, keep existing behavior
            var idKH = await SessionStorage.GetItemAsync<int>("clientId");
            DonHangData = await donHangService.GetByKhachHangId(Page, PageSize, "", "", "", idKH);
        }

        protected async Task ChangePage(int newPage)
        {
            Page = newPage;
            await LoadData();
        }

        // Apply current filters and load data
        private async Task ApplyFilters()
        {
            var idKH = await SessionStorage.GetItemAsync<int>("clientId");
            string statusParam = string.IsNullOrWhiteSpace(StatusFilter) || StatusFilter == "all" ? "" : StatusFilter;

            string startday = "";
            string endday = "";
            if (FilterDate.HasValue)
            {
                // service expects parseable date strings; use yyyy-MM-dd to be safe
                startday = FilterDate.Value.Date.ToString("yyyy-MM-dd");
                endday = FilterDate.Value.Date.ToString("yyyy-MM-dd");
            }

            DonHangData = await donHangService.GetByKhachHangId(Page, PageSize, statusParam, startday, endday, idKH);
        }

        // Event handlers requested: FilterByStatus, FilterByDate, ResetFilter

        protected async Task FilterByStatus(ChangeEventArgs e)
        {
            StatusFilter = e.Value?.ToString() ?? "all";
            Page = 1;
            await ApplyFilters();
        }

        protected async Task FilterByDate(ChangeEventArgs e)
        {
            var raw = e.Value?.ToString();
            if (DateTime.TryParse(raw, out var dt))
            {
                FilterDate = dt.Date;
            }
            else
            {
                FilterDate = null;
            }

            Page = 1;
            await ApplyFilters();
        }

        protected async Task ResetFilter()
        {
            StatusFilter = "all";
            FilterDate = null;
            Page = 1;
            await LoadData();
        }

        protected async Task CancelOrder(int orderId)
        {
            var updatedOrder = await donHangService.UpdateOrderStatus(orderId, "canceled");
            await JS.InvokeAsync<object>("showToast", "success", "Hủy đơn hàng thành công!");
            await LoadData();
        }
    }
}
