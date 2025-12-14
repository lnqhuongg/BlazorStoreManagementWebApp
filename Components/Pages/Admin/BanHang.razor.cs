using BlazorStoreManagementWebApp.DTOs.Admin.LoaiSanPham;
using BlazorStoreManagementWebApp.DTOs.Admin.SanPham;
using BlazorStoreManagementWebApp.Helpers;
using BlazorStoreManagementWebApp.Services.Interfaces;
using Microsoft.AspNetCore.Components;

namespace BlazorStoreManagementWebApp.Components.Pages.Admin
{
    public partial class BanHang : ComponentBase
    {
        [Inject] public ISanPhamService sanPhamService { get; set; }
        [Inject] public ITonKhoService tonKhoService { get; set; }
        [Inject] public IKhachHangService khachHangService { get; set; }

        protected PagedResult<SanPhamDTO> SPData = new();
        protected List<LoaiSanPhamDTO> LSPData = new();

        protected int Page = 1;
        protected int PageSize = 5;
        protected string? Keyword = "";


    }
}
