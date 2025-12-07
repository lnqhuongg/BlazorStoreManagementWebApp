using AutoMapper;
using BlazorStoreManagementWebApp.Models;
using BlazorStoreManagementWebApp.Models.Entities;
using BlazorStoreManagementWebApp.Services.Interfaces;
using BlazorStoreManagementWebApp.DTOs.Admin.ChiTietPhieuNhap;
using Microsoft.EntityFrameworkCore;

namespace BlazorStoreManagementWebApp.Services.Implements
{
    public class ChiTietPNService : IChiTietPNService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public ChiTietPNService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // get all
        public async Task<List<ChiTietPhieuNhapDTO>> GetAll()
        {
            var list = await _context.ChiTietPhieuNhaps
                .Include(ct => ct.Product).ToListAsync();
            return _mapper.Map<List<ChiTietPhieuNhapDTO>>(list);
        }

        // get by id
        public async Task<ChiTietPhieuNhapDTO> GetById(int id)
        {
            var chiTietPhieuNhap = await _context.ChiTietPhieuNhaps
                .Include(ct => ct.Product)
                .FirstOrDefaultAsync(ct => ct.ImportDetailId == id);
            if (chiTietPhieuNhap == null) return null;
            return _mapper.Map<ChiTietPhieuNhapDTO>(chiTietPhieuNhap);
        }


        // create
        public async Task<ChiTietPhieuNhapDTO> Create(ChiTietPhieuNhapDTO chiTietPhieuNhapDto)
        {
            try
            {
                var chiTietPhieuNhap = _mapper.Map<ChiTietPhieuNhap>(chiTietPhieuNhapDto);
                if (chiTietPhieuNhap.Product != null && chiTietPhieuNhap.Product.ProductID > 0)
                {
                    var existingProduct = await _context.SanPhams.FindAsync(chiTietPhieuNhap.Product.ProductID);
                    if (existingProduct != null)
                    {
                        chiTietPhieuNhap.Product = existingProduct;
                        chiTietPhieuNhap.ProductId = existingProduct.ProductID;
                    }
                }

                _context.ChiTietPhieuNhaps.Add(chiTietPhieuNhap);
                await _context.SaveChangesAsync();
                return _mapper.Map<ChiTietPhieuNhapDTO>(chiTietPhieuNhap);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error creating import detail: " + ex.Message);
                throw new Exception("Error creating import detail: " + ex.Message);
            }
        }

        // update
        public async Task<ChiTietPhieuNhapDTO> Update(int id, ChiTietPhieuNhapDTO chiTietPhieuNhapDto)
        {
            try
            {
                var existingDetail = await _context.ChiTietPhieuNhaps
                    .Include(ct => ct.Product)
                    .FirstOrDefaultAsync(ct => ct.ImportDetailId == id);

                if (existingDetail == null)
                {
                    Console.WriteLine("Không tìm thấy phiếu nhập để cập nhật.");
                    return null;
                }

                existingDetail.ImportId = chiTietPhieuNhapDto.ImportId;
                existingDetail.Quantity = chiTietPhieuNhapDto.Quantity;
                existingDetail.Price = chiTietPhieuNhapDto.Price;
                existingDetail.Subtotal = chiTietPhieuNhapDto.Subtotal;


                if (chiTietPhieuNhapDto.Product != null)
                {
                    existingDetail.ProductId = chiTietPhieuNhapDto.Product.ProductID;
                }
                existingDetail.Product = null; // Đặt về null để tránh xung đột khi cập nhật

                // đánh dấu entity là đã sửa đổi
                _context.Entry(existingDetail).State = EntityState.Modified;

                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    await _context.Entry(existingDetail).Reference(pn => pn.Product).LoadAsync();
                    Console.WriteLine("Cập nhật chi tiết phiếu nhập thành công.");
                    return _mapper.Map<ChiTietPhieuNhapDTO>(existingDetail);
                }
                else
                {
                    Console.WriteLine("Cập nhật chi tiết phiếu nhập thất bại: Không có bản ghi nào được cập nhật.");
                    return null;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error updating import detail: " + ex.Message);
                throw new Exception("Error updating import detail: " + ex.Message);
            }
        }
        public Task<bool> Delete(int id)
        {
            throw new NotImplementedException();
        }

    }

}
