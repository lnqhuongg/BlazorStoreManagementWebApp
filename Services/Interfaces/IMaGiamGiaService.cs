
using BlazorStoreManagementWebApp.DTOs.Admin.MaGiamGia;
using BlazorStoreManagementWebApp.Helpers;

namespace BlazorStoreManagementWebApp.Services.Interfaces
{
    public interface IMaGiamGiaService
    {
        Task<PagedResult<MaGiamGiaDTO>> GetAll(int page, int pageSize, string? keyword, string? discountType);
        Task<MaGiamGiaDTO?> GetById(int id);
        Task<MaGiamGiaDTO> Create(MaGiamGiaDTO dto);
        Task<MaGiamGiaDTO?> Update(int id, MaGiamGiaDTO dto);
        Task<bool> Delete(int id);
        Task<List<MaGiamGiaDTO>> SearchByKeyword(string keyword);
        Task<List<MaGiamGiaDTO>> GetAllActive();
        Task<MaGiamGiaDTO?> updateAfterCreatedOrder(int? promoId);
        Task<bool> isPromoCodeExist(string promoCode, int id = 0);
    }
}
