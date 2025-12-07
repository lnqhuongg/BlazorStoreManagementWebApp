using BlazorStoreManagementWebApp.DTOs.Admin.NhanVien;
using BlazorStoreManagementWebApp.Helpers;
using BlazorStoreManagementWebApp.Models.Entities;

namespace BlazorStoreManagementWebApp.Services.Interfaces
{
    public interface INhanVienService
    {
        Task<PagedResult<NhanVienDTO>> GetAll(int page, int pageSize, NhanVienFilterDTO filter);
        Task<NhanVienDTO?> GetById(int userId);
        IQueryable<NhanVien> Search(NhanVienFilterDTO filter);
        Task<NhanVienDTO> Create(NhanVienDTO dto);
        Task<NhanVienDTO?> Update(int id, NhanVienDTO dto);
        Task<bool> isUsernameExist(string username);
        Task<bool> isUserExist(int userId);
    }

}
