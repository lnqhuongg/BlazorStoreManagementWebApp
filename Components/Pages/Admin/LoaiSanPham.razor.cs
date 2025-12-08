using Microsoft.AspNetCore.Components;
using BlazorStoreManagementWebApp.Services.Interfaces;
using BlazorStoreManagementWebApp.DTOs.Admin.LoaiSanPham;
using BlazorStoreManagementWebApp.Helpers;

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

        protected void Edit(LoaiSanPhamDTO dto)
        {
            // xử lý mở modal
        }

        protected async Task Delete(int id)
        {
            await LoaiSanPhamService.Delete(id);
            await LoadData();
        }

        protected void OpenAddModal()
        {
            // mở modal thêm
        }
    }
}

