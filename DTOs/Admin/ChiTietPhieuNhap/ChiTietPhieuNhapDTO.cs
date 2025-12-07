using BlazorStoreManagementWebApp.DTOs.Admin.SanPham;

namespace BlazorStoreManagementWebApp.DTOs.Admin.ChiTietPhieuNhap
{
    public class ChiTietPhieuNhapDTO
    {
        public int ImportDetailId { get; set; }
        public int ImportId { get; set; }
        public SanPhamDTO Product { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Subtotal { get; set; }
    }
}
