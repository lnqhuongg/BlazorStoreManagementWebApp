using BlazorStoreManagementWebApp.DTOs.Admin.DonHang;
using BlazorStoreManagementWebApp.DTOs.Admin.KhachHang;
using BlazorStoreManagementWebApp.DTOs.Admin.LoaiSanPham;
using BlazorStoreManagementWebApp.DTOs.Admin.MaGiamGia;
using BlazorStoreManagementWebApp.DTOs.Admin.SanPham;
using BlazorStoreManagementWebApp.DTOs.Admin.ThanhToanDTO;
using BlazorStoreManagementWebApp.Helpers;
using BlazorStoreManagementWebApp.Services.Implements;
using BlazorStoreManagementWebApp.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Threading;

namespace BlazorStoreManagementWebApp.Components.Pages.Admin
{
    public partial class BanHang : ComponentBase, IDisposable
    {
        [Inject] public ISanPhamService sanPhamService { get; set; }
        [Inject] public ITonKhoService tonKhoService { get; set; }
        [Inject] public ILoaiSanPhamService loaiSanPhamService { get; set; }
        [Inject] public IKhachHangService khachHangService { get; set; }
        [Inject] public IMaGiamGiaService maGiamGiaService { get; set; }
        [Inject] public IDonHangService donHangService { get; set; }
        [Inject] public Blazored.SessionStorage.ISessionStorageService SessionStorage { get; set; }
        [Inject] public IJSRuntime JS { get; set; }

        // load danh sach san pham + loai san pham
        protected PagedResult<SanPhamDTO> SPData = new();
        protected List<LoaiSanPhamDTO> LSPData = new();

        // filter + paging state
        protected int Page = 1;
        protected int PageSize = 8;
        protected string? Keyword = "";
        protected string? Order = "asc";
        protected int? CategoryID = null;
        private Dictionary<int, int> StockMap = new();

        // Prevent concurrent DB operations (DbContext is not thread-safe)
        private readonly SemaphoreSlim _loadSemaphore = new(1, 1);

        // Debounce for search input
        private CancellationTokenSource? _searchCts;

        // class giỏ hàng lưu lại các sp đã thêm
        protected class CartItem
        {
            public int ProductId { get; set; }
            public string Name { get; set; } = string.Empty;
            public decimal Price { get; set; }
            public int Quantity { get; set; } = 1;
        }

        // list các sp trong giỏ hàng
        protected List<CartItem> Cart { get; set; }

        // mặc định thanh toán tiền mặt
        protected string SelectedPayment { get; set; } = "cash";

        // khách hàng
        protected string CustomerPhone = "";
        protected KhachHangDTO? SelectedCustomer;
        protected bool ShowCustomerSuggestion = false;
        protected bool UsePoints = false;

        // mã giảm giá
        protected string DiscountKeyword = "";
        protected List<MaGiamGiaDTO> DiscountSuggestions = new();
        protected MaGiamGiaDTO? SelectedDiscount;

        // phương thức thanh toán
        protected string SelectedPaymentMethod = "cash";

        // nút thanh toán bị vô hiệu hóa khi giỏ hàng trống hoặc chưa chọn phương thức thanh toán
        protected bool IsCheckoutDisabled =>
            Cart == null || !Cart.Any()
            || string.IsNullOrEmpty(SelectedPaymentMethod);

        // chọn mã giảm giá từ gợi ý
        protected void SelectDiscount(MaGiamGiaDTO promo)
        {
            SelectedDiscount = promo;
            DiscountKeyword = promo.PromoCode;
            DiscountSuggestions.Clear();
        }

        // chọn phương thức thanh toán
        protected void SelectPayment(string method)
        {
            SelectedPaymentMethod = method;
        }

        protected override async Task OnInitializedAsync()
        {
            Cart = new List<CartItem>();
            await LoadAll();
        }

        // _loadSemaphore để đảm bảo chỉ có một thao tác tải dữ liệu diễn ra tại một thời điểm
        private async Task LoadAll()
        {
            await _loadSemaphore.WaitAsync();
            try
            {
                await LoadData();
                await LoadStockForProducts();
            }
            finally
            {
                _loadSemaphore.Release();
            }
            await InvokeAsync(StateHasChanged);
        }

        private async Task LoadStockForProducts()
        {
            StockMap.Clear();

            if (SPData?.Data != null)
            {
                foreach (var item in SPData.Data)
                {
                    // each GetByProductID is awaited sequentially to avoid concurrency on DbContext
                    var tonKho = await tonKhoService.GetByProductID(item.ProductID);
                    StockMap[item.ProductID] = tonKho?.Quantity ?? 0;
                }
            }
        }

        protected async Task LoadData()
        {
            // Load categories + products using the same safe approach from callers
            LSPData = await loaiSanPhamService.GetListLSP();
            SPData = await sanPhamService.GetAll(Page, PageSize, Keyword, Order, CategoryID, null);
        }

        // called by keyup -- debounce to avoid firing many concurrent requests (tránh gửi nhiều request đồng thời)
        protected async Task Search(KeyboardEventArgs? e = null)
        {
            _searchCts?.Cancel();
            _searchCts?.Dispose();
            _searchCts = new CancellationTokenSource();

            var token = _searchCts.Token;
            try
            {
                // debounce 300ms
                await Task.Delay(300, token);
            }
            catch (TaskCanceledException)
            {
                return; // new keystroke arrived
            }

            // ensure only one load at a time
            await _loadSemaphore.WaitAsync();
            try
            {
                Page = 1;
                await LoadData();
                await LoadStockForProducts();
            }
            finally
            {
                _loadSemaphore.Release();
            }

            await InvokeAsync(StateHasChanged);
        }

        // handler for category select change (xử lý khi thay đổi danh mục)
        private async Task OnCategoryChanged(ChangeEventArgs e)
        {
            if (e?.Value == null || string.IsNullOrEmpty(e.Value.ToString()))
            {
                CategoryID = null;
            }
            else if (int.TryParse(e.Value.ToString(), out var parsed))
            {
                CategoryID = parsed;
            }
            else
            {
                CategoryID = null;
            }

            // single-threaded load (đảm bảo chỉ có một thao tác tải dữ liệu diễn ra tại một thời điểm)
            await _loadSemaphore.WaitAsync();
            try
            {
                Page = 1;
                await LoadData();
                await LoadStockForProducts();
            }
            finally
            {
                _loadSemaphore.Release();
            }

            await InvokeAsync(StateHasChanged);
        }

        protected async Task ChangePage(int newPage)
        {
            if (newPage < 1) return;

            await _loadSemaphore.WaitAsync();
            try
            {
                Page = newPage;
                await LoadData();
                await LoadStockForProducts();
            }
            finally
            {
                _loadSemaphore.Release();
            }

            await InvokeAsync(StateHasChanged);
        }

        // bắt sự kiện thêm vào giỏ hàng
        protected async Task AddToCart(int productId, string name, decimal price)
        {
            var item = Cart.FirstOrDefault(x => x.ProductId == productId);
            if (item != null)
            {
                item.Quantity++;
            }
            else
            {
                Cart.Add(new CartItem { ProductId = productId, Name = name, Price = price, Quantity = 1 });
            }
            await InvokeAsync(StateHasChanged);
        }

        protected async Task IncreaseQuantity(int productId)
        {
            var item = Cart.FirstOrDefault(x => x.ProductId == productId);
            if (item != null)
            {
                item.Quantity++;
                await InvokeAsync(StateHasChanged);
            }
        }

        protected async Task DecreaseQuantity(int productId)
        {
            var item = Cart.FirstOrDefault(x => x.ProductId == productId);
            if (item != null)
            {
                item.Quantity--;
                if (item.Quantity <= 0)
                    Cart.Remove(item);
                await InvokeAsync(StateHasChanged);
            }
        }

        protected async Task RemoveFromCart(int productId)
        {
            var item = Cart.FirstOrDefault(x => x.ProductId == productId);
            if (item != null)
            {
                Cart.Remove(item);
                await InvokeAsync(StateHasChanged);
            }
        }

        // tìm khách hàng theo số điện thoại
        protected async Task FindCustomer()
        {
            if (string.IsNullOrWhiteSpace(CustomerPhone) || CustomerPhone.Length < 6)
            {
                SelectedCustomer = null;
                ShowCustomerSuggestion = false;
                return;
            }

            await _loadSemaphore.WaitAsync();
            try
            {
                SelectedCustomer = await khachHangService.findByPhone(CustomerPhone);
                if (SelectedCustomer == null)
                {
                    await JS.InvokeAsync<object>(
                        "showToast",
                        "info",
                        "Không tìm thấy khách hàng"
                    );
                }
                ShowCustomerSuggestion = SelectedCustomer != null;
            }
            finally
            {
                _loadSemaphore.Release();
            }

            await InvokeAsync(StateHasChanged);
        }   

        // tìm mã giảm giá theo từ khóa
        protected async Task ApplyDiscount()
        {
            // Nếu input rỗng → hủy mã giảm giá
            if (string.IsNullOrWhiteSpace(DiscountKeyword))
            {
                SelectedDiscount = null;
                DiscountSuggestions.Clear();
                await InvokeAsync(StateHasChanged);
                return;
            }

            await _loadSemaphore.WaitAsync();
            try
            {
                var results = await maGiamGiaService.SearchByKeyword(DiscountKeyword);

                if (results.Any())
                {
                    // LUÔN gán object MỚI
                    SelectedDiscount = results.First();
                }
                else
                {
                    // Không tìm thấy → hủy
                    if (SelectedCustomer == null)
                    {
                        await JS.InvokeAsync<object>(
                            "showToast",
                            "info",
                            "Không tìm thấy mã giảm giá"
                        );
                    }
                    SelectedDiscount = null;
                }

                DiscountSuggestions = results;
            }
            finally
            {
                _loadSemaphore.Release();
            }

            await InvokeAsync(StateHasChanged);
        }

        protected decimal Subtotal => Cart.Sum(x => x.Price * x.Quantity);

        protected decimal PointsDiscount =>
            UsePoints && SelectedCustomer != null
            ? Math.Min(SelectedCustomer.RewardPoints * 1000, Subtotal)
            : 0;

        protected decimal PromoDiscount
        {
            get
            {
                if (SelectedDiscount == null)
                    return 0;

                if (SelectedDiscount.DiscountType == "percent")
                {
                    return Subtotal * (SelectedDiscount.DiscountValue / 100m);
                }

                if (SelectedDiscount.DiscountType == "fixed")
                {
                    return Math.Min(SelectedDiscount.DiscountValue, Subtotal);
                }

                return 0;
            }
        }

        protected decimal Total =>
            Subtotal - PromoDiscount - PointsDiscount;

        protected async Task Checkout()
        {
            if (IsCheckoutDisabled)
                return;

            if (Cart == null || !Cart.Any())
                return;

            foreach (var item in Cart)
            {
                if (!StockMap.TryGetValue(item.ProductId, out var stock))
                {
                    await JS.InvokeAsync<object>(
                        "showToast",
                        "error",
                        "Không xác định được tồn kho sản phẩm"
                    );
                    return;
                }

                if (stock < item.Quantity)
                {
                    await JS.InvokeAsync<object>(
                        "showToast",
                        "warning",
                        $"Sản phẩm \"{item.Name}\" không đủ số lượng trong tồn kho để than toán"
                    );
                    return;
                }
            }

            int? customerId = SelectedCustomer?.CustomerId;
            int? promoId = SelectedDiscount?.PromoId;

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

            var userId = await SessionStorage.GetItemAsync<int>("adminId");
            var dto = new CreateDonHangDTO
            {
                CustomerId = customerId,
                UserId = userId,
                PromoId = promoId,
                TotalAmount = Total,
                DiscountAmount = PromoDiscount + PointsDiscount,
                Items = orderItems,
                Payments = payments,
                rewardPoints = UsePoints && SelectedCustomer != null
                    ? SelectedCustomer.RewardPoints
                    : 0
            };

            try
            {
                var result = await donHangService.Create(dto);

                // reset sau checkout
                Cart.Clear();
                SelectedCustomer = null;
                SelectedDiscount = null;
                UsePoints = false;

                await JS.InvokeAsync<object>("showToast", "success", "Thanh toán thành công!");

                await InvokeAsync(StateHasChanged);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


        // cái này để hủy các thao tác bất đồng bộ khi component bị hủy
        public void Dispose()
        {
            _searchCts?.Cancel();
            _searchCts?.Dispose();
            _loadSemaphore.Dispose();
        }
    }
}