using AutoMapper;
using BlazorStoreManagementWebApp.DTOs.Admin.PhieuNhap;
using BlazorStoreManagementWebApp.Helpers;
using BlazorStoreManagementWebApp.Models;
using BlazorStoreManagementWebApp.Models.Entities;
using BlazorStoreManagementWebApp.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BlazorStoreManagementWebApp.Services.Implements
{
    public class PhieuNhapService : IPhieuNhapService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public PhieuNhapService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        //get by id
        public async Task<PhieuNhapDTO> GetById(int id)
        {
            if (!isExist(id)) return null;
            var phieuNhap = await _context.PhieuNhaps
                .Include(p => p.Staff)
                .Include(p => p.Supplier)
                .Include(p => p.ImportDetails)
                    .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(p => p.ImportId == id);
            return _mapper.Map<PhieuNhapDTO>(phieuNhap);
        }

        //create
        public async Task<PhieuNhapDTO> Create(PhieuNhapDTO phieuNhapDto)
        {
            try
            {
                var phieuNhap = _mapper.Map<PhieuNhap>(phieuNhapDto);

                // Nếu có supplier id
                if (phieuNhap.Supplier != null && phieuNhap.Supplier.SupplierId > 0)
                {
                    // Lấy entity thực từ context để gắn vào
                    var existingSupplier = await _context.NhaCungCaps.FindAsync(phieuNhap.Supplier.SupplierId);
                    if (existingSupplier != null)
                    {
                        phieuNhap.Supplier = existingSupplier; // Gắn lại entity đang được track
                        phieuNhap.SupplierId = existingSupplier.SupplierId;
                    }
                }
                if (phieuNhap.Staff != null && phieuNhap.Staff.UserId > 0)
                {
                    var existingStaff = await _context.NhanViens.FindAsync(phieuNhap.Staff.UserId);
                    if (existingStaff != null)
                    {
                        phieuNhap.Staff = existingStaff;
                        phieuNhap.UserId = existingStaff.UserId;
                    }
                }

                _context.PhieuNhaps.Add(phieuNhap);

                var result = _context.SaveChanges(acceptAllChangesOnSuccess: false);
                _context.ChangeTracker.AcceptAllChanges();

                _context.ChangeTracker.DetectChanges();

                if (result > 0)
                {
                    // Nạp thêm navigation properties (nếu cần)
                    await _context.Entry(phieuNhap).Reference(p => p.Supplier).LoadAsync();
                    await _context.Entry(phieuNhap).Reference(p => p.Staff).LoadAsync();
                    await _context.Entry(phieuNhap).Collection(p => p.ImportDetails).LoadAsync();

                    return _mapper.Map<PhieuNhapDTO>(phieuNhap);
                }
                else
                {
                    Console.WriteLine("Thêm phiếu nhập thất bại: Không có bản ghi nào được thêm.");
                    return null; // thêm thất bại
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi thêm phiếu nhập: {ex.Message}");
                return null;
            }
        }


        //update
        public async Task<PhieuNhapDTO> Update(PhieuNhapDTO phieuNhapDto)
        {
            try
            {
                var phieuNhap = await _context.PhieuNhaps
                    .Include(p => p.Supplier)
                    .Include(p => p.Staff)
                    .FirstOrDefaultAsync(p => p.ImportId == phieuNhapDto.ImportId);

                if (phieuNhap == null)
                {
                    Console.WriteLine("Không tìm thấy phiếu nhập để cập nhật.");
                    return null;
                }

                // Gán lại các thuộc tính cơ bản
                phieuNhap.ImportDate = phieuNhapDto.ImportDate;
                phieuNhap.TotalAmount = phieuNhapDto.TotalAmount;

                // Gán lại SupplierId (nếu có)
                if (phieuNhapDto.Supplier != null)
                {
                    phieuNhap.SupplierId = phieuNhapDto.Supplier.SupplierId;
                }

                // Không gán lại Supplier object để tránh lỗi tracked entity
                phieuNhap.Supplier = null;

                if (phieuNhapDto.Staff != null)
                {
                    phieuNhap.UserId = phieuNhapDto.Staff.UserId;
                }
                // Không gán lại Staff object để tránh lỗi tracked entity
                phieuNhap.Staff = null;

                // Đánh dấu entity là modified
                _context.Entry(phieuNhap).State = EntityState.Modified;

                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    // Nạp lại navigation property sau khi cập nhật
                    await _context.Entry(phieuNhap).Reference(p => p.Supplier).LoadAsync();
                    await _context.Entry(phieuNhap).Reference(p => p.Staff).LoadAsync();
                    return _mapper.Map<PhieuNhapDTO>(phieuNhap);
                }
                else
                {
                    Console.WriteLine("Không có thay đổi nào được lưu.");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi cập nhật phiếu nhập: {ex.Message}");
                return null;
            }
        }


        //delete
        public bool Delete(int id)
        {
            var phieuNhapTMP = _context.PhieuNhaps.Find(id);
            if (phieuNhapTMP == null) return false;
            if (isExist(id))
            {
                try
                {
                    _context.PhieuNhaps.Remove(phieuNhapTMP);
                    _context.SaveChanges();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            else return false;
        }
        public bool isExist(int id)
        {
            return _context.PhieuNhaps.Any(e => e.ImportId == id);
        }

        // add phieu nhap bao gom chi tiet
        public async Task<PhieuNhapDTO> CreateWithDetails(CreatePhieuNhapDTO phieuNhapDto)
        {
            if (phieuNhapDto.ImportDetails == null || !phieuNhapDto.ImportDetails.Any())
                throw new Exception("Phiếu nhập phải có ít nhất 1 chi tiết!");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Lấy entity thật từ DB
                var supplier = await _context.NhaCungCaps.FindAsync(phieuNhapDto.SupplierId)
                    ?? throw new Exception($"Không tìm thấy nhà cung cấp ID = {phieuNhapDto.SupplierId}");

                var staff = await _context.NhanViens.FindAsync(phieuNhapDto.UserId)
                    ?? throw new Exception($"Không tìm thấy nhân viên ID = {phieuNhapDto.UserId}");

                // Tạo phiếu nhập
                var phieuNhap = new PhieuNhap
                {
                    ImportDate = DateTime.UtcNow,
                    Supplier = supplier,
                    Staff = staff,
                    TotalAmount = phieuNhapDto.ImportDetails.Sum(d => d.Quantity * d.Price)
                };

                _context.PhieuNhaps.Add(phieuNhap);
                await _context.SaveChangesAsync();

                // Thêm chi tiết phiếu nhập
                foreach (var detailDto in phieuNhapDto.ImportDetails)
                {
                    var product = await _context.SanPhams.FindAsync(detailDto.ProductId)
                        ?? throw new Exception($"Không tìm thấy sản phẩm ID = {detailDto.ProductId}");

                    var detail = new ChiTietPhieuNhap
                    {
                        ImportId = phieuNhap.ImportId,
                        Product = product,
                        Quantity = detailDto.Quantity,
                        Price = detailDto.Price,
                        Subtotal = detailDto.Quantity * detailDto.Price
                    };

                    _context.ChiTietPhieuNhaps.Add(detail);

                    // cập nhật tồn kho
                    var tonKho = await _context.TonKhos.FirstOrDefaultAsync(tk => tk.ProductId == product.ProductID);
                    if (tonKho != null)
                    {
                        tonKho.Quantity += detailDto.Quantity;
                        _context.Entry(tonKho).State = EntityState.Modified;
                    }
                    else
                    {
                        // nếu chưa có tồn kho thì tạo mới
                        var newTonKho = new TonKho
                        {
                            ProductId = detailDto.ProductId,
                            Quantity = detailDto.Quantity
                        };
                        _context.TonKhos.Add(newTonKho);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // 🔹 Lấy lại entity đầy đủ để trả về
                var created = await _context.PhieuNhaps
                    .Include(p => p.Supplier)
                    .Include(p => p.Staff)
                    .Include(p => p.ImportDetails)
                        .ThenInclude(d => d.Product)
                    .FirstAsync(p => p.ImportId == phieuNhap.ImportId);

                return _mapper.Map<PhieuNhapDTO>(created);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public IQueryable<PhieuNhap> FilterAndSearch(IQueryable<PhieuNhap> query, PhieuNhapFilter input)
        {
            //query = _context.PhieuNhaps.AsQueryable();

            if (input == null)
            {
                // Xử lý trường hợp input là null (ví dụ: trả về query mà không lọc)
                Console.WriteLine("InputFilter received is NULL in FilterAndSearch.");
                return query;
            }
            Console.WriteLine($"StartDate: {input.StartDate}");
            Console.WriteLine($"EndDate: {input.EndDate}");
            // 🔹 Tìm kiếm theo từ khóa
            if (!string.IsNullOrEmpty(input.Keyword))
            {
                query = query.Where(p =>
                    (p.Supplier != null && p.Supplier.Name.Contains(input.Keyword)) ||
                    (p.Staff != null && p.Staff.FullName.Contains(input.Keyword)));
            }

            // 🔹 Lọc theo giá tiền
            if (input.MinPrice.HasValue)
                query = query.Where(p => p.TotalAmount >= input.MinPrice.Value);

            if (input.MaxPrice.HasValue)
                query = query.Where(p => p.TotalAmount <= input.MaxPrice.Value);

            // 🔹 Lọc theo ngày (chỉ ngày, bỏ giờ)
            if (input.StartDate.HasValue)
            {
                var startDate = input.StartDate.Value.Date; // 2025-11-05 00:00:00
                query = query.Where(p => p.ImportDate >= startDate);
            }

            if (input.EndDate.HasValue)
            {
                // Lấy đến cuối ngày 2025-11-06
                var endDate = input.EndDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(p => p.ImportDate <= endDate);
            }

            // Include navigation
            query = query
                .Include(p => p.Staff)
                .Include(p => p.Supplier)
                .Include(p => p.ImportDetails)
                    .ThenInclude(d => d.Product);

            return query;
        }



        public async Task<PagedResult<PhieuNhapDTO>> GetAll(PhieuNhapFilter input, int pageNumber, int pageSize)
        {
            var query = FilterAndSearch(_context.PhieuNhaps, input);

            // ✅ Sắp xếp trước khi Skip/Take
            query = query.OrderBy(p => p.ImportDate);
            var total = await query.CountAsync();

            var pagedList = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PagedResult<PhieuNhapDTO>
            {
                Total = total,
                Page = pageNumber,
                PageSize = pageSize,
                Data = _mapper.Map<List<PhieuNhapDTO>>(pagedList)
            };
        }

        public long TinhTongTienNhap(string mode, int month, int year)
        {
            var query = _context.PhieuNhaps.AsQueryable();
            if (mode == "Month")
            {
                query = query.Where(p => p.ImportDate.Month == month && p.ImportDate.Year == year);
            }
            else if (mode == "Year")
            {
                query = query.Where(p => p.ImportDate.Year == year);
            }
            long totalTienNhap = query.Sum(p => (long?)p.TotalAmount) ?? 0;
            return totalTienNhap;
        }

        public List<long> GetCapitalByMonth(int month, int year)
        {
            int days = DateTime.DaysInMonth(year, month);
            List<long> result = new();

            for (int day = 1; day <= days; day++)
            {
                var start = new DateTime(year, month, day);
                var end = start.AddDays(1);

                long total = (long) _context.PhieuNhaps
                    .Where(i => i.ImportDate >= start && i.ImportDate < end)
                    .Sum(i => i.TotalAmount);

                result.Add(total);
            }

            return result;
        }

        public List<long> GetCapitalByYear(int year)
        {
            List<long> result = new();

            for (int month = 1; month <= 12; month++)
            {
                var start = new DateTime(year, month, 1);
                var end = start.AddMonths(1);

                long total = (long) _context.PhieuNhaps
                    .Where(o => o.ImportDate >= start && o.ImportDate < end)
                    .Sum(o => o.TotalAmount);

                result.Add(total);
            }

            return result;
        }

    }


}
