using BlazorStoreManagementWebApp.Components.Forms.Admin;
using BlazorStoreManagementWebApp.DTOs.Admin.LoaiSanPham;
using BlazorStoreManagementWebApp.DTOs.Admin.NhaCungCap;
using BlazorStoreManagementWebApp.DTOs.Admin.SanPham;
using BlazorStoreManagementWebApp.DTOs.Admin.TonKho;
using BlazorStoreManagementWebApp.Helpers;
using BlazorStoreManagementWebApp.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace BlazorStoreManagementWebApp.Components.Pages.Admin
{
    public partial class SanPham : ComponentBase
    {
        [Inject] public ISanPhamService SanPhamService { get; set; }
        [Inject] public ILoaiSanPhamService LoaiSanPhamService { get; set; }
        [Inject] public INhaCungCapService NhaCungCapService { get; set; }
        [Inject] public ITonKhoService TonKhoService { get; set; }

        protected PagedResult<SanPhamDTO> SPData = new();
        protected List<LoaiSanPhamDTO> LSPData = new();
        protected List<NhaCungCapDTO> NCCData = new();
        protected int Page = 1;
        protected int PageSize = 5;
        protected string? Keyword = "";
        protected string? Order = "asc";
        protected int? CategoryID = null;
        protected int? SupplierID = null;
        private Dictionary<int, int> StockMap = new();

        private SanPhamForm SanPhamFormRef;

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
                var tonKho = await TonKhoService.GetByProductID(item.ProductID);
                StockMap[item.ProductID] = tonKho?.Quantity ?? 0;
            }
        }

        protected async Task LoadData()
        {
            LSPData = await LoaiSanPhamService.GetListLSP();
            NCCData = await NhaCungCapService.GetAllNCC();
            SPData = await SanPhamService.GetAll(Page, PageSize, Keyword, Order, CategoryID, SupplierID);
        }

        protected async Task ChangePage(int newPage)
        {
            Page = newPage;
            await LoadData();
            await LoadStockForProducts();

        }

        protected async Task Search()
        {
            Page = 1;
            await LoadData();
            await LoadStockForProducts();
        }

        protected void handleToggleIcon()
        {
            Order = (Order == "asc") ? "desc" : "asc";
        }
    }
}
