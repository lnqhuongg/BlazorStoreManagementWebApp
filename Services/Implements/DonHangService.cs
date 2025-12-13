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

        public DonHangService(ApplicationDbContext context)
        {
            _context = context;
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
            if (filter.StartDate.HasValue)
                q = q.Where(x => x.OrderDate >= filter.StartDate);

            if (filter.EndDate.HasValue)
            {
                var end = filter.EndDate.Value.Date.AddDays(1).AddTicks(-1);
                q = q.Where(x => x.OrderDate <= end);
            }

            // 4. Lọc theo tổng tiền
            if (filter.MinPrice.HasValue)
                q = q.Where(x => (x.TotalAmount ?? 0) >= filter.MinPrice.Value);

            if (filter.MaxPrice.HasValue)
                q = q.Where(x => (x.TotalAmount ?? 0) <= filter.MaxPrice.Value);

            return q;
        }

        // ==================== 2. LẤY DANH SÁCH (Phân trang) ====================
        public async Task<PagedResult<DonHangDTO>> GetAll(int page, int pageSize, DonHangFilterDTO filter)
        {
            var query = from o in _context.DonHangs
                        join c in _context.KhachHangs on o.CustomerId equals c.CustomerId into custGroup
                        from cust in custGroup.DefaultIfEmpty()
                        join p in _context.MaGiamGias on o.PromoId equals p.PromoId into promoGroup
                        from promo in promoGroup.DefaultIfEmpty()
                        select new
                        {
                            Order = o,
                            CustomerName = cust != null ? cust.Name : "Khách vãng lai",
                            Phone = cust != null ? cust.Phone : "",
                            PromoCode = promo != null ? promo.PromoCode : "",
                            Status = o.Status
                        };

            // Lọc dữ liệu
            if (!string.IsNullOrEmpty(filter.Keyword))
            {
                query = query.Where(x => x.Order.OrderId.ToString().Contains(filter.Keyword)
                                      || x.CustomerName.Contains(filter.Keyword));
            }
            // Sửa DateFrom -> StartDate (Theo DTO mới của bạn)
            if (filter.StartDate.HasValue)
            {
                query = query.Where(x => x.Order.OrderDate >= filter.StartDate.Value);
            }
            if (filter.EndDate.HasValue)
            {
                var toDate = filter.EndDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(x => x.Order.OrderDate <= toDate);
            }
            if (filter.MinPrice.HasValue)
            {
                query = query.Where(x => x.Order.TotalAmount >= filter.MinPrice.Value);
            }
            if (filter.MaxPrice.HasValue)
            {
                query = query.Where(x => x.Order.TotalAmount <= filter.MaxPrice.Value);
            }

            var total = await query.CountAsync();

            var data = await query
                .OrderByDescending(x => x.Order.OrderDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new DonHangDTO
                {
                    OrderId = x.Order.OrderId,
                    CustomerName = x.CustomerName,
                    Phone = x.Phone,
                    OrderDate = x.Order.OrderDate,
                    TotalAmount = x.Order.TotalAmount,
                    DiscountAmount = x.Order.DiscountAmount,
                    Status = x.Status,
                    PromoCode = x.PromoCode
                })
                .ToListAsync();

            return new PagedResult<DonHangDTO>
            {
                Data = data,
                Total = total,
                Page = page,
                PageSize = pageSize
            };
        }

        // --- 2. LẤY CHI TIẾT (Bắt buộc phải có để sửa lỗi CS0535) ---
        // --- Copy đè đoạn này vào hàm GetOrderById ---
        public async Task<ChiTietDonHangDTO> GetOrderById(int orderId)
        {
            // 1. Lấy thông tin chung (Header)
            var orderInfo = await (from o in _context.DonHangs
                                   join c in _context.KhachHangs on o.CustomerId equals c.CustomerId into custGroup
                                   from cust in custGroup.DefaultIfEmpty()
                                   where o.OrderId == orderId
                                   select new ChiTietDonHangDTO
                                   {
                                       OrderId = o.OrderId,
                                       CustomerName = cust != null ? cust.Name : "Khách vãng lai",
                                       Phone = cust != null ? cust.Phone : "",
                                       Email = cust != null ? cust.Email : "",
                                       Address = cust != null ? cust.Address : "",
                                       OrderDate = o.OrderDate,
                                       TotalAmount = o.TotalAmount,
                                       Status = o.Status
                                   }).FirstOrDefaultAsync();

            if (orderInfo == null) return null;

            // 2. Lấy danh sách sản phẩm (Detail)
            // SỬA: Dùng _context.ChiTietDonHangs thay vì OrderItems
            var items = await (from d in _context.ChiTietDonHangs
                               join p in _context.SanPhams on d.ProductId equals p.ProductID
                               where d.OrderId == orderId
                               select new DonHangItemDTO
                               {
                                   ProductId = d.ProductId ?? 0,
                                   ProductName = p.ProductName,
                                   ImageUrl = p.ImageUrl,
                                   Quantity = d.Quantity,
                                   Price = d.Price,
                                   Subtotal = d.Subtotal
                               }).ToListAsync();

            orderInfo.DanhSachSanPham = items;
            return orderInfo;
        }
    }
}
