using AutoMapper;
using BlazorStoreManagementWebApp.DTOs.Admin.TonKho;
using BlazorStoreManagementWebApp.Models;
using BlazorStoreManagementWebApp.Models.Entities;
using BlazorStoreManagementWebApp.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BlazorStoreManagementWebApp.Services.Implements
{
    public class TonKhoService : ITonKhoService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        public TonKhoService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<List<TonKhoDTO>> GetAll()
        {
            var list = await _context.TonKhos.ToListAsync();
            return _mapper.Map<List<TonKhoDTO>>(list);
        }
        public async Task<TonKhoDTO> GetByProductID(int productID)
        {
            try
            {
                var tonkho = await _context.TonKhos.FirstOrDefaultAsync(x => x.ProductId == productID);
                return _mapper.Map<TonKhoDTO>(tonkho);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<TonKhoDTO> UpdateInventory(int productID, int quantityChange)
        {
            var tonkho = await _context.TonKhos.FirstOrDefaultAsync(x => x.ProductId == productID);
            if (tonkho == null)
            {
                throw new Exception("Không tìm thấy tồn kho cho sản phẩm với ID: " + productID);
            }
            tonkho.Quantity += quantityChange;
            tonkho.UpdatedAt = DateTime.UtcNow;
            _context.TonKhos.Update(tonkho);
            await _context.SaveChangesAsync();
            return _mapper.Map<TonKhoDTO>(tonkho);
        }

        public async Task<TonKhoDTO> deductQuantityOfCreatedOrder(int productID, int quantityChange)
        {
            var tonkho = await _context.TonKhos.FirstOrDefaultAsync(x => x.ProductId == productID);
            if (tonkho == null)
            {
                return null;
            }
            tonkho.Quantity -= quantityChange;
            tonkho.UpdatedAt = DateTime.UtcNow;
            _context.TonKhos.Update(tonkho);
            await _context.SaveChangesAsync();
            return _mapper.Map<TonKhoDTO>(tonkho);
        }

    public async Task<TonKhoDTO> InitializeStock(int productID, int initialQuantity = 0)
        {
            try
            {
                // 1. Kiểm tra xem tồn kho cho sản phẩm này đã tồn tại chưa (đề phòng trùng lặp)
                var existingStock = await _context.TonKhos
                    .FirstOrDefaultAsync(t => t.ProductId == productID);

                if (existingStock != null)
                {
                    throw new Exception($"Sản phẩm ID {productID} đã có dữ liệu tồn kho.");
                }

                // 2. Tạo đối tượng Entity TonKho mới
                // Lưu ý: Kiểm tra tên bảng/Entity của bạn là TonKho hay Stock
                var newStock = new TonKho
                {
                    ProductId = productID,
                    Quantity = initialQuantity,
                    UpdatedAt = DateTime.Now
                };

                // 3. Lưu vào Database
                _context.TonKhos.Add(newStock);
                await _context.SaveChangesAsync();

                // 4. Map sang DTO để trả về
                return new TonKhoDTO
                {
                    ProductId = newStock.ProductId,
                    Quantity = newStock.Quantity,
                    // Map thêm các trường khác nếu có
                };
            }
            catch (Exception ex)
            {
                // Log lỗi nếu cần
                throw new Exception("Lỗi khi tạo tồn kho: " + ex.Message);
            }
        }
    }
}
