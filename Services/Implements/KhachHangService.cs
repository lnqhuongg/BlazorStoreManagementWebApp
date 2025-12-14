using AutoMapper;
using BlazorStoreManagementWebApp.DTOs.Admin.KhachHang;
using BlazorStoreManagementWebApp.DTOs.Authentication;
using BlazorStoreManagementWebApp.Helpers;
using BlazorStoreManagementWebApp.Models;
using BlazorStoreManagementWebApp.Models.Entities;
using BlazorStoreManagementWebApp.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BlazorStoreManagementWebApp.Services.Implements
{
    public class KhachHangService : IKhachHangService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public KhachHangService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // lấy tất cả khách hàng + phân trang + tìm kiếm theo tên, sđt, email
        public async Task<PagedResult<KhachHangDTO>> GetAll(int page, int pageSize, string keyword)
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

            return new PagedResult<KhachHangDTO>
            {
                Data = _mapper.Map<List<KhachHangDTO>>(list),
                Total = total,
                Page = page,
                PageSize = pageSize
            };
        }

        public IQueryable<KhachHang> SearchByKeyword(string keyword)
        {
            IQueryable<KhachHang> query = _context.KhachHangs;
            if (!string.IsNullOrEmpty(keyword))
            {
                keyword = keyword.Trim().ToLower();
                query = query.Where(x =>
                    (x.Name != null && x.Name.ToLower().Contains(keyword)) ||
                    (x.Phone != null && x.Phone.Contains(keyword)) ||
                    (x.Email != null && x.Email.ToLower().Contains(keyword))
                );
            }
            return query;
        }

        // lấy ra 1 khách hàng theo id 
        public async Task<KhachHangDTO> GetById(int id)
        {
            var kh = await _context.KhachHangs.FindAsync(id);
            if (kh == null) throw new KeyNotFoundException($"Khách hàng với id {id} không tồn tại.");
            return _mapper.Map<KhachHangDTO>(kh);
        }

        // tạo mới (đã kiểm tra trùng email và số điện thoại)
        public async Task<KhachHangDTO> Create(DangKyDTO dto)
        {
            try
            {
                // Kiểm tra nhập số điện thoại (không được để trống)
                //if (dto.Phone == null || string.IsNullOrWhiteSpace(dto.Phone))
                //{
                //    throw new InvalidOperationException("Số điện thoại không được để trống!");
                //}

                //// Kiểm tra trùng số điện thoại
                //if (await IsPhoneExist(dto.Phone))
                //{
                //    throw new InvalidOperationException("Số điện thoại đã được sử dụng!");
                //}

                //// Kiểm tra trùng email (nếu có nhập)
                //if (!string.IsNullOrEmpty(dto.Email) && await IsEmailExist(dto.Email))
                //{
                //    throw new InvalidOperationException("Email đã được sử dụng!");
                //}

                var entity = _mapper.Map<KhachHang>(dto);
                entity.Password = BCrypt.Net.BCrypt.HashPassword(dto.Password);

                entity.CreatedAt = DateTime.Now;

                _context.KhachHangs.Add(entity);
                await _context.SaveChangesAsync();

                return _mapper.Map<KhachHangDTO>(entity);
            }
            catch (Exception ex)
            {
                if (ex is InvalidOperationException)
                    throw;
                else
                    throw new Exception("Lỗi khi thêm khách hàng: " + ex.Message);
            }
        }

        // cập nhật (CHỈ CHO SỬA TÊN VÀ ĐỊA CHỈ - KHÔNG CHO SỬA SĐT, EMAIL, ĐIỂM)
        public async Task<KhachHangDTO?> Update(int id, KhachHangDTO dto)
        {
            try
            {
                var existing = await _context.KhachHangs.FindAsync(id);
                if (existing == null)
                {
                    return null; // ← 404
                }

                // CHỈ CHO PHÉP SỬA TÊN VÀ ĐỊA CHỈ
                existing.Name = dto.Name;
                existing.Address = dto.Address;

                // KHÔNG CHO SỬA SỐ ĐIỆN THOẠI, EMAIL, ĐIỂM TÍCH LŨY
                // existing.Phone = dto.Phone;
                // existing.Email = dto.Email;
                // existing.RewardPoints = dto.RewardPoints;

                await _context.SaveChangesAsync();

                return _mapper.Map<KhachHangDTO>(existing);
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi cập nhật khách hàng: " + ex.Message);
            }
        }

        // kiểm tra phone đã tồn tại chưa (chỉ dùng khi tạo mới)
        public async Task<bool> IsPhoneExist(string phone)
        {
            return await _context.KhachHangs.AnyAsync(x => x.Phone == phone);
        }

        // kiểm tra email đã tồn tại chưa (chỉ dùng khi tạo mới)
        public async Task<bool> IsEmailExist(string email)
        {
            return await _context.KhachHangs.AnyAsync(x => x.Email == email);
        }

        // kiểm tra khách hàng có tồn tại không
        public async Task<bool> IsCustomerExist(int customerId)
        {
            return await _context.KhachHangs.AnyAsync(x => x.CustomerId == customerId);
        }

        public async Task<KhachHangDTO?> addRewardPoints(int? customerId)
        {
            var customer = await _context.KhachHangs.FindAsync(customerId);
            if (customer == null)
            {
                return null; // ← 404
            }
            // Cập nhật điểm tích lũy (ví dụ: mỗi đơn hàng thành công được cộng 500 điểm)
            customer.RewardPoints += 500;
            await _context.SaveChangesAsync();
            return _mapper.Map<KhachHangDTO>(customer);
        }

        public async Task<KhachHangDTO?> deductRewardPoints(int? customerId, int? pointsToDeduct)
        {
            var customer = await _context.KhachHangs.FindAsync(customerId);
            if (customer == null)
            {
                return null; // ← 404
            }
            // Trừ điểm tích lũy
            customer.RewardPoints -= pointsToDeduct;
            if (customer.RewardPoints < 0)
            {
                customer.RewardPoints = 0; // Đảm bảo điểm không âm
            }
            await _context.SaveChangesAsync();
            return _mapper.Map<KhachHangDTO>(customer);
        }
    }
}
