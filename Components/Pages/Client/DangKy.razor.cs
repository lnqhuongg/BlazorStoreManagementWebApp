using Blazored.SessionStorage;
using BlazorStoreManagementWebApp.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Text.RegularExpressions;

namespace BlazorStoreManagementWebApp.Components.Pages.Client
{
    public partial class DangKy : ComponentBase 
    {
        [Inject] public IAuthService AuthService { get; set; }
        [Inject] public IKhachHangService KhachHangService { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }
        [Inject] public ISessionStorageService SessionStorage { get; set; }
        [Inject] public IJSRuntime JSRuntime { get; set; }

        private string Name = "";
        private string Phone = "";
        private string Address = "";
        private string Email = "";
        private string Password = "";
        private string ConfirmPassword = "";

        private string? NameError;
        private string? PhoneError;
        private string? AddressError;
        private string? EmailError;
        private string? PasswordError;
        private string? ConfirmPasswordError;

        // Separate visibility state for password and confirm password
        private bool showPassword = false;
        private bool showConfirmPassword = false;

        private string passwordType => showPassword ? "text" : "password";
        private string confirmPasswordType => showConfirmPassword ? "text" : "password";

        private string icon => showPassword ? "fa-solid fa-eye-slash" : "fa-solid fa-eye";
        private string confirmIcon => showConfirmPassword ? "fa-solid fa-eye-slash" : "fa-solid fa-eye";

        private void TogglePasswordVisibility()
        {
            showPassword = !showPassword;
        }

        private void ToggleConfirmedPasswordVisibility()
        {
            showConfirmPassword = !showConfirmPassword;
        }

        private bool Validate()
        {
            // reset errors
            NameError = PhoneError = AddressError = EmailError = PasswordError = ConfirmPasswordError = null;

            if (string.IsNullOrWhiteSpace(Name))
            {
                NameError = "Vui lòng nhập họ và tên.";
            }
            else
            {
                // Require at least two words, each starting with an uppercase letter
                // Example valid: "Nguyễn Thành Nam"
                var namePattern = @"^\p{Lu}\p{Ll}+(?:\s\p{Lu}\p{Ll}+)+$";
                if (!Regex.IsMatch(Name.Trim(), namePattern))
                {
                    NameError = "Họ tên phải viết hoa chữ cái đầu và có phân tách bằng dấu cách (ví dụ: Trần Đức Bo).";
                }
            }

            if (string.IsNullOrWhiteSpace(Email))
            {
                EmailError = "Vui lòng nhập email.";
            }
            else
            {
                // must be gmail.com
                var emailPattern = @"^[A-Za-z0-9._%+-]+@gmail\.com$";
                if (!Regex.IsMatch(Email.Trim(), emailPattern, RegexOptions.IgnoreCase))
                {
                    EmailError = "Email phải có đuôi @gmail.com.";
                }
            }

            if (string.IsNullOrWhiteSpace(Phone))
            {
                PhoneError = "Vui lòng nhập số điện thoại.";
            }
            else
            {
                // 10 digits starting with 0
                var phonePattern = @"^0\d{9}$";
                if (!Regex.IsMatch(Phone.Trim(), phonePattern))
                {
                    PhoneError = "Số điện thoại phải có 10 chữ số và bắt đầu bằng 0.";
                }
            }

            if (string.IsNullOrWhiteSpace(Address))
            {
                AddressError = "Vui lòng nhập địa chỉ.";
            }
            // no regex for address per request

            if (string.IsNullOrWhiteSpace(Password))
            {
                PasswordError = "Vui lòng nhập mật khẩu.";
            }
            else
            {
                // at least 1 upper, 1 lower, 1 digit, 1 special, min length 6
                var pwdPattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{6,}$";
                if (!Regex.IsMatch(Password, pwdPattern))
                {
                    PasswordError = "Mật khẩu cần ít nhất 1 chữ hoa, 1 chữ thường, 1 số, 1 ký tự đặc biệt và tối thiểu 6 ký tự.";
                }
            }

            if (string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                ConfirmPasswordError = "Vui lòng xác nhận mật khẩu.";
            }
            else if (Password != ConfirmPassword)
            {
                ConfirmPasswordError = "Mật khẩu xác nhận không khớp.";
            }

            return NameError == null && PhoneError == null && AddressError == null &&
                   EmailError == null && PasswordError == null && ConfirmPasswordError == null;
        }

        private async Task HandleKeyDown(KeyboardEventArgs e)
        {
            if (e.Key == "Enter")
                await SignUpAsync();
        }

        private async Task SignUpAsync()
        {
            if (!Validate())
                return;

            // Await để chờ Task<bool> hoàn tất
            if (await KhachHangService.IsEmailExist(Email))
            {
                await JSRuntime.InvokeVoidAsync("showToast", "error", "Email đã tồn tại!");
                return;
            }

            if (await KhachHangService.IsPhoneExist(Phone))
            {
                await JSRuntime.InvokeVoidAsync("showToast", "error", "Số điện thoại đã tồn tại!");
                return;
            }

            // Gọi API đăng ký
            var result = await KhachHangService.Create(new DTOs.Authentication.DangKyDTO
            {
                Name = Name,
                Phone = Phone,
                Address = Address,
                Email = Email,
                Password = Password
            });

            if (result != null && !string.IsNullOrEmpty(result.Name))
            {
                await JSRuntime.InvokeVoidAsync("showToast", "success", "Đăng ký thành công!");
                //await JSRuntime.InvokeAsync<object>("showToast", "error", result.Message);
                await Task.Delay(1500);
                NavigationManager.NavigateTo("/dangnhap");
            }
            else
            {
                // Đăng ký thất bại
                await JSRuntime.InvokeVoidAsync("showToast", "error", "Đăng ký thất bại, vui lòng thử lại!");
            }
        }
    }
}
