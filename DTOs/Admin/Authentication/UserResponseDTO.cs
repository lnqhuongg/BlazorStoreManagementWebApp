namespace BlazorStoreManagementWebApp.DTOs.Admin.Authentication
{
    public class UserResponseDTO
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
    }
}
