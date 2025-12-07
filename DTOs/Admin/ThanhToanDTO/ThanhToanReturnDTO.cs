namespace BlazorStoreManagementWebApp.DTOs.Admin.ThanhToanDTO
{
    public class ThanhToanReturnDTO
    {
        public int PaymentId { get; set; }
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = null!;
        public DateTime PaymentDate { get; set; }
    }
}
