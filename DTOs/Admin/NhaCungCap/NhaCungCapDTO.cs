namespace BlazorStoreManagementWebApp.DTOs.Admin.NhaCungCap
{
    public class NhaCungCapDTO
    {
        public int SupplierId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public bool Status { get; set; } = true; // Mapping từ bit(1)
    }
}