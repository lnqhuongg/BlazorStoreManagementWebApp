// Infrastructure/ServiceCollectionExtensions.cs
using BlazorStoreManagementWebApp.Services.Implements;
using BlazorStoreManagementWebApp.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace StoreManagementBE.BackendServer.Infrastructure.DI
{
    public static class ServiceCollectionExtensions
    {
        // IServiceCollection la giao dien co san trong ASP.NET Core de dang ky cac dich vu (services) cho ung dung
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Đăng ký tất cả service ở đây
            services.AddScoped<INhanVienService, NhanVienService>();
            services.AddScoped<ILoaiSanPhamService, LoaiSanPhamService>();
            services.AddScoped<ISanPhamService, SanPhamService>();
            services.AddScoped<IImageService, ImageService>();
            services.AddScoped<IPhieuNhapService, PhieuNhapService>();
            services.AddScoped<INhaCungCapService, NhaCungCapService>();
            services.AddScoped<IMaGiamGiaService, MaGiamGiaService>();
            services.AddScoped<IKhachHangService, KhachHangService>();
            services.AddScoped<IDonHangService, DonHangService>();
            services.AddScoped<PdfService>();

            return services;
        }
    }
}