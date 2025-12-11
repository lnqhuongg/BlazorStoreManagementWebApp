using BlazorStoreManagementWebApp.Components.Forms.Admin;
using BlazorStoreManagementWebApp.DTOs.Admin.NhanVien;
using BlazorStoreManagementWebApp.Helpers;
using BlazorStoreManagementWebApp.Services.Interfaces;
using Microsoft.AspNetCore.Components;

namespace BlazorStoreManagementWebApp.Components.Pages.Admin
{
    public partial class NhanVien : ComponentBase
    {
        [Inject] public INhanVienService NhanVienService { get; set; } = default!;

        protected PagedResult<NhanVienDTO> NhanVienData = new();
        protected int Page = 1;
        protected int PageSize = 10;
        protected NhanVienFilterDTO Filter = new();

        private NhanVienForm NhanVienFormRef = default!;

        protected override async Task OnInitializedAsync()
        {
            await LoadData();
        }

        protected async Task LoadData()
        {
            NhanVienData = await NhanVienService.GetAll(Page, PageSize, Filter);
        }

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
    }
}