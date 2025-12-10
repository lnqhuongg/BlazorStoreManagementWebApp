using BlazorStoreManagementWebApp.Components.Forms.Admin;
using BlazorStoreManagementWebApp.DTOs.Admin.KhachHang;
using BlazorStoreManagementWebApp.Helpers;
using BlazorStoreManagementWebApp.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BlazorStoreManagementWebApp.Components.Pages.Admin
{
    public partial class KhachHang : ComponentBase
    {
        [Inject] public IKhachHangService KhachHangService { get; set; } = default!;

        protected PagedResult<KhachHangDTO> KhachHangData = new();
        protected int Page = 1;
        protected int PageSize = 10;
        protected string Keyword = "";

        private KhachHangForm? KhachHangFormRef;

        protected override async Task OnInitializedAsync()
        {
            await LoadData();
        }

        protected async Task LoadData()
        {
            KhachHangData = await KhachHangService.GetAll(Page, PageSize, Keyword.Trim());
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

        private async Task OnKeyUp(KeyboardEventArgs e)
        {
            if (e.Key == "Enter")
            {
                await Search();
            }
        }
    }
}