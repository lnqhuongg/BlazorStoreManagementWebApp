using BlazorStoreManagementWebApp.DTOs.Admin.LoaiSanPham;
using BlazorStoreManagementWebApp.Helpers;
using BlazorStoreManagementWebApp.Models.Entities;

namespace BlazorStoreManagementWebApp.Services.Interfaces
{
    public interface ILoaiSanPhamService
    {
        Task<PagedResult<LoaiSanPhamDTO>> GetAll(int page, int pageSize, string keyword);
        IQueryable<LoaiSanPham> SearchByKeyword(string keyword);
        Task<LoaiSanPhamDTO> GetById(int category_id);
        Task<LoaiSanPhamDTO> Create(LoaiSanPhamDTO loaiSanPhamDTO);
        Task<LoaiSanPhamDTO> Update(int id, LoaiSanPhamDTO loaiSanPhamDTO);
        Task<bool> isCategoryNameExist(string categoryName, int id = 0);
        Task<bool> isCategoryExist(int category_id);
        Task<bool> Delete(int category_id);
    }
}
