using BlazorStoreManagementWebApp.DTOs.Payments;
using BlazorStoreManagementWebApp.Services.Momo;
using BlazorStoreManagementWebApp.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BlazorStoreManagementWebApp.Controllers
{
    [Route("[controller]")]
    public class CheckoutController : Controller
    {
        private readonly IMomoService _momoService;
        private readonly IDonHangService _donHangService;

        public CheckoutController(IMomoService momoService, IDonHangService donHangService)
        {
            _momoService = momoService;
            _donHangService = donHangService;
        }

        // Bước 1: Tạo đơn hàng và yêu cầu thanh toán MoMo
        [HttpPost("CreateMomoPayment")]
        public async Task<IActionResult> CreateMomoPayment([FromBody] ThongTinDH model)
        {
            try
            {
                // Gọi API MoMo để tạo payment
                var response = await _momoService.CreatePaymentMomo(model);

                if (response.ErrorCode == 0)
                {
                    // Lưu đơn hàng vào database với trạng thái "Pending"
                    // await _donHangService.CreateOrder(model, "Pending");

                    // Trả về PayUrl để frontend redirect
                    return Ok(new
                    {
                        success = true,
                        payUrl = response.PayUrl,
                        orderId = response.OrderId,
                        message = "Tạo link thanh toán thành công"
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = $"Lỗi MoMo: {response.Message}"
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        [HttpGet("PaymentCallBack")]
        public IActionResult PaymentCallBack()
        {
            try
            {
                // Lấy query parameters từ MoMo
                var queryString = Request.QueryString.Value;

                // Redirect sang trang Blazor với query string nguyên vẹn
                return Redirect($"/payment-result{queryString}");
            }
            catch (Exception ex)
            {
                // Nếu có lỗi, redirect về trang lỗi
                return Redirect($"/payment-result?error={ex.Message}");
            }
        }


        // Bước 3: MoMo gọi API này để thông báo kết quả (IPN)
        [HttpPost("MomoNotify")]
        public async Task<IActionResult> MomoNotify()
        {
            try
            {
                var collection = Request.Query;
                var response = _momoService.PaymentExecuteAsync(collection);

                // Xác thực signature (đã làm trong MomoPaymentController)
                bool isPaymentSuccess = response.ErrorCode == "0";

                if (isPaymentSuccess)
                {
                    if (int.TryParse(response.OrderId, out int orderIdInt))
                    {
                        // Cập nhật trạng thái đơn hàng
                        await _donHangService.UpdateOrderStatus(orderIdInt, "paid");

                        // Log thành công
                        Console.WriteLine($"Order {orderIdInt} payment completed. TransId: {response.TransId}");
                    }
                }
                else
                {
                    if (int.TryParse(response.OrderId, out int orderIdInt))
                    {
                        await _donHangService.UpdateOrderStatus(orderIdInt, "canceled");
                        Console.WriteLine($"Order {orderIdInt} payment failed. ErrorCode: {response.ErrorCode}");
                    }
                }

                // Phải trả về 200 OK để MoMo biết đã nhận được thông báo
                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in MomoNotify: {ex.Message}");
                return Ok(); // Vẫn trả về OK để MoMo không gọi lại liên tục
            }
        }
    }
}