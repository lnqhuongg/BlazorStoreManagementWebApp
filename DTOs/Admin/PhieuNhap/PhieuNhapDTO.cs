using BlazorStoreManagementWebApp.DTOs.Admin.ChiTietPhieuNhap;
using BlazorStoreManagementWebApp.DTOs.Admin.NhaCungCap;
using BlazorStoreManagementWebApp.DTOs.Admin.NhanVien;
using System.ComponentModel.DataAnnotations;

namespace BlazorStoreManagementWebApp.DTOs.Admin.PhieuNhap
{
    public class PhieuNhapDTO
    {
        public int ImportId { get; set; }
        public DateTime ImportDate { get; set; }

        [Required] //DataAnnotations
        public NhaCungCapDTO? Supplier { get; set; } // dung NhaCungCapDTO

        [Required]
        public NhanVienDTO? Staff { get; set; } // dung NhanVienDTO
        public decimal TotalAmount { get; set; }

        public List<ChiTietPhieuNhapDTO>? ImportDetails { get; set; }
    }
}
