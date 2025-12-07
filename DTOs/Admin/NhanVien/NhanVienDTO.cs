namespace BlazorStoreManagementWebApp.DTOs.Admin.NhanVien
{
    public class NhanVienDTO
    {
        public int UserId { get; set; }
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Role { get; set; } = "staff";
        public DateTime CreatedAt { get; set; }
        public byte Status { get; set; } = 1;
    }
}
