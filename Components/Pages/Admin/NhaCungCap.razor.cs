using BlazorStoreManagementWebApp.Components.Forms.Admin;
using BlazorStoreManagementWebApp.DTOs.Admin.NhaCungCap;
using BlazorStoreManagementWebApp.Helpers;
using BlazorStoreManagementWebApp.Services.Interfaces;
using Microsoft.AspNetCore.Components;

namespace BlazorStoreManagementWebApp.Components.Pages.Admin
{
    public partial class NhaCungCap : ComponentBase
    {

        [Inject] public INhaCungCapService NhaCungCapService { get; set; } 

        protected PagedResult<NhaCungCapDTO> SupplierData = new();
        protected int Page = 1;
        protected int PageSize = 5;
        protected string Keyword = "";

        private NhaCungCapForm NhaCungCapFormRef;

        protected override async Task OnInitializedAsync()
        {
            await LoadData();
        }

        // Load data
        protected async Task LoadData()
        {
            SupplierData = await NhaCungCapService.GetAll(Page, PageSize, Keyword);
        }

        // Đổi page thì gọi hàm này
        protected async Task ChangePage(int newPage)
        {
            Page = newPage;
            await LoadData();
        }

        protected async Task Search()
        {
            Page = 1;
            await LoadData();
        }

        //protected async Task DeleteSupplier(int id)
        //{
        //    var ok = await NhaCungCapService.Delete(id);
        //    if (ok)
        //    {
        //        await LoadData();
        //    }
        //    // nếu muốn có thông báo lỗi thì có thể inject IJSRuntime và alert
        //}
    }
}
