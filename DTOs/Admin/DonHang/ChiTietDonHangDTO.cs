using System.Collections.Generic;

namespace BlazorStoreManagementWebApp.DTOs.Admin.DonHang
{
    // [QUAN TRỌNG] Phải kế thừa DonHangDTO thì mới có CustomerName, Phone...
    public class ChiTietDonHangDTO : DonHangDTO
    {
        public string Address { get; set; } = "";
        public string Email { get; set; } = "";

        // Danh sách sản phẩm bên trong
        public List<DonHangItemDTO> DanhSachSanPham { get; set; } = new();
    }

    public class DonHangItemDTO
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public string ImageUrl { get; set; } = "";
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Subtotal { get; set; }
    }
}