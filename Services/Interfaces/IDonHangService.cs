using BlazorStoreManagementWebApp.DTOs.Admin.DonHang;
using BlazorStoreManagementWebApp.Helpers;
using System.Threading.Tasks;

namespace BlazorStoreManagementWebApp.Services.Interfaces
{
    public interface IDonHangService
    {
        Task<PagedResult<DonHangDTO>> GetAll(int page, int pageSize, DonHangFilterDTO filter);

        // Sửa thành ChiTietDonHangDTO
        Task<ChiTietDonHangDTO> GetOrderById(int orderId);
    }
}