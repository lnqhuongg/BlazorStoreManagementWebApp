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

        // Lấy danh sách có phân trang + tìm kiếm
        public async Task<PagedResult<NhaCungCapDTO>> GetAll(int page, int pageSize, string keyword)
        {
            var query = SearchByKeyword(keyword);

            var total = await query.CountAsync();

            var list = await query
                .OrderBy(x => x.SupplierId)
                .Skip((page - 1) * pageSize)
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

        // Get all
        public async Task<List<NhaCungCapDTO>> GetAllNCC()
        {
            var list = await _context.NhaCungCaps
                .OrderBy(x => x.Name)
                .ToListAsync();

            return _mapper.Map<List<NhaCungCapDTO>>(list);
        }

        // Filter theo keyword
        public IQueryable<NhaCungCap> SearchByKeyword(string keyword)
        {
            var query = _context.NhaCungCaps.AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.ToLower();
                query = query.Where(x =>
                    x.Name.ToLower().Contains(keyword) ||
                    x.Phone.ToLower().Contains(keyword) ||
                    x.Address.ToLower().Contains(keyword) ||
                    x.Email.ToLower().Contains(keyword));
            }

            return query;
        }

        public async Task<NhaCungCapDTO?> GetById(int supplierId)
        {
            var entity = await _context.NhaCungCaps.FindAsync(supplierId);
            return entity == null ? null : _mapper.Map<NhaCungCapDTO>(entity);
        }

        public async Task<NhaCungCapDTO> Create(NhaCungCapDTO dto)
        {
            var entity = _mapper.Map<NhaCungCap>(dto);
            _context.NhaCungCaps.Add(entity);
            await _context.SaveChangesAsync();

            return _mapper.Map<NhaCungCapDTO>(entity);
        }

        public async Task<NhaCungCapDTO?> Update(int id, NhaCungCapDTO dto)
        {
            var existing = await _context.NhaCungCaps.FindAsync(id);
            if (existing == null) return null;

            existing.Name = dto.Name;
            existing.Phone = dto.Phone;
            existing.Email = dto.Email;
            existing.Address = dto.Address;
            existing.Status = dto.Status;

            await _context.SaveChangesAsync();

            return _mapper.Map<NhaCungCapDTO>(existing);
        }

        public async Task<bool> IsSupplierIdExist(int supplierId)
        {
            return await _context.NhaCungCaps
                .AnyAsync(x => x.SupplierId == supplierId);
        }

        // Check trùng Tên / Email / SĐT
        public async Task<bool> IsSupplierExist(string name, string email, string phone, int? ignoreId = null)
        {
            var q = _context.NhaCungCaps.AsQueryable();

            if (ignoreId.HasValue)
            {
                q = q.Where(x => x.SupplierId != ignoreId.Value);
            }

            return await q.AnyAsync(x =>
                (!string.IsNullOrEmpty(name) && x.Name == name) ||
                (!string.IsNullOrEmpty(email) && x.Email == email) ||
                (!string.IsNullOrEmpty(phone) && x.Phone == phone));
        }

        public async Task<bool> Delete(int supplierId)
        {
            var existing = await _context.NhaCungCaps
                .Include(x => x.SanPhams)
                .FirstOrDefaultAsync(x => x.SupplierId == supplierId);

            if (existing == null) return false;

            // Nếu đang có sản phẩm dùng NCC này thì không cho xóa
            if (existing.SanPhams.Any())
            {
                throw new InvalidOperationException("Không thể xóa vì đang có sản phẩm thuộc nhà cung cấp này!");
            }

            _context.NhaCungCaps.Remove(existing);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
