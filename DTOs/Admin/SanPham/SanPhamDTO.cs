using BlazorStoreManagementWebApp.DTOs.Admin.LoaiSanPham;
using BlazorStoreManagementWebApp.DTOs.Admin.NhaCungCap;

namespace BlazorStoreManagementWebApp.DTOs.Admin.SanPham
{
    public class SanPhamDTO
    {
        public int ProductID { get; set; }
        public int? SupplierID { get; set; }
        public int? CategoryID { get; set; }
        public LoaiSanPhamDTO Category { get; set; }
        public NhaCungCapDTO Supplier { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Barcode { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Unit { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string? ImageUrl { get; set; }
        //public int? stock { get; set; } 

        public int Status { get; set; }

    }
}
