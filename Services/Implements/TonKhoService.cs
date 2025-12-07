using AutoMapper;
using BlazorStoreManagementWebApp.DTOs.Admin.TonKho;
using BlazorStoreManagementWebApp.Models;
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
    }


}
