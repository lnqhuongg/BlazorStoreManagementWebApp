namespace BlazorStoreManagementWebApp.DTOs.Admin.NhaCungCap
{
    public class NhaCungCapDTO
    {
        public int SupplierId { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public int Status { get; set; } = 0;
    }
}