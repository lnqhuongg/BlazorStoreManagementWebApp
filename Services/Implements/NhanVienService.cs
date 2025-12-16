using AutoMapper;
using BlazorStoreManagementWebApp.DTOs.Admin.NhanVien;
using BlazorStoreManagementWebApp.Helpers;
using BlazorStoreManagementWebApp.Models;
using BlazorStoreManagementWebApp.Models.Entities;
using BlazorStoreManagementWebApp.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BlazorStoreManagementWebApp.Services.Implements
{
    public class NhanVienService : INhanVienService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public NhanVienService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PagedResult<NhanVienDTO>> GetAll(int page, int pageSize, NhanVienFilterDTO filter)
        {
            var query = Search(filter);

            var total = await query.CountAsync();

            var list = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var dtos = _mapper.Map<List<NhanVienDTO>>(list);
            dtos.ForEach(dto => dto.Password = "");

            return new PagedResult<NhanVienDTO>
            {
                Data = dtos,
                Total = total,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<NhanVienDTO?> GetById(int userId)
        {
            var entity = await _context.NhanViens.FindAsync(userId);
            if (entity == null) return null;

            var dto = _mapper.Map<NhanVienDTO>(entity);
            dto.Password = "";
            return dto;
        }

        public IQueryable<NhanVien> Search(NhanVienFilterDTO filter)
        {
            IQueryable<NhanVien> query = _context.NhanViens;

            if (!string.IsNullOrEmpty(filter.Keyword))
            {
                query = query.Where(x =>
                    x.Username.Contains(filter.Keyword) ||
                    x.FullName.Contains(filter.Keyword) ||
                    x.Role.Contains(filter.Keyword));
            }

            if (!string.IsNullOrEmpty(filter.Role))
            {
                if (filter.Role != "admin" && filter.Role != "staff")
                {
                    throw new Exception("Role phải là 'admin' hoặc 'staff'!");
                }
                query = query.Where(x => x.Role == filter.Role);
            }

            return query;
        }

        // CREATE: Không cho phép trùng Username, và role set cứng admin/staff
        public async Task<NhanVienDTO> Create(NhanVienDTO dto)
        {
            if (dto.Role != "admin" && dto.Role != "staff")
            {
                throw new Exception("Role phải là 'admin' hoặc 'staff'!");
            }

            // Kiểm tra trùng username
            if (await isUsernameExist(dto.Username))
            {
                throw new Exception("Tên đăng nhập đã tồn tại!");
            }

            var entity = _mapper.Map<NhanVien>(dto);
            entity.CreatedAt = DateTime.Now;
            entity.Status = 1;

            // TODO: Hash password
            entity.Password = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            _context.NhanViens.Add(entity);
            await _context.SaveChangesAsync();

            var resultDto = _mapper.Map<NhanVienDTO>(entity);
            resultDto.Password = "";
            return resultDto;
        }

        // UPDATE: Không cho phép trùng Username (ngoại trừ chính nó), và role set cứng admin/staff
        public async Task<NhanVienDTO?> Update(int id, NhanVienDTO dto)
        {
            if (dto.Role != "admin" && dto.Role != "staff")
            {
                throw new Exception("Role phải là 'admin' hoặc 'staff'!");
            }

            var existing = await _context.NhanViens.FindAsync(id);
            if (existing == null)
            {
                return null; // 404
            }

            // Kiểm tra: Nếu username thay đổi → kiểm tra trùng
            if (dto.Username != existing.Username && await isUsernameExist(dto.Username))
            {
                throw new Exception("Tên đăng nhập đã được sử dụng bởi tài khoản khác!");
            }

            existing.Username = dto.Username;
            existing.FullName = dto.FullName;
            existing.Role = dto.Role;
            existing.Status = dto.Status;

            // ĐỔI MẬT KHẨU THẬT - Phần quan trọng nhất!
            if (!string.IsNullOrEmpty(dto.NewPassword))
            {
                // Hash mật khẩu mới bằng BCrypt (an toàn nhất)
                existing.Password = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword.Trim());
            }
            // Nếu NewPassword rỗng hoặc null → không đổi mật khẩu

            await _context.SaveChangesAsync();

            var resultDto = _mapper.Map<NhanVienDTO>(existing);
            resultDto.Password = "";
            resultDto.NewPassword = null;
            return resultDto;
        }

        // Kiểm tra username tồn tại
        public async Task<bool> isUsernameExist(string username)
        {
            return await _context.NhanViens.AnyAsync(x => x.Username == username);
        }

        // Kiểm tra user tồn tại theo ID
        public async Task<bool> isUserExist(int userId)
        {
            return await _context.NhanViens.AnyAsync(x => x.UserId == userId);
        }
    }


}
