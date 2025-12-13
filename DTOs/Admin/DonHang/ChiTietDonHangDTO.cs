using BlazorStoreManagementWebApp.DTOs.Admin.SanPham;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorStoreManagementWebApp.DTOs.Admin.DonHang
{
    public class ChiTietDonHangDTO : DonHangDTO
    {
        public string Address { get; set; } = "";
        public string Email { get; set; } = "";

        // Danh sách sản phẩm bên trong
        public List<DonHangItemDTO> DanhSachSanPham { get; set; } = new();
    }

    // Class đại diện cho từng dòng sản phẩm (để tránh trùng tên với Entity ChiTietDonHang)
    public class DonHangItemDTO
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public string ImageUrl { get; set; } = ""; // Thêm ảnh nếu cần
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Subtotal { get; set; }
    }
}
