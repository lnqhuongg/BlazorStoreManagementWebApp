using BlazorStoreManagementWebApp.DTOs.Admin.NhaCungCap;
using BlazorStoreManagementWebApp.Helpers;
using BlazorStoreManagementWebApp.Models.Entities;

namespace BlazorStoreManagementWebApp.Services.Interfaces
{
    public interface INhaCungCapService
    {
        Task<PagedResult<NhaCungCapDTO>> GetAll(int page, int pageSize, string keyword);
        Task<List<NhaCungCapDTO>> GetAllNCC();
        IQueryable<NhaCungCap> SearchByKeyword(string keyword);
        Task<NhaCungCapDTO?> GetById(int supplierId);
        Task<NhaCungCapDTO> Create(NhaCungCapDTO nhaCungCapDTO);
        Task<NhaCungCapDTO?> Update(int id, NhaCungCapDTO dtnhaCungCapDTOo);
        Task<bool> IsSupplierIdExist(int supplierId);
        Task<bool> IsSupplierExist(string name, string email, string phone, int? ignoreId = null);
        Task<bool> Delete(int supplierId);
    }

}
