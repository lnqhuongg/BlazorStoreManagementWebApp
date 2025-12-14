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
using BlazorStoreManagementWebApp.DTOs.Authentication;
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
            CreateMap<DonHang, DonHangDTO>()
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.Name : ""))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.FullName : ""))

                .ReverseMap();

            // entity chitietdonhang <-> chitietdonhangDTO
            CreateMap<ChiTietDonHang, ChiTietDonHangDTO>().ReverseMap();

            // entity thanhtoan <-> thanhtoanDTO
            CreateMap<ThanhToan, ThanhToanReturnDTO>().ReverseMap();

            // entity khachhang <-> khachhangDTO
            CreateMap<KhachHang, KhachHangDTO>().ReverseMap();

            // DangKyDTO -> KhachHang (chỉ map các property có chung tên)
            CreateMap<DangKyDTO, KhachHang>()
                .ForMember(dest => dest.CustomerId, opt => opt.Ignore()) // CustomerId được DB tự sinh
                .ForMember(dest => dest.RewardPoints, opt => opt.MapFrom(src => 0)) // Set mặc định
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore()); // CreatedAt được set trong service

            // entity magiamgia <-> magiamgiaDTO
            CreateMap<MaGiamGia, MaGiamGiaDTO>().ReverseMap();

            // entity tonkho <-> tonkhoDTO
            CreateMap<TonKho, TonKhoDTO>().ReverseMap();
        }
    }
}
