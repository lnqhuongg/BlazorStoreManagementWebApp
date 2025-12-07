using AutoMapper;
using BlazorStoreManagementWebApp.DTOs.Admin.LoaiSanPham;
using BlazorStoreManagementWebApp.Helpers;
using BlazorStoreManagementWebApp.Models;
using BlazorStoreManagementWebApp.Models.Entities;
using BlazorStoreManagementWebApp.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace BlazorStoreManagementWebApp.Services.Implements
{
    public class LoaiSanPhamService : ILoaiSanPhamService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public LoaiSanPhamService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // lay tat ca loai san pham
        public async Task<PagedResult<LoaiSanPhamDTO>> GetAll(int page, int pageSize, string keyword)
        {
            // 1. tìm kiếm trước (nếu mà keyword rỗng thì nó lấy tất cả)
            var query = SearchByKeyword(keyword);

            // 2. đếm tổng số bản ghi sau khi tìm kiếm
            var total = await query.CountAsync();

            // 3 phân trang
            // LẤY THEO TRANG, mỗi trang nó lấy theo cái pagesize mình khai báo bên trên, pagesize = 5
            // là lấy 5 bản ghi mỗi trang
            var list = await query
                .Skip((page - 1) * pageSize)  // Bỏ qua bao nhiêu? -- ví dụ trang đầu 1 - 1 * 5 = 0 -> lấy từ 1 đến pagesize = 5 
                .Take(pageSize)               // Lấy bao nhiêu?
                .ToListAsync();


            return new PagedResult<LoaiSanPhamDTO>
            {
                Data = _mapper.Map<List<LoaiSanPhamDTO>>(list),
                Total = total,
                Page = page,
                PageSize = pageSize
            };
        }

        public IQueryable<LoaiSanPham> SearchByKeyword(string keyword)
        {
            IQueryable<LoaiSanPham> query = _context.LoaiSanPhams;
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(x => x.CategoryName.Contains(keyword));
            }
            return query;
        }

        // lay ra 1 san pham theo id 
        public async Task<LoaiSanPhamDTO> GetById(int category_id)
        {
            var category = await _context.LoaiSanPhams.FindAsync(category_id);
            if (category == null) return null;
            else return _mapper.Map<LoaiSanPhamDTO>(category);
        }

        // tao moi 
        public async Task<LoaiSanPhamDTO> Create(LoaiSanPhamDTO loaiSanPhamDTO)
        {
            try
            {
                var loaiEntity = _mapper.Map<LoaiSanPham>(loaiSanPhamDTO);

                _context.LoaiSanPhams.Add(loaiEntity);

                await _context.SaveChangesAsync();

                return _mapper.Map<LoaiSanPhamDTO>(loaiEntity);
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi thêm loại sản phẩm: " + ex.Message);
            }
        }

        public async Task<LoaiSanPhamDTO?> Update(int id, LoaiSanPhamDTO dto)
        {
            try
            {
                var existing = await _context.LoaiSanPhams.FindAsync(id);
                if (existing == null)
                {
                    return null; // ← 404
                }

                existing.CategoryName = dto.CategoryName;

                await _context.SaveChangesAsync();

                return _mapper.Map<LoaiSanPhamDTO>(existing);
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi cập nhật loại sản phẩm: " + ex.Message);
            }
        }

        public async Task<bool> Delete(int id)
        {
            try
            {
                var existing = await _context.LoaiSanPhams
                    .Include(l => l.SanPhams)
                    .FirstOrDefaultAsync(l => l.CategoryId == id);

                if (existing == null)
                    return false;

                if (existing.SanPhams.Any())
                {
                    throw new InvalidOperationException("Không thể xóa vì có sản phẩm đang sử dụng loại này!");
                }

                _context.LoaiSanPhams.Remove(existing);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi xóa loại sản phẩm: " + ex.Message);
            }
        }

        public async Task<bool> isCategoryNameExist(string categoryName, int id = 0)
        {
            // id = 0 -> thêm 
            // truyền id vô để kiểm tra trùng lúc sửa ko bị trùng với chính nó
            return await _context.LoaiSanPhams
                .AnyAsync(x =>
                    x.CategoryName == categoryName &&
                    (id == 0 || x.CategoryId != id)
                );
        }

        public async Task<bool> isCategoryExist(int category_id)
        {
            bool CategoryExist = await _context.LoaiSanPhams
                .AnyAsync(x => x.CategoryId == category_id);
            return CategoryExist;
        }
    }


}
