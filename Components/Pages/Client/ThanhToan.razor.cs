using BlazorStoreManagementWebApp.Components.Forms.Client;
using BlazorStoreManagementWebApp.DTOs.Admin.DonHang;
using BlazorStoreManagementWebApp.DTOs.Admin.KhachHang;
using BlazorStoreManagementWebApp.DTOs.Admin.MaGiamGia;
using BlazorStoreManagementWebApp.DTOs.Admin.ThanhToanDTO;
using BlazorStoreManagementWebApp.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorStoreManagementWebApp.Components.Pages.Client
{
    public partial class ThanhToan : ComponentBase
    {
        [Inject] ISanPhamService SanPhamService { get; set; } = default!;
        [Inject] Blazored.SessionStorage.ISessionStorageService SessionStorage { get; set; } = default!;
        [Inject] IJSRuntime JS { get; set; } = default!;
        [Inject] IKhachHangService IKhachHangService { get; set; } = default!;
        [Inject] IMaGiamGiaService IMaGiamGiaService { get; set; } = default!;
        [Inject] IDonHangService donHangService { get; set; } = default!;

        private Dictionary<int, int> StockMap = new();

        public class CartItemSession
        {
            public int ProductId { get; set; }
            public string ProductName { get; set; } = string.Empty;
            public decimal Price { get; set; }
            public int Quantity { get; set; }
        }

        protected List<CartItemSession> Cart { get; set; } = new();
        protected KhachHangDTO Client { get; set; } = new();
        protected List<MaGiamGiaDTO> DiscountCodes { get; set; } = new();

        private DiscountModal DiscountModalRef;
        private MaGiamGiaDTO? SelectedPromo;
        private bool UseRewardPoints;
        private decimal DiscountAmount;
        private decimal RewardDiscountAmount;

        protected string SelectedPaymentMethod = "cash";

        protected void SelectPayment(string method)
        {
            SelectedPaymentMethod = method;
        }

        protected override async Task OnInitializedAsync()
        {
            await LoadCartFromSession();
            await LoadCustomerInfoSession();
        }

        private async Task LoadCustomerInfoSession()
        {
            var idKH = await SessionStorage.GetItemAsync<int>("clientId");
            Client = await IKhachHangService.GetById(idKH);
        }

        private async Task LoadCartFromSession()
        {
            Cart = await SessionStorage.GetItemAsync<List<CartItemSession>>("cart")
                    ?? new List<CartItemSession>();
            await InvokeAsync(StateHasChanged);
        }

        private void ToggleRewardPoints()
        {
            if (!UseRewardPoints)
            {
                RewardDiscountAmount = 0;
                return;
            }

            // 1000 điểm = 1000đ
            var maxByPoints = Client.RewardPoints;

            // Không được trừ quá tiền còn lại
            var maxByOrder = Subtotal - DiscountAmount;

            RewardDiscountAmount = Math.Min(maxByPoints, maxByOrder);

            if (RewardDiscountAmount < 0)
                RewardDiscountAmount = 0;
        }

        protected decimal Subtotal => Cart.Sum(x => x.Price * x.Quantity);

        protected decimal Total => Subtotal - DiscountAmount - RewardDiscountAmount;

        private void ApplyDiscount(MaGiamGiaDTO promo)
        {
            SelectedPromo = promo;

            DiscountAmount = 0;

            // Kiểm tra điều kiện đơn tối thiểu
            if (Subtotal < promo.MinOrderAmount)
            {
                SelectedPromo = null;
                return;
            }

            if (promo.DiscountType == "fixed")
            {
                DiscountAmount = promo.DiscountValue;
            }
            else if (promo.DiscountType == "percent")
            {
                DiscountAmount = Subtotal * promo.DiscountValue / 100;
            }

            // Không cho giảm quá tiền đơn
            if (DiscountAmount > Subtotal)
                DiscountAmount = Subtotal;

            StateHasChanged();
        }

        protected async Task Checkout()
        {
            if (!Cart.Any())
                return;

            int customerId = await SessionStorage.GetItemAsync<int>("clientId");

            int? promoId = SelectedPromo?.PromoId;

            var orderItems = Cart.Select(c => new ChiTietDonHangDTO
            {
                ProductId = c.ProductId,
                Quantity = c.Quantity,
                Price = c.Price,
                Subtotal = c.Price * c.Quantity
            }).ToList();

            var payments = new List<CreateThanhToanDTO>
            {
                new CreateThanhToanDTO
                {
                    Amount = Total,
                    PaymentMethod = SelectedPaymentMethod
                }
            };

            var dto = new CreateDonHangDTO
            {
                CustomerId = customerId,
                PromoId = promoId,
                TotalAmount = Total,
                DiscountAmount = DiscountAmount + RewardDiscountAmount,
                Items = orderItems,
                Payments = payments,
                rewardPoints = UseRewardPoints
                                ? (int)Math.Round(RewardDiscountAmount)
                                : 0
            };

            try
            {
                await donHangService.Create(dto, "client", SelectedPaymentMethod);

                Cart.Clear();
                await SessionStorage.RemoveItemAsync("cart");

                await JS.InvokeAsync<object>(
                    "showToast",
                    "success",
                    "Thanh toán thành công!"
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                await JS.InvokeAsync<object>(
                    "showToast",
                    "error",
                    "Có lỗi xảy ra khi thanh toán"
                );
            }
        }

    }
}
