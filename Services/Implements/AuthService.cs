// Services/AuthService.cs
using AutoMapper;
using BlazorStoreManagementWebApp.DTOs.Authentication;
using BlazorStoreManagementWebApp.Helpers;
using BlazorStoreManagementWebApp.Models;
using BlazorStoreManagementWebApp.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BlazorStoreManagementWebApp.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;

        public AuthService(ApplicationDbContext context)
        {
            _context = context;   
        }
        
        public async Task<ServiceResult<UserResponseDTO>> CheckLoginAdmin (DangNhapDTO dto) 
        {
            var usernameLower = dto.Username?.Trim().ToLower() ?? string.Empty;
            var user = await _context.NhanViens
                .FirstOrDefaultAsync(u => u.Username.ToLower() == usernameLower);
            if (user == null)
            {
                return new ServiceResult<UserResponseDTO>
                {
                    Type = "error",
                    Message = "Tên đăng nhập hoặc mật khẩu không đúng.",
                    Data = null
                };
            }
            if (!(user.Role == "admin" || user.Role == "staff"))
            {
                return new ServiceResult<UserResponseDTO>
                {
                    Type = "error",
                    Message = "Tài khoản không có quyền truy cập giao diện quản lý.",
                    Data = null
                };
            }
                var isValid = BCrypt.Net.BCrypt.Verify(dto.Password ?? string.Empty, user.Password);
            if (!isValid)
            {
                return new ServiceResult<UserResponseDTO>
                {
                    Type = "error",
                    Message = "Tên đăng nhập hoặc mật khẩu không đúng.",
                    Data = null
                };
            }
            var userDto = new UserResponseDTO 
            {
                UserId = user.UserId,
                Username = user.Username,
                FullName = user.FullName,
                Role = user.Role
            };

            return new ServiceResult<UserResponseDTO>
            {
                Type = "success",
                Message = "Đăng nhập thành công.",
                Data = userDto
            };
        }

        public async Task<ServiceResult<UserResponseDTO>> CheckLoginClient (DangNhapDTO dto) 
        {
            var usernameLower = dto.Username?.Trim().ToLower() ?? string.Empty;
            var user = await _context.KhachHangs
                .FirstOrDefaultAsync(u => u.Email.ToLower() == usernameLower);
            if (user == null)
            {
                return new ServiceResult<UserResponseDTO>
                {
                    Type = "error",
                    Message = "Email hoặc mật khẩu không đúng.",
                    Data = null
                };
            }
            var isValid = BCrypt.Net.BCrypt.Verify(dto.Password ?? string.Empty, user.Password);
            if (!isValid)
            {
                return new ServiceResult<UserResponseDTO>
                {
                    Type = "error",
                    Message = "Sai mật khẩu, vui lòng thử lại!",
                    Data = null
                };
            }
            var userDto = new UserResponseDTO 
            {
                UserId = user.CustomerId,
                Username = user.Email,
                FullName = user.Name,
                Role = "client"
            };
            return new ServiceResult<UserResponseDTO>
            {
                Type = "success",
                Message = "Đăng nhập thành công.",
                Data = userDto
            };
        }
    }
}