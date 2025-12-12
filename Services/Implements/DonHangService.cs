using AutoMapper;
using BlazorStoreManagementWebApp.Models;
using BlazorStoreManagementWebApp.Models.Entities;
using BlazorStoreManagementWebApp.Services.Interfaces;
using BlazorStoreManagementWebApp.DTOs.Admin.DonHang;
using Microsoft.EntityFrameworkCore;
using BlazorStoreManagementWebApp.Helpers;

namespace BlazorStoreManagementWebApp.Services.Implements
{
    public class DonHangService : IDonHangService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public DonHangService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // ==================== 1. LỌC DỮ LIỆU (Tương tự SearchByKeyword) ====================
        public IQueryable<DonHang> ApplyFilter(DonHangFilterDTO filter)
        {
            // 1. Include Khách hàng và Nhân viên ngay từ đầu để tìm kiếm
            var q = _context.DonHangs
                .Include(x => x.Customer) // Để tìm theo tên khách
                .Include(x => x.User)     // Để tìm theo tên nhân viên (nếu có quan hệ)
                .AsQueryable();

            // 2. Logic tìm kiếm đa năng (Keyword)
            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                var kw = filter.Keyword.Trim().ToLower(); // Chuyển về chữ thường để tìm không phân biệt hoa thường

                // Tìm theo: Mã đơn OR Tên khách OR Tên nhân viên
                q = q.Where(x =>
                    x.OrderId.ToString().Contains(kw) ||
                    (x.Customer != null && x.Customer.Name.ToLower().Contains(kw)) ||
                    (x.User != null && x.User.FullName.ToLower().Contains(kw)) // Giả sử User có cột FullName
                );
            }

            // 3. Lọc theo ngày
            if (filter.DateFrom.HasValue)
                q = q.Where(x => x.OrderDate >= filter.DateFrom);

            if (filter.DateTo.HasValue)
            {
                var end = filter.DateTo.Value.Date.AddDays(1).AddTicks(-1);
                q = q.Where(x => x.OrderDate <= end);
            }

            // 4. Lọc theo tổng tiền
            if (filter.MinTotal.HasValue)
                q = q.Where(x => (x.TotalAmount ?? 0) >= filter.MinTotal.Value);

            if (filter.MaxTotal.HasValue)
                q = q.Where(x => (x.TotalAmount ?? 0) <= filter.MaxTotal.Value);

            return q;
        }

        // ==================== 2. LẤY DANH SÁCH (Phân trang) ====================
        public async Task<PagedResult<DonHangDTO>> GetAll(int page, int pageSize, DonHangFilterDTO filter)
        {
            // Bước 1: Gọi hàm lọc ở trên
            var query = ApplyFilter(filter);

            // Bước 2: Đếm tổng số bản ghi thỏa mãn điều kiện lọc
            var total = await query.CountAsync();

            // Bước 3: Phân trang và lấy dữ liệu
            var list = await query
                .OrderByDescending(x => x.OrderDate) // Đơn mới nhất lên đầu
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(x => x.Items)      // Quan trọng: Lấy kèm chi tiết đơn hàng
                .Include(x => x.Payments)   // Quan trọng: Lấy kèm lịch sử thanh toán
                .ToListAsync();

            // Bước 4: Map sang DTO và trả về
            return new PagedResult<DonHangDTO>
            {
                Data = _mapper.Map<List<DonHangDTO>>(list),
                Total = total,
                Page = page,
                PageSize = pageSize
            };
        }

        // ==================== 3. LẤY CHI TIẾT (GetById) ====================
        public async Task<DonHangDTO?> GetById(int orderId)
        {
            var e = await _context.DonHangs
                .Include(x => x.Promotion)
                .Include(x => x.Customer)
                .Include(x => x.User)
                .Include(x => x.Payments)
                .Include(x => x.Items)
                    .ThenInclude(d => d.Product)
                        .ThenInclude(p => p.Category)
                .FirstOrDefaultAsync(x => x.OrderId == orderId);

            if (e == null) return null;

            var dto = _mapper.Map<DonHangDTO>(e);
            dto.CustomerName = e.Customer?.Name ?? "";
            dto.UserName = e.User?.FullName ?? "";
            return dto;
        }

        // ==================== 4. TẠO MỚI (Create) ====================
        public async Task<DonHangDTO> CreateStaff(CreateDonHangDTO dto)
        {
            using var tran = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1) Map Order
                var orderEntity = new DonHang
                {
                    CustomerId = dto.CustomerId,
                    UserId = dto.UserId,
                    PromoId = dto.PromoId,
                    TotalAmount = dto.TotalAmount ?? 0,
                    DiscountAmount = dto.DiscountAmount,
                    OrderDate = DateTime.Now,
                    Status = "paid",
                    Items = dto.Items?.Select(i => new ChiTietDonHang
                    {
                        ProductId = i.ProductId,
                        Quantity = i.Quantity,
                        Price = i.Price,
                        Subtotal = i.Subtotal
                    }).ToList()
                };

                // 2) Insert Order + Items
                _context.DonHangs.Add(orderEntity);
                await _context.SaveChangesAsync();   // <-- sinh OrderId

                // 3) Insert Payments nếu có
                if (dto.Payments != null)
                {
                    foreach (var p in dto.Payments)
                    {
                        var payment = new ThanhToan
                        {
                            OrderId = orderEntity.OrderId,
                            Amount = p.Amount,
                            PaymentMethod = p.PaymentMethod,
                            PaymentDate = DateTime.Now
                        };

                        _context.ThanhToans.Add(payment);
                    }

                    await _context.SaveChangesAsync();
                }

                await tran.CommitAsync();

                // 4) Trả về DTO
                return _mapper.Map<DonHangDTO>(orderEntity);
            }
            catch (Exception ex)
            {
                await tran.RollbackAsync();
                var msg = ex.InnerException?.Message ?? ex.Message;
                throw new Exception("SQL Error: " + msg);
            }
        }

        public Task<List<DonHangDTO>> GetTodayOrders()
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            var orders = _context.DonHangs
                .Where(o => o.OrderDate >= today && o.OrderDate < tomorrow)
                .Include(o => o.Customer)
                .Include(o => o.User)
                .Include(o => o.Items)
                .AsNoTracking();
            return orders
                .Select(o => _mapper.Map<DonHangDTO>(o))
                .ToListAsync();
        }

        public long TinhTongDoanhThu(string mode, int month, int year)
        {
            if (mode == "Month")
            {
                var start = new DateTime(year, month, 1);
                var end = start.AddMonths(1);

                return (long)_context.DonHangs
                    .Where(o => o.OrderDate >= start && o.OrderDate < end)
                    .Sum(o => o.TotalAmount ?? 0);
            }
            else if (mode == "Year")
            {
                var start = new DateTime(year, 1, 1);
                var end = start.AddYears(1);

                return (long)_context.DonHangs
                    .Where(o => o.OrderDate >= start && o.OrderDate < end)
                    .Sum(o => o.TotalAmount ?? 0);
            }

            // Không làm crash UI
            return 0;
        }

        public List<long> GetRevenueByMonth(int month, int year)
        {
            int days = DateTime.DaysInMonth(year, month);
            List<long> result = new();

            for (int day = 1; day <= days; day++)
            {
                var start = new DateTime(year, month, day);
                var end = start.AddDays(1);

                long total = (long) _context.DonHangs
                    .Where(o => o.OrderDate >= start && o.OrderDate < end)
                    .Sum(o => o.TotalAmount ?? 0);

                result.Add(total);
            }

            return result;
        }

        public List<long> GetRevenueByYear(int year)
        {
            List<long> result = new();

            for (int month = 1; month <= 12; month++)
            {
                var start = new DateTime(year, month, 1);
                var end = start.AddMonths(1);

                long total = (long) _context.DonHangs
                    .Where(o => o.OrderDate >= start && o.OrderDate < end)
                    .Sum(o => o.TotalAmount ?? 0);

                result.Add(total);
            }

            return result;
        }

    }
}
