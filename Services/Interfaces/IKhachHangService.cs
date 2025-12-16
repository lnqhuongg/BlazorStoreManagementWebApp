using BlazorStoreManagementWebApp.DTOs.Admin.KhachHang;
using BlazorStoreManagementWebApp.DTOs.Authentication;
using BlazorStoreManagementWebApp.Helpers;
using BlazorStoreManagementWebApp.Models.Entities;

namespace BlazorStoreManagementWebApp.Services.Interfaces
{
    public interface IKhachHangService
    {
        // lấy tất cả khách hàng + phân trang 
        Task<PagedResult<KhachHangDTO>> GetAll(int page, int pageSize, string keyword);

        // tìm kiếm theo từ khóa (tên, sđt, email)
        IQueryable<KhachHang> SearchByKeyword(string keyword);

        // lấy ra 1 khách hàng theo id 
        Task<KhachHangDTO> GetById(int id);

        // tạo mới khách hàng(đã tự kiểm tra trùng email + sđt)
        Task<KhachHangDTO> Create(DangKyDTO dto);

        // cập nhật khách hàng (chỉ cho sửa Tên và Địa chỉ, KHÔNG cho sửa SĐT, Email, Điểm)
        Task<KhachHangDTO?> Update(int id, KhachHangDTO dto);

        // kiểm tra phone đã tồn tại chưa (chỉ dùng khi tạo mới)
        Task<bool> IsPhoneExist(string phone);

        // kiểm tra email đã tồn tại chưa (chỉ dùng khi tạo mới)
        Task<bool> IsEmailExist(string email);

        // kiểm tra khách hàng có tồn tại không
        Task<bool> IsCustomerExist(int customerId);

        // tìm khách hàng theo số điện thoại
        Task<KhachHangDTO> findByPhone(string phone);

        // thêm điểm thưởng cho khách hàng
        Task<KhachHangDTO?> addRewardPoints(int? customerId);

        // trừ điểm thưởng cho khách hàng
        Task<KhachHangDTO?> deductRewardPoints(int? customerId, int? pointsToDeduct);
        // admin reset mật khẩu khách hàng
        Task<KhachHangDTO> AdminResetPasswordAsync(int customerId, AdminResetPasswordDTO dto);
    }

}
