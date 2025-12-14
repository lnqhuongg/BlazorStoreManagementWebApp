using BlazorStoreManagementWebApp.DTOs.Admin.SanPham;
using BlazorStoreManagementWebApp.Helpers;

namespace BlazorStoreManagementWebApp.Services.Interfaces
{
    public interface ISanPhamService
    {
        public Task<List<SanPhamDTO>> getListProducts();
        public Task<PagedResult<SanPhamDTO>> GetAll(int page, int pageSize, string? keyword, string? order, int? categoryID, int? supplierID);
        public Task<SanPhamDTO> GetById(int id);
        public Task<SanPhamDTO> Create(SanPhamRequestDTO sp);
        public Task<bool> Delete(int id);
        public Task<SanPhamDTO> Update(int id, SanPhamRequestDTO sp);
        public Task<List<SanPhamDTO>> getBySupplierID(int? supplier_id);
        public Task<bool> checkExistBarcode(string barcode);
        public Task<bool> checkExistID(int ID);
        public Task<bool> checkBarcodeExistForOtherProducts(int id, string barcode);
        public Task<List<SanPhamDTO>> searchByCategoryAndSortOrderAndKeyword(int? categoryID, string? sortOrder, string? keyword);
        public Task<List<SanPhamDTO>> GetAllSP();
        public Task<PagedResult<SanPhamDTO>> GetAll2(int page, int pageSize, string? keyword, string? order, int? categoryID, int? supplierID, decimal? minPrice, decimal? maxPrice);
    }

}
