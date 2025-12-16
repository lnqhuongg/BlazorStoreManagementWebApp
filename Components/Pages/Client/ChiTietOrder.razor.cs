using BlazorStoreManagementWebApp.DTOs.Admin.DonHang;
using BlazorStoreManagementWebApp.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorStoreManagementWebApp.Components.Pages.Client
{
    public partial class ChiTietOrder : ComponentBase
    {
        [Parameter] public int OrderId { get; set; }
        
        [Inject] IDonHangService DonHangService { get; set; } = default!;
        [Inject] Blazored.SessionStorage.ISessionStorageService SessionStorage { get; set; } = default!;
        [Inject] IJSRuntime JS { get; set; } = default!;
        [Inject] NavigationManager Navigation { get; set; } = default!;

        protected DonHangDTO? Order { get; set; }
        protected bool IsLoading { get; set; } = true;
        private int customerId;

        protected override async Task OnInitializedAsync()
        {
            await LoadOrderDetail();
        }

        private async Task LoadOrderDetail()
        {
            try
            {
                IsLoading = true;

                // Lấy customerId từ session
                customerId = await SessionStorage.GetItemAsync<int>("clientId");

                if (customerId <= 0)
                {
                    await JS.InvokeAsync<object>("showToast", "error", "Vui lòng đăng nhập");
                    Navigation.NavigateTo("/dangnhap");
                    return;
                }

                // Lấy chi tiết đơn hàng
                Order = await DonHangService.GetById(OrderId);

                // Kiểm tra quyền truy cập (chỉ cho phép xem đơn hàng của mình)
                if (Order == null || Order.CustomerId != customerId)
                {
                    await JS.InvokeAsync<object>("showToast", "error", "Không tìm thấy đơn hàng hoặc bạn không có quyền xem");
                    Navigation.NavigateTo("/lichsudonhang");
                    return;
                }

                IsLoading = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading order detail: {ex.Message}");
                await JS.InvokeAsync<object>("showToast", "error", "Có lỗi khi tải chi tiết đơn hàng");
                Navigation.NavigateTo("/lichsudonhang");
            }
        }

        protected string FormatCurrency(decimal amount)
        {
            return amount.ToString("N0") + " VND";
        }

        protected string GetImageUrl(string? imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
                return "images/home/product-default.jpg";
            
            return imagePath;
        }

        protected void BackToHistory()
        {
            Navigation.NavigateTo("/lichsudonhang");
        }
    }
}