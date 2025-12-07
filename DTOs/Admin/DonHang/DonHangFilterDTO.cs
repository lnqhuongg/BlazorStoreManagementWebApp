namespace BlazorStoreManagementWebApp.DTOs.Admin.DonHang
{
    public class DonHangFilterDTO
    {
        public string? Keyword { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public decimal? MinTotal { get; set; }
        public decimal? MaxTotal { get; set; }
    }
}
