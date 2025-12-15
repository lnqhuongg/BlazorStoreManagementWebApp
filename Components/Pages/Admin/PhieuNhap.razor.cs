using BlazorStoreManagementWebApp.Components.Forms.Admin;
using BlazorStoreManagementWebApp.DTOs.Admin.PhieuNhap;
using BlazorStoreManagementWebApp.Helpers;
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

        protected PagedResult<PhieuNhapDTO> PhieuNhapData = new();
        protected int Page = 1;
        protected int PageSize = 5;
        private PhieuNhapFilter InputFilter { get; set; } = new PhieuNhapFilter();

        private PhieuNhapForm PhieuNhapFormRef;

        private bool ShowPdfModal = false;
        private string Base64Pdf = "";
        private PhieuNhapDTO selected = new PhieuNhapDTO();
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

        private async Task PreviewPdf(PhieuNhapDTO phieu)
        {
            selected = phieu;
            // 1. Gọi service để lấy mảng byte[]
            byte[] pdfBytes = PdfService.ExportPhieuNhap(phieu);

            // 2. Convert sang Base64 để nhét vào iframe
            Base64Pdf = Convert.ToBase64String(pdfBytes);

            // 3. Hiện modal preview
            ShowPdfModal = true;
            await Task.Delay(50);
            // 4. Gọi JS để load PDF vào iframe
            await JS.InvokeVoidAsync("showPdfPreview", Base64Pdf);
        }


        private async Task PrintPdf()
        {
            await JS.InvokeVoidAsync("downloadFileFromBase64",
                $"PhieuNhap_{selected.ImportId}.pdf", Base64Pdf);
        }


        private void ClosePreview()
        {
            ShowPdfModal = false;
        }


    }
}
