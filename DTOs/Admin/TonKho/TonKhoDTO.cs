namespace BlazorStoreManagementWebApp.DTOs.Admin.TonKho
{
    public class TonKhoDTO
    {
        public int InventoryId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; } = 0;
        public DateTime UpdatedAt { get; set; }
    }
}
