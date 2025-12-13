using System;

namespace BlazorStoreManagementWebApp.DTOs.Admin.DonHang
{
    public class DonHangDTO
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; } = "";
        public string Phone { get; set; } = "";
        
        // Trạng thái lấy từ DB (paid, pending, canceled)
        public string Status { get; set; } = ""; 
        
        // Cột mới: Mã giảm giá
        public string PromoCode { get; set; } = ""; 

        public DateTime? OrderDate { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; } // Hiển thị số tiền giảm
    }
}