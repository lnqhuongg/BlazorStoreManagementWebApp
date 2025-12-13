using BlazorStoreManagementWebApp.DTOs.Admin.DonHang;
using BlazorStoreManagementWebApp.Helpers;
using BlazorStoreManagementWebApp.Models;
using BlazorStoreManagementWebApp.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BlazorStoreManagementWebApp.Services.Implements
{
    public class DonHangService : IDonHangService
    {
        private readonly ApplicationDbContext _context;
        // private readonly IMapper _mapper; // Tạm thời chưa dùng mapper thì có thể bỏ qua

        public DonHangService(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==================== 1. LẤY DANH SÁCH (Phân trang & Lọc) ====================
        public async Task<PagedResult<DonHangDTO>> GetAll(int page, int pageSize, DonHangFilterDTO filter)
        {
            // Join các bảng: DonHang -> KhachHang -> Promotions (KhuyenMai)
            var query = from o in _context.DonHangs
                        join c in _context.KhachHangs on o.CustomerId equals c.CustomerId into custGroup
                        from cust in custGroup.DefaultIfEmpty()

                            // LƯU Ý: Kiểm tra tên bảng trong DbContext. Thường là Promotions hoặc MaGiamGias
                        join p in _context.MaGiamGias on o.PromoId equals p.PromoId into promoGroup
                        from promo in promoGroup.DefaultIfEmpty()

                        select new
                        {
                            Order = o,
                            CustomerName = cust != null ? cust.Name : "Khách vãng lai",
                            Phone = cust != null ? cust.Phone : "",
                            PromoCode = promo != null ? promo.PromoCode : "",

                            // QUAN TRỌNG: Chuyển về chữ thường (.ToLower) để khớp với giao diện "paid", "canceled"
                            Status = o.Status.ToLower()
                        };

            // --- LỌC DỮ LIỆU ---
            if (!string.IsNullOrEmpty(filter.Keyword))
            {
                query = query.Where(x => x.Order.OrderId.ToString().Contains(filter.Keyword)
                                      || x.CustomerName.Contains(filter.Keyword));
            }

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

            // --- ĐẾM TỔNG & PHÂN TRANG ---
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
                    Status = x.Status,      // Status đã được ToLower() ở trên
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

        // ==================== 2. LẤY CHI TIẾT ====================
        public async Task<ChiTietDonHangDTO> GetOrderById(int orderId)
        {
            // 1. Lấy thông tin Header (Đơn hàng chung)
            var orderInfo = await (from o in _context.DonHangs
                                   join c in _context.KhachHangs on o.CustomerId equals c.CustomerId into custGroup
                                   from cust in custGroup.DefaultIfEmpty()

                                       // Join bảng Promotions
                                   join p in _context.MaGiamGias on o.PromoId equals p.PromoId into promoGroup
                                   from promo in promoGroup.DefaultIfEmpty()

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
                                       DiscountAmount = o.DiscountAmount,
                                       PromoCode = promo != null ? promo.PromoCode : "",

                                       // QUAN TRỌNG: Chuyển Status về chữ thường
                                       Status = o.Status.ToLower()
                                   }).FirstOrDefaultAsync();

            if (orderInfo == null) return null;

            // 2. Lấy danh sách sản phẩm (Chi tiết)
            var items = await (from d in _context.ChiTietDonHangs // Kiểm tra tên: ChiTietDonHangs hoặc OrderItems
                               join p in _context.SanPhams on d.ProductId equals p.ProductID // SỬA LỖI: ProductId (d thường)
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