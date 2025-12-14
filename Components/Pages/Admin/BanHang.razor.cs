using BlazorStoreManagementWebApp.DTOs.Admin.LoaiSanPham;
using BlazorStoreManagementWebApp.DTOs.Admin.SanPham;
using BlazorStoreManagementWebApp.Helpers;
using BlazorStoreManagementWebApp.Services.Interfaces;
using Microsoft.AspNetCore.Components;

namespace BlazorStoreManagementWebApp.Components.Pages.Admin
{
    public partial class BanHang : ComponentBase
    {
        [Inject] public ISanPhamService sanPhamService { get; set; }
        [Inject] public ITonKhoService tonKhoService { get; set; }
        [Inject] public ILoaiSanPhamService loaiSanPhamService { get; set; }

        protected PagedResult<SanPhamDTO> SPData = new();
        protected List<LoaiSanPhamDTO> LSPData = new();

        protected int Page = 1;
        protected int PageSize = 5;
        protected string? Keyword = "";
        protected string? Order = "asc";
        protected int? CategoryID = null;
        private Dictionary<int, int> StockMap = new();

        protected override async Task OnInitializedAsync()
        {
            await LoadData();
            await LoadStockForProducts();
            StateHasChanged();
        }

        private async Task LoadStockForProducts()
        {
            StockMap.Clear();

            foreach (var item in SPData.Data)
            {
                var tonKho = await tonKhoService.GetByProductID(item.ProductID);
                StockMap[item.ProductID] = tonKho?.Quantity ?? 0;
            }
        }

        protected async Task LoadData()
        {
            LSPData = await loaiSanPhamService.GetListLSP();
            SPData = await sanPhamService.GetAll(Page, PageSize, Keyword, Order, CategoryID, null);
        }

        protected async Task Search()
        {
            Page = 1;
            await LoadData();
            await LoadStockForProducts();
        }

        protected async Task ChangePage(int newPage)
        {
            Page = newPage;
            await LoadData();
            await LoadStockForProducts();

        }
    }
}
