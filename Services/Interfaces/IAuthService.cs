using BlazorStoreManagementWebApp.DTOs.Authentication;
using BlazorStoreManagementWebApp.Helpers;

namespace BlazorStoreManagementWebApp.Services.Interfaces
{
    public interface IAuthService
    {
        Task<ServiceResult<UserResponseDTO>> CheckLoginAdmin(DangNhapDTO dto);
        Task<ServiceResult<UserResponseDTO>> CheckLoginClient(DangNhapDTO dto);
    }
}
