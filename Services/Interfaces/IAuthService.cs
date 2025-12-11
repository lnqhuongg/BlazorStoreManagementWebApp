using BlazorStoreManagementWebApp.DTOs.Admin.Authentication;
using BlazorStoreManagementWebApp.DTOs.Admin.NhanVien;
using BlazorStoreManagementWebApp.Helpers;

namespace BlazorStoreManagementWebApp.Services.Interfaces
{
    public interface IAuthService
    {
        Task<ServiceResult<UserResponseDTO>> CheckLogin(DangNhapDTO dto, string userType);
        Task<ServiceResult<UserResponseDTO>> LogoutAccount(string? username);
    }
}
