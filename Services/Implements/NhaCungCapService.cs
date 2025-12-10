using AutoMapper;
using BlazorStoreManagementWebApp.DTOs.Admin.NhaCungCap;
using BlazorStoreManagementWebApp.Helpers;
using BlazorStoreManagementWebApp.Models;
using BlazorStoreManagementWebApp.Models.Entities;
using BlazorStoreManagementWebApp.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BlazorStoreManagementWebApp.Services.Implements
{
    public class NhaCungCapService : INhaCungCapService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public NhaCungCapService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // Lấy danh sách + phân trang + tìm kiếm
        public async Task<PagedResult<NhaCungCapDTO>> GetAll(int page, int pageSize, string keyword)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

            // 1. Tìm kiếm trước (nếu keyword rỗng thì lấy tất cả)
            var query = SearchByKeyword(keyword);

            // 2. Đếm tổng số dòng sau khi tìm kiếm
            var total = await query.CountAsync();

            // 3. Phân trang 
            var list = await query
                .Skip((page - 1) * pageSize)    //copy Qhuong
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<NhaCungCapDTO>
            {
                Data = _mapper.Map<List<NhaCungCapDTO>>(list),
                Total = total,
                Page = page,
                PageSize = pageSize
            };
        }

        // hàm này xài cho product
        public async Task<List<NhaCungCapDTO>> GetAllNCC()
        {
            var list = await _context.NhaCungCaps.ToListAsync();
            //return list DTO
            return _mapper.Map<List<NhaCungCapDTO>>(list);
        }

        // controller gọi để kết hợp điều kiện
        public IQueryable<NhaCungCap> SearchByKeyword(string keyword)
        {
            var q = _context.NhaCungCaps.AsQueryable();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();
                q = q.Where(x =>
                    x.Name.Contains(keyword) ||
                    x.Email.Contains(keyword) ||
                    x.Phone.Contains(keyword) ||
                    x.Address.Contains(keyword)
                );
            }
            return q;
        }

        public async Task<NhaCungCapDTO?> GetById(int supplierId)
        {
            var entity = await _context.NhaCungCaps.FindAsync(supplierId);
            return entity == null ? null : _mapper.Map<NhaCungCapDTO>(entity);
        }

        public async Task<NhaCungCapDTO> Create(NhaCungCapDTO nhaCungCapDTO)
        {
            try
            {
                var entity = _mapper.Map<NhaCungCap>(nhaCungCapDTO);
                _context.NhaCungCaps.Add(entity);
                await _context.SaveChangesAsync();
                return _mapper.Map<NhaCungCapDTO>(entity);
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi thêm nhà cung cấp: " + ex.Message);
            }
        }

        public async Task<NhaCungCapDTO?> Update(int id, NhaCungCapDTO dto)
        {
            try
            {
                var entity = await _context.NhaCungCaps.FindAsync(id);
                if (entity == null) return null;

                // Cập nhật các field
                entity.Name = dto.Name?.Trim() ?? "";
                entity.Phone = dto.Phone?.Trim() ?? "";
                entity.Email = dto.Email?.Trim() ?? "";
                entity.Address = dto.Address?.Trim() ?? "";
                entity.Status = dto.Status;

                _context.NhaCungCaps.Update(entity);
                await _context.SaveChangesAsync();

                return _mapper.Map<NhaCungCapDTO>(entity);
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi cập nhật nhà cung cấp: " + ex.Message);
            }
        }

        public async Task<bool> Delete(int id)
        {
            try
            {
                var existing = await _context.NhaCungCaps
                    .Include(n => n.SanPhams)
                    .FirstOrDefaultAsync(n => n.SupplierId == id);

                if (existing == null)
                    return false;

                if (existing.SanPhams != null && existing.SanPhams.Any())
                {
                    throw new InvalidOperationException("Không thể xóa vì có sản phẩm đang thuộc nhà cung cấp này!");
                }

                _context.NhaCungCaps.Remove(existing);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi xóa nhà cung cấp: " + ex.Message);
            }
        }


        public async Task<bool> IsSupplierIdExist(int supplierId)
        {
            return await _context.NhaCungCaps.AnyAsync(x => x.SupplierId == supplierId);
        }

        // Kiểm tra trùng Name/Email/Phone (có ignoreId để Update)
        public async Task<bool> IsSupplierExist(string name, string email, string phone, int? ignoreId = null)
        {
            // Chuẩn hóa để tránh null reference
            name = name?.Trim() ?? "";
            email = email?.Trim() ?? "";
            phone = phone?.Trim() ?? "";

            var q = _context.NhaCungCaps.AsQueryable();

            // Khi update, loại trừ chính bản ghi đang sửa để không báo trùng với chính nó
            if (ignoreId.HasValue) q = q.Where(x => x.SupplierId != ignoreId.Value);

            // Trùng bất kỳ 1 trong 3 thuộc tính
            return await q.AnyAsync(x =>
                (!string.IsNullOrEmpty(name) && x.Name == name) ||
                (!string.IsNullOrEmpty(email) && x.Email == email) ||
                (!string.IsNullOrEmpty(phone) && x.Phone == phone));
        }
    }


}
