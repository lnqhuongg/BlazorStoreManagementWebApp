using BlazorStoreManagementWebApp.DTOs.Admin.DonHang;
using BlazorStoreManagementWebApp.Services.Interfaces;
using Microsoft.AspNetCore.Components;
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

        /// <summary>
        /// Hiển thị modal chi tiết đơn hàng
        /// </summary>
        /// <param name="orderId">ID của đơn hàng cần xem</param>
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
    }
}