using BlazorStoreManagementWebApp.DTOs.Payments;
using BlazorStoreManagementWebApp.Models.Momo;

namespace BlazorStoreManagementWebApp.Services.Momo
{
    public interface IMomoService
    {
        Task<MomoCreatePaymentResponseModel> CreatePaymentMomo(ThongTinDH model);
        MomoExecuteResponseModel PaymentExecuteAsync(IQueryCollection collection);
        string ComputeHmacSha256(string message, string secretKey);
    }
}
