using BlazorStoreManagementWebApp.DTOs.Admin.TonKho;

namespace BlazorStoreManagementWebApp.Services.Interfaces
{
    public interface ITonKhoService
    {
        Task<List<TonKhoDTO>> GetAll();
        Task<TonKhoDTO> GetByProductID(int productID);
        Task<TonKhoDTO> deductQuantityOfCreatedOrder(int productID, int quantityChange);
    }

}
