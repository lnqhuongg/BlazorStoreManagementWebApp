namespace BlazorStoreManagementWebApp.DTOs.Admin.ThanhToanDTO
{
    public class CreateThanhToanDTO
    {
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = null!;
    }
}
