using BlazorStoreManagementWebApp.DTOs.Admin.LoaiSanPham;
using BlazorStoreManagementWebApp.DTOs.Admin.SanPham;
using BlazorStoreManagementWebApp.Helpers;
using BlazorStoreManagementWebApp.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorStoreManagementWebApp.Components.Pages.Client
{
    public partial class TrangChu : ComponentBase
    {
        [Inject] ISanPhamService SanPhamService { get; set; } = default!;
        [Inject] ILoaiSanPhamService LoaiSanPhamService { get; set; } = default!;
        [Inject] Blazored.SessionStorage.ISessionStorageService SessionStorage { get; set; } = default!;
        [Inject] IJSRuntime JS { get; set; } = default!;


        PagedResult<SanPhamDTO>? SanPhamData;

        List<LoaiSanPhamDTO> categoryList = new List<LoaiSanPhamDTO>();

        int Page = 1;
        int PageSize = 6; // 9 san pham 1 trang

        string? Keyword;
        string? Order;
        int? SelectedCategoryId;
        decimal? MinPrice;
        decimal? MaxPrice;
        string? sortType;

        int SelectedCategory;

        // Giỏ hàng trong session
        public class CartItemSession
        {
            public int ProductId { get; set; }
            public string ProductName { get; set; } = string.Empty;
            public decimal Price { get; set; }
            public int Quantity { get; set; }
        }

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

        // ------------------------------- PHẦN NÀY XỬ LÝ GIỎ HÀNG TRONG SESSION -------------------------------
        protected async Task AddToCart(SanPhamDTO sp)
        {
            // 1️⃣ Kiểm tra đăng nhập
            var clientId = await SessionStorage.GetItemAsync<int>("clientId");

            if (clientId <= 0)
            {
                await JS.InvokeAsync<object>(
                    "showToast",
                    "info",
                    "Đăng nhập vào tài khoản để thêm sản phẩm vào giỏ hàng"
                );
                return;
            }

            // 2️⃣ Lấy giỏ hàng từ session
            var cart = await SessionStorage.GetItemAsync<List<CartItemSession>>("cart")
                       ?? new List<CartItemSession>();

            // 3️⃣ Kiểm tra sản phẩm đã có trong giỏ chưa
            var item = cart.FirstOrDefault(x => x.ProductId == sp.ProductID);

            if (item != null)
            {
                item.Quantity++;
            }
            else
            {
                cart.Add(new CartItemSession
                {
                    ProductId = sp.ProductID,
                    ProductName = sp.ProductName,
                    Price = sp.Price,
                    Quantity = 1
                });
            }

            // 4️⃣ Lưu lại session
            await SessionStorage.SetItemAsync("cart", cart);

            // 5️⃣ Toast thành công
            await JS.InvokeAsync<object>(
                "showToast",
                "success",
                "Đã thêm sản phẩm vào giỏ hàng"
            );
        }

    }
}
