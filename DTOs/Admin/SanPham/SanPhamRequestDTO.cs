namespace BlazorStoreManagementWebApp.DTOs.Admin.SanPham
{
    public class SanPhamRequestDTO
    {
        public int? SupplierID { get; set; }
        public int? CategoryID { get; set; }
        //public LoaiSanPhamDTO Category { get; set; }
        //public NhaCungCapDTO Supplier { get; set; }
        public string ProductName { get; set; }
        public string? Barcode { get; set; }
        public decimal Price { get; set; }
        public string Unit { get; set; }
        //public DateTime CreatedAt { get; set; }
        public IFormFile? ImageUrl { get; set; }
        //public int? stock { get; set; } = 0;
        public int Status { get; set; }
    }
}
