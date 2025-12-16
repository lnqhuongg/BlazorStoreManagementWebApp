using BlazorStoreManagementWebApp.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using static BlazorStoreManagementWebApp.Components.Pages.Client.TrangChu;

namespace BlazorStoreManagementWebApp.Components.Pages.Client
{
    public partial class GioHang : ComponentBase
    {
        [Inject] ISanPhamService SanPhamService { get; set; } = default!;
        [Inject] Blazored.SessionStorage.ISessionStorageService SessionStorage { get; set; } = default!;
        [Inject] IJSRuntime JS { get; set; } = default!;

        // Giỏ hàng trong session
        public class CartItemSession
        {
            public int ProductId { get; set; }
            public string ProductName { get; set; } = string.Empty;
            public decimal Price { get; set; }
            public int Quantity { get; set; }
        }

        // Giỏ hàng hiển thị trên giao diện
        public class CartItemViewModel
        {
            public int ProductId { get; set; }
            public string ProductName { get; set; } = string.Empty;
            public string ImageUrl { get; set; } = string.Empty;
            public decimal Price { get; set; }
            public int Quantity { get; set; }
        }

        protected List<CartItemViewModel> Cart { get; set; } = new();
        protected override async Task OnInitializedAsync()
        {
            await LoadCartFromSession();
        }

        private async Task LoadCartFromSession()
        {
            // 1️⃣ Lấy cart từ session
            var sessionCart = await SessionStorage.GetItemAsync<List<CartItemSession>>("cart")
                              ?? new List<CartItemSession>();

            Cart.Clear();

            // 2️⃣ Load chi tiết sản phẩm
            foreach (var item in sessionCart)
            {
                var product = await SanPhamService.GetById(item.ProductId);
                if (product == null) continue;

                Cart.Add(new CartItemViewModel
                {
                    ProductId = item.ProductId,
                    ProductName = product.ProductName,
                    ImageUrl = product.ImageUrl,
                    Price = item.Price,
                    Quantity = item.Quantity
                });
            }
        }

        // Tăng số lượng sản phẩm trong giỏ hàng
        protected async Task IncreaseQuantity(int productId)
        {
            var sessionCart = await SessionStorage.GetItemAsync<List<CartItemSession>>("cart")
                              ?? new();

            var item = sessionCart.FirstOrDefault(x => x.ProductId == productId);
            if (item == null) return;

            item.Quantity++;

            await SessionStorage.SetItemAsync("cart", sessionCart);
            await LoadCartFromSession();
        }

        // Giảm số lượng sản phẩm trong giỏ hàng
        protected async Task DecreaseQuantity(int productId)
        {
            var sessionCart = await SessionStorage.GetItemAsync<List<CartItemSession>>("cart")
                              ?? new();

            var item = sessionCart.FirstOrDefault(x => x.ProductId == productId);
            if (item == null) return;

            item.Quantity--;
            if (item.Quantity <= 0)
                sessionCart.Remove(item);

            await SessionStorage.SetItemAsync("cart", sessionCart);
            await LoadCartFromSession();
        }

        // Xóa sản phẩm khỏi giỏ hàng
        protected async Task RemoveFromCart(int productId)
        {
            var sessionCart = await SessionStorage.GetItemAsync<List<CartItemSession>>("cart")
                              ?? new();

            sessionCart.RemoveAll(x => x.ProductId == productId);

            await SessionStorage.SetItemAsync("cart", sessionCart);
            await LoadCartFromSession();
        }


        // Tính tổng tiền trong giỏ hàng
        protected decimal Total => Cart.Sum(x => x.Price * x.Quantity);
    }
}
