using System;

namespace BlazorStoreManagementWebApp.DTOs.Admin.DonHang
{
    public class DonHangFilterDTO
    {
        public string Keyword { get; set; } = "";

        // Đặt tên StartDate/EndDate cho giống PhieuNhapFilter
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public decimal? MinPrice { get; set; } // Giống MinPrice bên Phiếu nhập
        public decimal? MaxPrice { get; set; }

        // (Optional) Nếu muốn giữ lọc trạng thái thì để, không thì xóa
        public string Status { get; set; } = "";
    }
}