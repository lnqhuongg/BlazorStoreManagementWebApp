using BlazorStoreManagementWebApp.DTOs.Admin.Authentication;
using BlazorStoreManagementWebApp.DTOs.Admin.NhanVien;
using BlazorStoreManagementWebApp.Helpers;

namespace BlazorStoreManagementWebApp.Services.Interfaces
{
    public interface IAuthService
    {
        Task<ServiceResult<UserResponseDTO>> CheckLoginAdmin(DangNhapDTO dto);
        Task<ServiceResult<UserResponseDTO>> CheckLoginClient(DangNhapDTO dto);
    }
}
