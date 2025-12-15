using BlazorStoreManagementWebApp.DTOs.Admin.DonHang;
using BlazorStoreManagementWebApp.Services.Implements;
using BlazorStoreManagementWebApp.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.JSInterop;

namespace BlazorStoreManagementWebApp.Components.Forms.Admin
{
    public partial class ChiTietDonHangForm : ComponentBase
    {
        [Inject]
        public IDonHangService _donHangService { get; set; } = default!;

        [Inject]
        private IJSRuntime JS { get; set; } = default!;

        [Parameter]
        public EventCallback OnSaved { get; set; }

        public DonHangDTO donHang { get; set; } = new();

        private DotNetObjectReference<ChiTietDonHangForm>? objRef;
        /// <summary>
        /// Hiển thị modal chi tiết đơn hàng
        /// </summary>
        /// <param name="orderId">ID của đơn hàng cần xem</param>
        /// 
        protected override void OnInitialized()
        {
            objRef = DotNetObjectReference.Create(this);
        }
        public async Task Show(int orderId)
        {
            // Lấy dữ liệu chi tiết đơn hàng từ service
            var result = await _donHangService.GetById(orderId);

            donHang = result ?? new DonHangDTO
            {
                OrderId      = orderId,
                CustomerName = "Không tìm thấy đơn hàng",
                Phone        = "",
                Status       = "Pending"
            };

            // Đảm bảo danh sách sản phẩm không null để tránh lỗi render
            donHang.Items ??= new List<ChiTietDonHangDTO>();

            // Cập nhật UI trước khi mở modal
            StateHasChanged();

            // Mở modal Bootstrap
            await JS.InvokeVoidAsync("showBootstrapModal", "ChiTietDonHangModal");
        }

        /// <summary>
        /// Hiển thị danh sách phương thức thanh toán (hỗ trợ nhiều phương thức)
        /// </summary>
        private string DisplayPaymentMethods()
        {
            if (donHang.Payments == null || !donHang.Payments.Any())
                return "Chưa thanh toán";

            var methods = donHang.Payments
                .Select(p => p.PaymentMethod switch
                {
                    "Cash"     => "Tiền mặt",
                    "Card"     => "Thẻ tín dụng / Thẻ ghi nợ",
                    "Transfer" => "Chuyển khoản",
                    _          => p.PaymentMethod ?? "Không xác định"
                })
                .Distinct();

            return string.Join(", ", methods);
        }

        [JSInvokable]
        
        public async Task ProcessPaymentConfirmed(int orderId)
        {
            try
            {
                await _donHangService.UpdateOrderStatus(orderId, "paid");

                await JS.InvokeVoidAsync("hideBootstrapModal", "ChiTietDonHangModal");

                if (OnSaved.HasDelegate)
                {
                    await OnSaved.InvokeAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi: {ex.Message}");
            }
        }

        // 3. Hàm kích hoạt từ nút bấm HTML
        private async Task TriggerConfirmPayment(int orderId)
        {
            if (objRef != null)
            {
                await JS.InvokeVoidAsync("confirmPaymentOrder", orderId, objRef);
            }
        }
        public void Dispose()
        {
            objRef?.Dispose();
        }

        private async Task TriggerCancelOrder(int orderId)
        {
            if (objRef != null)
            {
                await JS.InvokeVoidAsync("cancelOrderConfirm", orderId, objRef);
            }
        }

        [JSInvokable]
        public async Task ProcessCancelOrder(int orderId)
        {
            try
            {
                // Gọi Service cập nhật trạng thái sang "canceled"
                await _donHangService.UpdateOrderStatus(orderId, "canceled");

                // Đóng Modal
                await JS.InvokeVoidAsync("hideBootstrapModal", "ChiTietDonHangModal");

                // Reload dữ liệu trang cha
                if (OnSaved.HasDelegate)
                {
                    await OnSaved.InvokeAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi hủy đơn hàng: {ex.Message}");
            }
        }
    }
}