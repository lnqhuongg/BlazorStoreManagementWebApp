using BlazorStoreManagementWebApp.DTOs.Admin.ChiTietPhieuNhap;

namespace BlazorStoreManagementWebApp.DTOs.Admin.PhieuNhap
{
    public class CreatePhieuNhapDTO
    {
        public int SupplierId { get; set; }
        public int UserId { get; set; }
        public List<CreateChiTietPNDTO>? ImportDetails { get; set; }
    }
}
