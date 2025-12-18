using BlazorStoreManagementWebApp.DTOs.Admin.DonHang;
using BlazorStoreManagementWebApp.Helpers;
using BlazorStoreManagementWebApp.Models.Entities;

namespace BlazorStoreManagementWebApp.Services.Interfaces
{
    public interface IDonHangService
    {
        // 1. Lấy danh sách có phân trang & lọc (Giống GetAll bên LoaiSanPham)
        Task<PagedResult<DonHangDTO>> GetAll(int page, int pageSize, string keyword, string status = "");

        // 2. Hàm lọc (Thay thế cho SearchByKeyword vì đơn hàng cần lọc nhiều tiêu chí hơn)
        IQueryable<DonHang> ApplyFilter(DonHangFilterDTO filter);

        // 3. Lấy chi tiết đơn hàng theo ID
        Task<DonHangDTO?> GetById(int orderId);

        // 4. Tạo mới đơn hàng (Giống Create bên LoaiSanPham)
        Task<DonHangDTO> Create(CreateDonHangDTO dto, string userType = "staff", string paymentMethod = "cash");

        Task<List<DonHangDTO>> GetTodayOrders();

        //long TinhTongDoanhThu(string mode, int month, int year);
        Task<long> TinhTongDoanhThu(string mode, int month, int year);
        public List<long> GetRevenueByMonth(int month, int year);
        public List<long> GetRevenueByYear(int year);
        Task<DonHangDTO> UpdateOrderStatus(int orderId, string status);
        Task<PagedResult<DonHangDTO>> GetByKhachHangId(int page, int pageSize, string status = "", string startday = "", string endday = "", int idKH = 0);
    }

}
