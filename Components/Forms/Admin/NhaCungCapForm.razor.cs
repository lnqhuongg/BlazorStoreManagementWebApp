﻿using BlazorStoreManagementWebApp.DTOs.Admin.NhaCungCap;
using BlazorStoreManagementWebApp.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions; //regex cho số điện thoại


namespace BlazorStoreManagementWebApp.Components.Forms.Admin
{
    public partial class NhaCungCapForm : ComponentBase
    {
        [Inject] private INhaCungCapService NhaCungCapService { get; set; } = default!;
        [Inject] private IJSRuntime JS { get; set; } = default!;

        [Parameter] public EventCallback OnSuccess { get; set; }

        protected NhaCungCapDTO supplierDTO { get; set; } = new();
        protected bool IsEditMode { get; set; } = false;

        protected string NameError { get; set; } = "";
        protected string PhoneError { get; set; } = "";
        protected string EmailError { get; set; } = "";
        protected string AddressError { get; set; } = "";

        public async Task OpenCreate()
        {
            supplierDTO = new NhaCungCapDTO
            {
                Status = 1
            };
            IsEditMode = false;
            ClearErrors();
            StateHasChanged();
            await JS.InvokeVoidAsync("showBootstrapModal", "addSupplierModal");
            
        }

        public async Task OpenUpdate(NhaCungCapDTO dto)
        {
            supplierDTO = new NhaCungCapDTO
            {
                SupplierId = dto.SupplierId,
                Name = dto.Name,
                Phone = dto.Phone,
                Email = dto.Email,
                Address = dto.Address,
                Status = dto.Status
            };
            IsEditMode = true;
            ClearErrors();
            StateHasChanged();
            await JS.InvokeVoidAsync("showBootstrapModal", "addSupplierModal");
           
        }

        private void ClearErrors()
        {
            NameError = PhoneError = EmailError = AddressError = "";
        }

        private bool Validate()
        {
            ClearErrors();
            bool ok = true;

            if (string.IsNullOrWhiteSpace(supplierDTO.Name))
            {
                NameError = "Tên nhà cung cấp không được để trống.";
                ok = false;
            }

            if (string.IsNullOrWhiteSpace(supplierDTO.Phone))
            {
                PhoneError = "Số điện thoại không được để trống.";
                ok = false;
            }
            else if (!Regex.IsMatch(supplierDTO.Phone, @"^\d{10}$"))
            {
                PhoneError = "Sai định dạng số điện thoại.";
                ok = false;
            }

            if (string.IsNullOrWhiteSpace(supplierDTO.Email))
            {
                EmailError = "Email không được để trống.";
                ok = false;
            }

            else if (!string.IsNullOrWhiteSpace(supplierDTO.Email))
            {
                var emailAttr = new EmailAddressAttribute();
                if (!emailAttr.IsValid(supplierDTO.Email))
                {
                    EmailError = "Email không hợp lệ.";
                    ok = false;
                }
            }

            if (string.IsNullOrWhiteSpace(supplierDTO.Address))
            {
                AddressError = "Địa chỉ không được để trống.";
                ok = false;
            }

            return ok;
        }

        protected async Task HandleSubmit()
        {
            if (!Validate()) return;

            // Check trùng Name/Email/Phone
            bool isDuplicate = await NhaCungCapService.IsSupplierExist(
                supplierDTO.Name,
                supplierDTO.Email,
                supplierDTO.Phone,
                IsEditMode ? supplierDTO.SupplierId : null
            );

            if (isDuplicate)
            {
                await JS.InvokeVoidAsync("alert", "Tên / Email / SĐT nhà cung cấp đã tồn tại!");
                return;
            }

            if (IsEditMode)
            {
                await NhaCungCapService.Update(supplierDTO.SupplierId, supplierDTO);
                await JS.InvokeAsync<object>("showToast", "success", "Cập nhật nhà cung cấp thành công!");
            }
            else
            {
                await NhaCungCapService.Create(supplierDTO);
                await JS.InvokeAsync<object>("showToast", "success", "Thêm nhà cung cấp mới thành công!");
            }

            await JS.InvokeVoidAsync("hideBootstrapModal", "addSupplierModal");
            await OnSuccess.InvokeAsync();
        }
    }
}