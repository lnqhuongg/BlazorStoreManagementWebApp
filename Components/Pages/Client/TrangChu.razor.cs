using BlazorStoreManagementWebApp.DTOs.Admin.LoaiSanPham;
using BlazorStoreManagementWebApp.DTOs.Admin.SanPham;
using BlazorStoreManagementWebApp.Helpers;
using BlazorStoreManagementWebApp.Services.Interfaces;
using Microsoft.AspNetCore.Components;

namespace BlazorStoreManagementWebApp.Components.Pages.Client
{
    public partial class TrangChu : ComponentBase
    {
        [Inject] ISanPhamService SanPhamService { get; set; } = default!;
        [Inject] ILoaiSanPhamService LoaiSanPhamService { get; set; } = default!;


        PagedResult<SanPhamDTO>? SanPhamData;

        List<LoaiSanPhamDTO> categoryList = new List<LoaiSanPhamDTO>();

        int Page = 1;
        int PageSize = 9; // 9 san pham 1 trang

        string? Keyword;
        string? Order;
        int? SelectedCategoryId;
        decimal? MinPrice;
        decimal? MaxPrice;
        string? sortType;

        int SelectedCategory;

        protected override async Task OnInitializedAsync()
        {
            await LoadData();
        }

        protected async Task LoadData()
        {
            SanPhamData = await SanPhamService.GetAll2(
                Page,
                PageSize,
                Keyword = null,
                Order = null,
                SelectedCategoryId = null,
                supplierID: null,
                MinPrice,
                MaxPrice
            );
            categoryList = await LoaiSanPhamService.GetAllCategories();
        }

        protected async Task ChangePage(int page)
        {
            Page = page;

            SanPhamData = await SanPhamService.GetAll2(
                Page,
                PageSize,
                Keyword,
                Order,
                SelectedCategoryId,
                supplierID: null,
                MinPrice,
                MaxPrice
            );
        }

        protected async Task ApplyFilter()
        {
            Console.WriteLine($"Applying filter: Keyword={Keyword}, Order={Order}, SelectedCategoryId={SelectedCategoryId}, MinPrice={MinPrice}, MaxPrice={MaxPrice}");
            Page = 1; // reset về trang 1

            SanPhamData = await SanPhamService.GetAll2(
                Page,
                PageSize,
                Keyword,
                Order,
                SelectedCategoryId,
                supplierID: null,
                MinPrice,
                MaxPrice
            );
            Console.WriteLine($"Filtered data count: {SanPhamData?.Data.Count}");
        }

        protected async Task ResetFilter()
        {
            Keyword = null;
            Order = null;
            SelectedCategoryId = null;
            MinPrice = null;
            MaxPrice = null;

            SanPhamData = await SanPhamService.GetAll2(
                Page,
                PageSize,
                Keyword = null,
                Order = null,
                SelectedCategoryId = null,
                supplierID: null,
                MinPrice,
                MaxPrice
            );
        }

        async Task OnCategoryClick(int categoryId)
        {
            SelectedCategoryId = categoryId;
            Page = 1;

            SanPhamData = await SanPhamService.GetAll2(
                Page,
                PageSize,
                Keyword,
                Order,
                SelectedCategoryId,
                supplierID: null,
                MinPrice,
                MaxPrice
            );
        }


    }
}
