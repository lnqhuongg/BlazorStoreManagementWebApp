using BlazorStoreManagementWebApp.Components.Forms.Admin;
using BlazorStoreManagementWebApp.DTOs.Admin.PhieuNhap;
using BlazorStoreManagementWebApp.Helpers;
using BlazorStoreManagementWebApp.Services.Implements;
using BlazorStoreManagementWebApp.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorStoreManagementWebApp.Components.Pages.Admin
{
    public partial class PhieuNhap : ComponentBase
    {
        [Inject] public IPhieuNhapService PhieuNhapService { get; set; }
        [Inject] private IJSRuntime JS { get; set; } = default!;

        [Inject] PdfService PdfService { get; set; }
        [Inject] IHttpContextAccessor HttpContextAccessor { get; set; }

        protected PagedResult<PhieuNhapDTO> PhieuNhapData = new();
        protected int Page = 1;
        protected int PageSize = 5;
        private PhieuNhapFilter InputFilter { get; set; } = new PhieuNhapFilter();

        private PhieuNhapForm PhieuNhapFormRef;



        protected override async Task OnInitializedAsync()
        {
            await LoadData();
        }

        protected async Task LoadData()
        {
            PhieuNhapData = await PhieuNhapService.GetAll(InputFilter, Page, PageSize);
        }

        protected async Task ChangePage(int newPage)
        {
            Page = newPage;
            await LoadData();
        }

        private async Task Search()
        {
            // gọi service đọc danh sách sau khi lọc
            PhieuNhapData = await PhieuNhapService.GetAll(InputFilter, Page, PageSize);

            StateHasChanged();
        }

        protected async Task ClearFilter()
        {
            if (InputFilter != null)
            {
                InputFilter = new PhieuNhapFilter();
            }

            await LoadData();
        }

        public async Task PrintPdf(PhieuNhapDTO phieu)
        {
            var bytes = PdfService.ExportPhieuNhap(phieu);
            var base64 = Convert.ToBase64String(bytes);

            await JS.InvokeVoidAsync("downloadFileFromBase64",
                $"PhieuNhap_{phieu.ImportId}.pdf",
                base64);
        }

    }
}
