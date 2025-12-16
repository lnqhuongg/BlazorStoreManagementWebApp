using BlazorStoreManagementWebApp.Components.Forms.Client;
using BlazorStoreManagementWebApp.DTOs.Admin.DonHang;
using BlazorStoreManagementWebApp.DTOs.Admin.KhachHang;
using BlazorStoreManagementWebApp.DTOs.Admin.MaGiamGia;
using BlazorStoreManagementWebApp.DTOs.Admin.ThanhToanDTO;
using BlazorStoreManagementWebApp.DTOs.Payments;
using BlazorStoreManagementWebApp.Services.Interfaces;
using BlazorStoreManagementWebApp.Services.Momo;
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
        [Inject] IMomoService MomoService { get; set; } = default!;
        [Inject] NavigationManager NavigationManager { get; set; } = default!;

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

        private async Task CreateMomoPayment(ThongTinDH ttdh)
        {
            var response = await MomoService.CreatePaymentMomo(ttdh);
            if (response != null && response.ErrorCode == 0)
            {
                // Chuyển hướng người dùng đến URL thanh toán của MoMo
                NavigationManager.NavigateTo(response.PayUrl);
            }
            else
            {
                await JS.InvokeAsync<object>(
                    "showToast",
                    "error",
                    "Có lỗi xảy ra khi tạo thanh toán MoMo"
                );
            }
        }

        //protected async Task Checkout()
        //{
        //    if (!Cart.Any())
        //        return;

        //    int customerId = await SessionStorage.GetItemAsync<int>("clientId");

        //    int? promoId = SelectedPromo?.PromoId;

        //    var orderItems = Cart.Select(c => new ChiTietDonHangDTO
        //    {
        //        ProductId = c.ProductId,
        //        Quantity = c.Quantity,
        //        Price = c.Price,
        //        Subtotal = c.Price * c.Quantity
        //    }).ToList();

        //    var payments = new List<CreateThanhToanDTO>
        //    {
        //        new CreateThanhToanDTO
        //        {
        //            Amount = Total,
        //            PaymentMethod = SelectedPaymentMethod
        //        }
        //    };

        //    var dto = new CreateDonHangDTO
        //    {
        //        CustomerId = customerId,
        //        PromoId = promoId,
        //        TotalAmount = Total,
        //        DiscountAmount = DiscountAmount + RewardDiscountAmount,
        //        Items = orderItems,
        //        Payments = payments,
        //        rewardPoints = UseRewardPoints
        //                        ? (int)Math.Round(RewardDiscountAmount)
        //                        : 0
        //    };

        //    // 1. THANH TOÁN MOMO (E-WALLET)
        //    if (SelectedPaymentMethod == "e-wallet")
        //    {
        //        try
        //        {
        //            // BƯỚC QUAN TRỌNG:
        //            // 1a. LƯU ĐƠN HÀNG VÀO DATABASE VỚI TRẠNG THÁI CHƯA THANH TOÁN (Unpaid)
        //            // Giả định hàm Create() giờ đây trả về Order ID (int/long/string)
        //            string initialPaymentMethod = "Momo_Unpaid";
        //            var order = await donHangService.Create(dto, "client", initialPaymentMethod);

        //            string orderId = order.OrderId.ToString();

        //            // 1b. CHUẨN BỊ DỮ LIỆU MOMO BẰNG ORDER ID THỰC TẾ
        //            string orderInfo = $"thanh toan don hang";
        //            var ttdh = new ThongTinDH
        //            {
        //                OrderId = orderId, // ĐÃ CÓ ORDER ID THỰC TẾ!
        //                Amount = Total,
        //                FullName = "Thanhtoanmomo",
        //                OrderInfo = orderInfo
        //            };

        //            // 1c. GỌI MOMO API VÀ CHUYỂN HƯỚNG
        //            var response = await MomoService.CreatePaymentMomo(ttdh);
        //            if (response != null && response.ErrorCode == 0)
        //            {
        //                NavigationManager.NavigateTo(response.PayUrl);
        //                return; // Kết thúc hàm Checkout
        //            }
        //            else
        //            {
        //                // Nếu gọi MoMo thất bại, xóa đơn hàng tạm thời trong DB hoặc set status Failed
        //                //await donHangService.UpdateOrderStatus(orderId, "canceled");
        //                throw new Exception("Lỗi từ MoMo API");
        //            }

        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine(ex.Message);
        //            await JS.InvokeAsync<object>(
        //                "showToast",
        //                "error",
        //                $"Không thể tạo thanh toán MoMo: {ex.Message}"
        //            );
        //            return;
        //        }
        //    }

        //    // 2. THANH TOÁN THÔNG THƯỜNG (COD/KHÁC)
        //    try
        //    {
        //        await donHangService.Create(dto, "client", SelectedPaymentMethod);

        //        Cart.Clear();
        //        await SessionStorage.RemoveItemAsync("cart");

        //        await JS.InvokeAsync<object>(
        //            "showToast",
        //            "success",
        //            "Thanh toán thành công!"
        //        );
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.Message);
        //        await JS.InvokeAsync<object>(
        //            "showToast",
        //            "error",
        //            "Có lỗi xảy ra khi thanh toán"
        //        );
        //    }
        //}

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
                rewardPoints = UseRewardPoints ? (int)Math.Round(RewardDiscountAmount) : 0
            };

            // 1. THANH TOÁN MOMO (E-WALLET)
            if (SelectedPaymentMethod == "e-wallet")
            {
                try
                {
                    // 1a. LƯU ĐƠN HÀNG VÀO DATABASE VỚI TRẠNG THÁI PENDING
                    var order = await donHangService.Create(dto, "client", "Momo_Pending");
                    string orderId = order.OrderId.ToString();

                    // 1b. LƯU THÔNG TIN ĐƠN HÀNG VÀO SESSION
                    await SessionStorage.SetItemAsync("pendingOrderId", orderId);
                    await SessionStorage.SetItemAsync("pendingOrderAmount", Total);

                    // 1c. CHUẨN BỊ DỮ LIỆU MOMO
                    string orderInfo = $"Thanh toan don hang #{orderId}";
                    var ttdh = new ThongTinDH
                    {
                        OrderId = orderId,
                        Amount = Total,
                        FullName = "Khach hang",
                        OrderInfo = orderInfo
                    };

                    Console.WriteLine("=== CALLING MOMO API ===");
                    Console.WriteLine($"OrderId: {ttdh.OrderId}");
                    Console.WriteLine($"Amount: {ttdh.Amount}");
                    Console.WriteLine($"OrderInfo: {ttdh.OrderInfo}");

                    // 1d. GỌI MOMO API
                    var response = await MomoService.CreatePaymentMomo(ttdh);

                    // DEBUG: In ra toàn bộ response
                    Console.WriteLine("=== MOMO RESPONSE ===");
                    Console.WriteLine($"Response is null: {response == null}");

                    if (response != null)
                    {
                        Console.WriteLine($"ErrorCode: {response.ErrorCode}");
                        Console.WriteLine($"Message: {response.Message}");
                        Console.WriteLine($"LocalMessage: {response.LocalMessage}");
                        Console.WriteLine($"PayUrl: {response.PayUrl}");
                        Console.WriteLine($"QrCodeUrl: {response.QrCodeUrl}");
                        Console.WriteLine($"Deeplink: {response.Deeplink}");
                    }

                    // Kiểm tra response
                    if (response == null)
                    {
                        await JS.InvokeAsync<object>(
                            "showToast",
                            "error",
                            "Không nhận được phản hồi từ MoMo"
                        );
                        return;
                    }

                    if (response.ErrorCode != 0)
                    {
                        // Lỗi từ MoMo
                        await donHangService.UpdateOrderStatus(int.Parse(orderId), "Cancelled");

                        await JS.InvokeAsync<object>(
                            "showToast",
                            "error",
                            $"Lỗi MoMo [{response.ErrorCode}]: {response.Message}"
                        );
                        return;
                    }

                    if (string.IsNullOrEmpty(response.PayUrl))
                    {
                        await JS.InvokeAsync<object>(
                            "showToast",
                            "error",
                            "MoMo không trả về link thanh toán"
                        );
                        return;
                    }

                    // THÀNH CÔNG - CHUYỂN HƯỚNG
                    Console.WriteLine($"Redirecting to: {response.PayUrl}");
                    NavigationManager.NavigateTo(response.PayUrl, forceLoad: true);
                    return;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("=== EXCEPTION ===");
                    Console.WriteLine($"Message: {ex.Message}");
                    Console.WriteLine($"StackTrace: {ex.StackTrace}");

                    await JS.InvokeAsync<object>(
                        "showToast",
                        "error",
                        $"Lỗi: {ex.Message}"
                    );
                    return;
                }
            }

            // 2. THANH TOÁN THÔNG THƯỜNG (COD/CASH/KHÁC)
            try
            {
                await donHangService.Create(dto, "client", SelectedPaymentMethod);

                Cart.Clear();
                await SessionStorage.RemoveItemAsync("cart");

                await JS.InvokeAsync<object>(
                    "showToast",
                    "success",
                    "Đặt hàng thành công!"
                );

                NavigationManager.NavigateTo("/orders");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                await JS.InvokeAsync<object>(
                    "showToast",
                    "error",
                    "Có lỗi xảy ra khi đặt hàng"
                );
            }
        }
    }
}
