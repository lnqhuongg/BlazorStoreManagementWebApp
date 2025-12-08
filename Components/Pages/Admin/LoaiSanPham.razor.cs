using BlazorStoreManagementWebApp.Components.Forms.Admin;
using BlazorStoreManagementWebApp.DTOs.Admin.LoaiSanPham;
using BlazorStoreManagementWebApp.Helpers;
using BlazorStoreManagementWebApp.Services.Interfaces;
using Microsoft.AspNetCore.Components;

namespace BlazorStoreManagementWebApp.Components.Pages.Admin
{
    public partial class LoaiSanPham : ComponentBase
    {
        //tiêm service vào để gọi
        [Inject] public ILoaiSanPhamService LoaiSanPhamService { get; set; }

        protected PagedResult<LoaiSanPhamDTO> LoaiData = new();
        protected int Page = 1;
        protected int PageSize = 2;
        protected string Keyword = "";

        private LoaiSanPhamForm LoaiSanPhamFormRef;

        protected override async Task OnInitializedAsync()
        {
            await LoadData();
        }

        // load dữ liệu 
        protected async Task LoadData()
        {
            LoaiData = await LoaiSanPhamService.GetAll(Page, PageSize, Keyword);
        }

        // di chuyển đến trang nào thì gọi hàm này
        protected async Task ChangePage(int newPage)
        {
            Page = newPage;
            await LoadData();
        }

        // tìm kiếm
        protected async Task Search()
        {
            Page = 1;
            await LoadData();
        }
    }
}

