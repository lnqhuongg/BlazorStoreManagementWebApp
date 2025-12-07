using BlazorStoreManagementWebApp.DTOs.Admin.MaGiamGia;
using BlazorStoreManagementWebApp.Helpers;

namespace BlazorStoreManagementWebApp.Services.Interfaces
{
    public interface IMaGiamGiaService
    {
        Task<PagedResult<MaGiamGiaDTO>> GetAll(int page, int pageSize, string? keyword, string? discountType);
        Task<MaGiamGiaDTO?> GetById(int id);
        Task<ApiResponse<MaGiamGiaDTO>> Create(MaGiamGiaDTO dto);
        Task<ApiResponse<MaGiamGiaDTO>> Update(int id, MaGiamGiaDTO dto);
        Task<ApiResponse<bool>> Delete(int id);
        Task<List<MaGiamGiaDTO>> SearchByKeyword(string keyword);
        Task<List<MaGiamGiaDTO>> GetAllActive();
        Task<MaGiamGiaDTO?> updateAfterCreatedOrder(int? promoId);
    }

}
