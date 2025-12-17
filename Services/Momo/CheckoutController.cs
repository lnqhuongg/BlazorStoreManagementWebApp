using BlazorStoreManagementWebApp.Services.Momo;
using BlazorStoreManagementWebApp.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using BlazorStoreManagementWebApp.Models.Momo;

namespace BlazorStoreManagementWebApp.Controllers
{
    [ApiController]
    [Route("momo")]
    public class CheckoutController : ControllerBase
    {
        private readonly IMomoService _momoService;
        private readonly IDonHangService _donHangService;
        private readonly IOptions<MomoOptionModel> _momoOptions;
        private readonly ILogger<CheckoutController> _logger;

        public CheckoutController(
            IMomoService momoService,
            IDonHangService donHangService,
            IOptions<MomoOptionModel> momoOptions,
            ILogger<CheckoutController> logger)
        {
            _momoService = momoService;
            _donHangService = donHangService;
            _momoOptions = momoOptions;
            _logger = logger;
        }

        /// <summary>
        /// MoMo redirect về đây sau khi thanh toán
        /// </summary>
        [HttpGet("PaymentCallBack")]
        public async Task<ContentResult> PaymentCallBack(
            [FromQuery] string partnerCode,
            [FromQuery] string orderId,
            [FromQuery] string requestId,
            [FromQuery] string amount,
            [FromQuery] string orderInfo,
            [FromQuery] string orderType,
            [FromQuery] string transId,
            [FromQuery] string resultCode,
            [FromQuery] string message,
            [FromQuery] string payType,
            [FromQuery] string responseTime,
            [FromQuery] string extraData,
            [FromQuery] string signature)
        {
            try
            {
                _logger.LogInformation($"PaymentCallBack - OrderId: {orderId}, ResultCode: {resultCode}");

                // CẬP NHẬT TRẠNG THÁI ĐƠN HÀNG
                if (resultCode == "0" && int.TryParse(orderId, out int orderIdInt))
                {
                    await _donHangService.UpdateOrderStatus(orderIdInt, "paid");
                    _logger.LogInformation($"✅ Order {orderIdInt} marked as paid");
                }
                else if (resultCode != "0" && int.TryParse(orderId, out int failedOrderId))
                {
                    await _donHangService.UpdateOrderStatus(failedOrderId, "canceled");
                    _logger.LogWarning($"❌ Order {failedOrderId} payment failed with code {resultCode}");
                }

                // TẠO HTML ĐỂ REDIRECT (vì Blazor Server không support redirect trực tiếp)
                var redirectUrl = $"/payment-result?" +
                    $"orderId={orderId}" +
                    $"&amount={amount}" +
                    $"&transId={transId}" +
                    $"&resultCode={resultCode}" +
                    $"&message={Uri.EscapeDataString(message ?? "")}";

                var html = $@"
<!DOCTYPE html>
<html>
<head>
    <title>Redirecting...</title>
    <meta http-equiv='refresh' content='0;url={redirectUrl}'>
</head>
<body>
    <p>Đang chuyển hướng...</p>
    <script>
        window.location.href = '{redirectUrl}';
    </script>
</body>
</html>";

                return new ContentResult
                {
                    ContentType = "text/html",
                    Content = html
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in PaymentCallBack");

                var errorHtml = @"
<!DOCTYPE html>
<html>
<head>
    <meta http-equiv='refresh' content='0;url=/payment-result?resultCode=999'>
</head>
<body>
    <script>
        window.location.href = '/payment-result?resultCode=999';
    </script>
</body>
</html>";

                return new ContentResult
                {
                    ContentType = "text/html",
                    Content = errorHtml
                };
            }
        }

        /// <summary>
        /// IPN endpoint - MoMo server gọi API này (chỉ hoạt động với public domain)
        /// </summary>
        [HttpPost("MomoNotify")]
        public async Task<IActionResult> MomoNotify()
        {
            try
            {
                _logger.LogInformation("📩 Received MoMo IPN notification");

                var collection = Request.Query;
                var queryString = string.Join("&", collection.Select(x => $"{x.Key}={x.Value}"));
                _logger.LogInformation($"IPN Query: {queryString}");

                var response = _momoService.PaymentExecuteAsync(collection);

                // Verify signature
                var rawData =
                    $"accessKey={_momoOptions.Value.AccessKey}" +
                    $"&amount={response.Amount}" +
                    $"&extraData=" +
                    $"&message={collection.FirstOrDefault(s => s.Key == "message").Value}" +
                    $"&orderId={response.OrderId}" +
                    $"&orderInfo={response.OrderInfo}" +
                    $"&orderType={collection.FirstOrDefault(s => s.Key == "orderType").Value}" +
                    $"&partnerCode={_momoOptions.Value.PartnerCode}" +
                    $"&payType={collection.FirstOrDefault(s => s.Key == "payType").Value}" +
                    $"&requestId={response.OrderId}" +
                    $"&responseTime={collection.FirstOrDefault(s => s.Key == "responseTime").Value}" +
                    $"&resultCode={response.ErrorCode}" +
                    $"&transId={response.TransId}";

                var calculatedSignature = _momoService.ComputeHmacSha256(rawData, _momoOptions.Value.SecretKey);
                bool isSignatureValid = calculatedSignature.Equals(response.Signature, StringComparison.OrdinalIgnoreCase);
                bool isPaymentSuccess = response.ErrorCode == "0";

                _logger.LogInformation($"Signature valid: {isSignatureValid}, Payment success: {isPaymentSuccess}");

                if (isSignatureValid && isPaymentSuccess)
                {
                    if (int.TryParse(response.OrderId, out int orderIdInt))
                    {
                        await _donHangService.UpdateOrderStatus(orderIdInt, "paid");
                        _logger.LogInformation($"✅ IPN: Order {orderIdInt} marked as paid");
                        return Ok(new { message = "Success" });
                    }
                }
                else if (isSignatureValid && !isPaymentSuccess)
                {
                    if (int.TryParse(response.OrderId, out int orderIdInt))
                    {
                        await _donHangService.UpdateOrderStatus(orderIdInt, "canceled");
                        _logger.LogWarning($"❌ IPN: Order {orderIdInt} payment failed");
                    }
                    return Ok(new { message = "Payment failed" });
                }
                else
                {
                    _logger.LogError("❌ Invalid signature from MoMo IPN");
                    return Ok(new { message = "Invalid signature" });
                }

                return Ok(new { message = "Processed" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing MoMo IPN");
                return Ok(new { message = "Error occurred" });
            }
        }
    }
}