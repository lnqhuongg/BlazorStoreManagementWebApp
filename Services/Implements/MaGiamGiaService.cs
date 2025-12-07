using AutoMapper;
using BlazorStoreManagementWebApp.DTOs.Admin.MaGiamGia;
using BlazorStoreManagementWebApp.Helpers;
using BlazorStoreManagementWebApp.Models;
using BlazorStoreManagementWebApp.Models.Entities;
using BlazorStoreManagementWebApp.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BlazorStoreManagementWebApp.Services.Implements
{
    public class MaGiamGiaService : IMaGiamGiaService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public MaGiamGiaService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PagedResult<MaGiamGiaDTO>> GetAll(int page, int pageSize, string? keyword, string? discountType)
        {
            var query = _context.MaGiamGias.AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                var lowerKeyword = keyword.ToLower();
                query = query.Where(x => x.PromoCode.ToLower().Contains(lowerKeyword) ||
                                         (x.Description != null && x.Description.ToLower().Contains(lowerKeyword)));
            }

            if (!string.IsNullOrEmpty(discountType))
            {
                query = query.Where(x => x.DiscountType == discountType);
            }

            var total = await query.CountAsync();
            var data = await query
                .OrderByDescending(x => x.PromoId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<MaGiamGiaDTO>
            {
                Data = _mapper.Map<List<MaGiamGiaDTO>>(data),
                Total = total,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<List<MaGiamGiaDTO>> GetAllActive()
        {
            var entities = await _context.MaGiamGias
                .Where(x => x.Status == "Active" &&
                            (x.StartDate == null || x.StartDate <= DateTime.UtcNow) &&
                            (x.EndDate == null || x.EndDate >= DateTime.UtcNow))
                .ToListAsync();

            return _mapper.Map<List<MaGiamGiaDTO>>(entities);
        }

        public async Task<MaGiamGiaDTO?> GetById(int id)
        {
            var entity = await _context.MaGiamGias.FindAsync(id);
            return entity == null ? null : _mapper.Map<MaGiamGiaDTO>(entity);
        }

        public async Task<MaGiamGiaDTO> Create(MaGiamGiaDTO dto)
        {
            try
            {
                var exists = await _context.MaGiamGias.AnyAsync(x => x.PromoCode == dto.PromoCode);
                if (exists)
                {
                    throw new Exception("Mã giảm giá đã tồn tại!");
                }

                var entity = _mapper.Map<MaGiamGia>(dto);
                _context.MaGiamGias.Add(entity);
                await _context.SaveChangesAsync();

                return _mapper.Map<MaGiamGiaDTO>(entity);
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi thêm mã giảm giá: " + ex.Message);
            }
        }

        public async Task<MaGiamGiaDTO?> Update(int id, MaGiamGiaDTO dto)
        {
            try
            {
                var existing = await _context.MaGiamGias.FindAsync(id);
                if (existing == null)
                {
                    return null;
                }

                if (dto.PromoCode != existing.PromoCode && await _context.MaGiamGias.AnyAsync(x => x.PromoCode == dto.PromoCode))
                {
                    throw new Exception("Mã giảm giá đã được sử dụng bởi mã khác!");
                }

                existing.PromoCode = dto.PromoCode;
                existing.Description = dto.Description;
                existing.DiscountType = dto.DiscountType;
                existing.DiscountValue = dto.DiscountValue;
                existing.StartDate = dto.StartDate;
                existing.EndDate = dto.EndDate;
                existing.MinOrderAmount = dto.MinOrderAmount;
                existing.UsageLimit = dto.UsageLimit;
                existing.UsedCount = dto.UsedCount;
                existing.Status = dto.Status;

                await _context.SaveChangesAsync();

                return _mapper.Map<MaGiamGiaDTO>(existing);
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi cập nhật mã giảm giá: " + ex.Message);
            }
        }

        public async Task<bool> Delete(int id)
        {
            try
            {
                var existing = await _context.MaGiamGias.FindAsync(id);
                if (existing == null)
                    return false;

                _context.MaGiamGias.Remove(existing);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi xóa mã giảm giá: " + ex.Message);
            }
        }

        public async Task<List<MaGiamGiaDTO>> SearchByKeyword(string keyword)
        {
            IQueryable<MaGiamGia> query = _context.MaGiamGias;

            if (!string.IsNullOrEmpty(keyword))
            {
                keyword = keyword.Trim().ToLower();
                query = query.Where(x =>
                    x.PromoCode.ToLower().Contains(keyword) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)));
            }

            var result = await query.ToListAsync();
            return _mapper.Map<List<MaGiamGiaDTO>>(result);
        }

        public async Task<MaGiamGiaDTO?> updateAfterCreatedOrder(int? promoId)
        {
            var maGiamGia = await _context.MaGiamGias.FirstOrDefaultAsync(x => x.PromoId == promoId);
            if (maGiamGia == null)
            {
                return null;
            }
            maGiamGia.UsedCount += 1;
            maGiamGia.UsageLimit -= 1;
            _context.MaGiamGias.Update(maGiamGia);
            await _context.SaveChangesAsync();
            return _mapper.Map<MaGiamGiaDTO>(maGiamGia);
        }
    }
}
