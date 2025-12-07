using AutoMapper;
using BlazorStoreManagementWebApp.DTOs.Admin.NhanVien;
using BlazorStoreManagementWebApp.Helpers;
using BlazorStoreManagementWebApp.Models;
using BlazorStoreManagementWebApp.Models.Entities;
using BlazorStoreManagementWebApp.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BlazorStoreManagementWebApp.Services.Implements
{
    {
            return await _context.NhanViens.AnyAsync(x => x.UserId == userId);
        }
    }


}
