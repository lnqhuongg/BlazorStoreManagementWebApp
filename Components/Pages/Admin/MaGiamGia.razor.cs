using BlazorStoreManagementWebApp.Components.Forms.Admin;
using BlazorStoreManagementWebApp.DTOs.Admin.MaGiamGia;
using BlazorStoreManagementWebApp.Helpers;
using BlazorStoreManagementWebApp.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BlazorStoreManagementWebApp.Components.Pages.Admin
{
    public partial class MaGiamGia : ComponentBase
    {
        [Inject] private IMaGiamGiaService MaGiamGiaService { get; set; } = default!;
        private MaGiamGiaForm MaGiamGiaFormRef = default!;
        private PagedResult<MaGiamGiaDTO>? PromoData;
        private int Page = 1;
        private int PageSize = 10;
        private string Keyword = "";
        private string DiscountType = "";

        protected override async Task OnInitializedAsync()
        {
            await LoadData();
        }

        protected async Task LoadData()
        {
            PromoData = await MaGiamGiaService.GetAll(
                Page,
                PageSize,
                Keyword,
                DiscountType
            );
        }


        private async Task Search()
        {
            Page = 1;
            await LoadData();
        }

        private async Task ChangePage(int newPage)
        {
            Page = newPage;
            await LoadData();
        }
        private async Task OnSearchKeyDown(KeyboardEventArgs e)
        {
            if (e.Key == "Enter")
            {
                await Search();
            }
        }
        private async Task FilterChanged(ChangeEventArgs e)
        {
            Page = 1;
            await LoadData();
        }


    }
}
