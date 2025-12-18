using BlazorStoreManagementWebApp.DTOs.Admin.DonHang;
using BlazorStoreManagementWebApp.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorStoreManagementWebApp.Components.Forms.Client
{
    public partial class ChiTietDH : ComponentBase
    {
        [Inject] private IJSRuntime JS { get; set; } = default!;
        [Inject] IDonHangService _donHangService { get; set; } = default!;
        [Parameter] public EventCallback<DonHangDTO> OnSelected { get; set; }

        private bool IsOpen;

        public DonHangDTO donHang { get; set; } = new();

        public void OpenModal()
        {
            IsOpen = true;
            StateHasChanged();
        }

        // New: open modal and load order by id
        public async Task OpenModal(int orderId)
        {
            await LoadData(orderId);
        }

        private void CloseModal()
        {
            IsOpen = false;
            StateHasChanged();
        }

        protected override async Task OnInitializedAsync()
        {
        }

        public async Task LoadData(int orderId)
        {
            var result = await _donHangService.GetById(orderId);

            if (result == null)
            {
                donHang = new DonHangDTO();
            }
            else
            {
                donHang = result;
            }

            IsOpen = true;
            await InvokeAsync(StateHasChanged);
        }

        private string DisplayPaymentMethods()
        {
            if (donHang.Payments == null || !donHang.Payments.Any())
                return "Chưa thanh toán";

            var methods = donHang.Payments
                .Select(p => p.PaymentMethod.ToLower() switch
                {
                    "cash" => "Tiền mặt",
                    "e-wallet" => "Ví điện tử",
                    "bank_transfer" => "Chuyển khoản",
                    "card" => "Thẻ tín dụng / Thẻ ghi nợ",
                    _ => p.PaymentMethod ?? "Không xác định"
                })
                .Distinct();

            return string.Join(", ", methods);
        }
    }
}