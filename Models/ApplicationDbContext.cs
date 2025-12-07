using BlazorStoreManagementWebApp.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BlazorStoreManagementWebApp.Models { 
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
           : base(options)
        {
        }

        public DbSet<ChiTietDonHang> ChiTietDonHangs { set; get; }
        public DbSet<ChiTietPhieuNhap> ChiTietPhieuNhaps { set; get; }
        public DbSet<DonHang> DonHangs { set; get; }
        public DbSet<KhachHang> KhachHangs { set; get; }
        public DbSet<MaGiamGia> MaGiamGias { get; set; }
        public DbSet<LoaiSanPham> LoaiSanPhams { set; get; }
        public DbSet<NhaCungCap> NhaCungCaps { set; get; }
        public DbSet<NhanVien> NhanViens { set; get; }
        public DbSet<PhieuNhap> PhieuNhaps { set; get; }
        public DbSet<SanPham> SanPhams { set; get; }
        public DbSet<ThanhToan> ThanhToans { set; get; }
        public DbSet<TonKho> TonKhos { get; set; }
    }
}
