using BlazorStoreManagementWebApp.DTOs.Payments;
using BlazorStoreManagementWebApp.Models.Momo;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using System.Security.Cryptography;
using System.Text;

namespace BlazorStoreManagementWebApp.Services.Momo
{
    public class MomoService : IMomoService
    {
        private readonly IOptions<MomoOptionModel> _options;

        public MomoService(IOptions<MomoOptionModel> options)
        {
            _options = options;
        }

        public async Task<MomoCreatePaymentResponseModel> CreatePaymentMomo(ThongTinDH model)
        {
            // KHÔNG GHI ĐÈ OrderId nữa - dùng OrderId từ database
            // Chỉ format lại OrderInfo
            model.OrderInfo = "Khách hàng: " + model.FullName + ". Nội dung: " + model.OrderInfo;

            // Số tiền phải là string không có dấu phẩy
            var amountString = Math.Round(model.Amount).ToString("0", System.Globalization.CultureInfo.InvariantCulture);

            // SIGNATURE theo đúng thứ tự alphabet của MoMo API v2
            var rawData =
                $"accessKey={_options.Value.AccessKey}" +
                $"&amount={amountString}" +
                $"&extraData=" +
                $"&ipnUrl={_options.Value.NotifyUrl}" +
                $"&orderId={model.OrderId}" +
                $"&orderInfo={model.OrderInfo}" +
                $"&partnerCode={_options.Value.PartnerCode}" +
                $"&redirectUrl={_options.Value.ReturnUrl}" +
                $"&requestId={model.OrderId}" +
                $"&requestType={_options.Value.RequestType}";

            Console.WriteLine("=== RAW DATA FOR SIGNATURE ===");
            Console.WriteLine(rawData);

            var signature = ComputeHmacSha256(rawData, _options.Value.SecretKey);

            Console.WriteLine($"=== SIGNATURE ===");
            Console.WriteLine(signature);

            var client = new RestClient(_options.Value.MomoApiUrl);
            var request = new RestRequest() { Method = Method.Post };
            request.AddHeader("Content-Type", "application/json; charset=UTF-8");

            // REQUEST BODY theo format MoMo API v2
            var requestData = new
            {
                partnerCode = _options.Value.PartnerCode,
                partnerName = "Test",
                storeId = _options.Value.PartnerCode,
                requestId = model.OrderId,
                amount = amountString,
                orderId = model.OrderId,
                orderInfo = model.OrderInfo,
                redirectUrl = _options.Value.ReturnUrl,
                ipnUrl = _options.Value.NotifyUrl,
                lang = "vi",
                extraData = "",
                requestType = _options.Value.RequestType,
                signature = signature
            };

            var jsonRequest = JsonConvert.SerializeObject(requestData);
            Console.WriteLine("=== REQUEST JSON ===");
            Console.WriteLine(jsonRequest);

            request.AddParameter("application/json", jsonRequest, ParameterType.RequestBody);

            var response = await client.ExecuteAsync(request);

            Console.WriteLine("=== RESPONSE STATUS ===");
            Console.WriteLine($"StatusCode: {response.StatusCode}");
            Console.WriteLine("=== RESPONSE CONTENT ===");
            Console.WriteLine(response.Content);

            var momoResponse = JsonConvert.DeserializeObject<MomoCreatePaymentResponseModel>(response.Content);

            if (momoResponse.ErrorCode != 0)
            {
                Console.WriteLine($"MoMo API Error Code: {momoResponse.ErrorCode}");
                Console.WriteLine($"MoMo API Message: {momoResponse.Message}");
            }

            return momoResponse;
        }

        public MomoExecuteResponseModel PaymentExecuteAsync(IQueryCollection collection)
        {
            var amount = collection.First(s => s.Key == "amount").Value;
            var orderInfo = collection.First(s => s.Key == "orderInfo").Value;
            var orderId = collection.First(s => s.Key == "orderId").Value;

            return new MomoExecuteResponseModel()
            {
                OrderId = collection.FirstOrDefault(s => s.Key == "orderId").Value.ToString(),
                Amount = collection.FirstOrDefault(s => s.Key == "amount").Value.ToString(),
                OrderInfo = collection.FirstOrDefault(s => s.Key == "orderInfo").Value.ToString(),
                ErrorCode = collection.FirstOrDefault(s => s.Key == "errorCode").Value.ToString(),
                TransId = collection.FirstOrDefault(s => s.Key == "transId").Value.ToString(),
                Signature = collection.FirstOrDefault(s => s.Key == "signature").Value.ToString(),
            };
        }

        public string ComputeHmacSha256(string message, string secretKey)
        {
            var keyBytes = Encoding.UTF8.GetBytes(secretKey);
            var messageBytes = Encoding.UTF8.GetBytes(message);

            byte[] hashBytes;

            using (var hmac = new HMACSHA256(keyBytes))
            {
                hashBytes = hmac.ComputeHash(messageBytes);
            }

            var sb = new StringBuilder();
            foreach (byte b in hashBytes)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }
    }
}