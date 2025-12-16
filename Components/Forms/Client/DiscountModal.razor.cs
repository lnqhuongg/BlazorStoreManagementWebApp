using BlazorStoreManagementWebApp.DTOs.Admin.LoaiSanPham;
using BlazorStoreManagementWebApp.DTOs.Admin.MaGiamGia;
using BlazorStoreManagementWebApp.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BlazorStoreManagementWebApp.Components.Forms.Client
{
    public partial class DiscountModal : ComponentBase
    {
        [Inject] IMaGiamGiaService  MaGiamGiaService { get; set; } = default!;
        [Inject] private IJSRuntime JS { get; set; } = default!;
        [Parameter] public decimal Subtotal { get; set; }
        [Parameter] public EventCallback<MaGiamGiaDTO> OnSelected { get; set; }

        private bool IsOpen;
        private string SearchText = "";
        private List<MaGiamGiaDTO> Promos = new();

        private bool CanUsePromo(MaGiamGiaDTO promo)
        => Subtotal >= promo.MinOrderAmount;

        protected override async Task OnInitializedAsync()
        {
            Promos = await MaGiamGiaService.GetAllActive();
        }

        public void OpenModal()
        {
            IsOpen = true;
            StateHasChanged();
        }

        private void CloseModal()
        {
            IsOpen = false;
        }

        private IEnumerable<MaGiamGiaDTO> FilteredPromos =>
            Promos.Where(x =>
                string.IsNullOrWhiteSpace(SearchText) ||
                x.PromoCode.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

        private async Task SelectPromo(MaGiamGiaDTO promo)
        {
            await OnSelected.InvokeAsync(promo);
            CloseModal();
        }

        private string DisplayDiscountDate (MaGiamGiaDTO promo)
        {
            return $"{promo.StartDate:dd/MM/yyyy} - {promo.EndDate:dd/MM/yyyy}";
        }

        private string DisplayDiscountUsage(MaGiamGiaDTO promo)
        {
            return $"Đã được dùng {promo.UsedCount} trên {promo.UsageLimit}";
        }
    }
}
