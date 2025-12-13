using AutoMapper;
using BlazorStoreManagementWebApp.DTOs.Admin.ChiTietPhieuNhap;
using BlazorStoreManagementWebApp.DTOs.Admin.DonHang;
using BlazorStoreManagementWebApp.DTOs.Admin.KhachHang;
using BlazorStoreManagementWebApp.DTOs.Admin.LoaiSanPham;
using BlazorStoreManagementWebApp.DTOs.Admin.MaGiamGia;
using BlazorStoreManagementWebApp.DTOs.Admin.NhaCungCap;
using BlazorStoreManagementWebApp.DTOs.Admin.NhanVien;
using BlazorStoreManagementWebApp.DTOs.Admin.PhieuNhap;
using BlazorStoreManagementWebApp.DTOs.Admin.SanPham;
using BlazorStoreManagementWebApp.DTOs.Admin.ThanhToanDTO;
using BlazorStoreManagementWebApp.DTOs.Admin.TonKho;
using BlazorStoreManagementWebApp.Models.Entities;

namespace BlazorStoreManagementWebApp.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // entity loaisanpham <-> loaisanphamDTO --- reservemap de map nguoc lai, nghia la tu DTO ve entity
            CreateMap<LoaiSanPham, LoaiSanPhamDTO>().ReverseMap();

            // entity phieunhap <-> phieunhapDTO
            CreateMap<PhieuNhap, PhieuNhapDTO>().ReverseMap();
            // entity chitietphieunhap <-> chitietphieunhapDTO
            CreateMap<ChiTietPhieuNhap, ChiTietPhieuNhapDTO>().ReverseMap();

            // entity nhacungcap <-> nhacungcapDTO
            CreateMap<NhaCungCap, NhaCungCapDTO>().ReverseMap();

            // entity nhanvien <-> nhanvienDTO
            CreateMap<NhanVien, NhanVienDTO>().ReverseMap();

            // entity sanpham <-> sanphamDTO
            CreateMap<SanPham, SanPhamDTO>().ReverseMap();

            // entity nhacungcap <-> nhacungcapDTO
            CreateMap<NhaCungCap, NhaCungCapDTO>().ReverseMap();

            // entity donhang <-> donhangDTO
            //CreateMap<DonHang, DonHangDTO>()

            // entity chitietdonhang <-> chitietdonhangDTO
            CreateMap<ChiTietDonHang, ChiTietDonHangDTO>().ReverseMap();

            // entity thanhtoan <-> thanhtoanDTO
            CreateMap<ThanhToan, ThanhToanReturnDTO>().ReverseMap();

            // entity khachhang <-> khachhangDTO
            CreateMap<KhachHang, KhachHangDTO>().ReverseMap();

            // entity magiamgia <-> magiamgiaDTO
            CreateMap<MaGiamGia, MaGiamGiaDTO>().ReverseMap();

            // entity tonkho <-> tonkhoDTO
            CreateMap<TonKho, TonKhoDTO>().ReverseMap();
        }
    }
}
