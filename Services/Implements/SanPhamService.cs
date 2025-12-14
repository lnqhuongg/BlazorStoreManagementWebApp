using AutoMapper;
using BlazorStoreManagementWebApp.DTOs.Admin.SanPham;
using BlazorStoreManagementWebApp.Helpers;
using BlazorStoreManagementWebApp.Models;
using BlazorStoreManagementWebApp.Models.Entities;
using BlazorStoreManagementWebApp.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BlazorStoreManagementWebApp.Services.Implements
{
    public class SanPhamService : ISanPhamService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IImageService _imageService;
        private static readonly char[] Base62Chars =
        "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz".ToCharArray();
        public SanPhamService(ApplicationDbContext context, IMapper mapper, IImageService imageService)
        {
            _context = context;
            _mapper = mapper;
            _imageService = imageService;
        }

        public async Task<List<SanPhamDTO>> getListProducts()
        {
            var list = await _context.SanPhams.Include(sp => sp.Category).Include(sp => sp.Supplier).ToArrayAsync();
            return _mapper.Map<List<SanPhamDTO>>(list);
        }

        public async Task<PagedResult<SanPhamDTO>> GetAll(int page, int pageSize, string? keyword, string? order, int? categoryID, int? supplierID)
        {
            try
            {
                //var list = await _context.SanPhams.Include(x => x.Category).Include(x => x.Supplier).ToListAsync();
                //return _mapper.Map<List<SanPhamDTO>>(list);i
                var query = _context.SanPhams.Include(sp => sp.Category).Include(sp => sp.Supplier).AsQueryable();
                if (!string.IsNullOrEmpty(keyword))
                {
                    query = query.Where(x => x.ProductName.ToLower().Contains(keyword.ToLower()) || x.Barcode.ToLower().Contains(keyword.ToLower()));
                }

                if (categoryID.HasValue)
                {
                    query = query.Where(x => x.CategoryID == categoryID);
                }
                if (supplierID.HasValue)
                {
                    query = query.Where(x => x.SupplierID == supplierID);
                }
                if (!string.IsNullOrEmpty(order))
                {
                    query = order.ToLower() == "desc"
                        ? query.OrderByDescending(p => p.Price)
                        : query.OrderBy(p => p.Price);
                }
                int totalPage = await query.CountAsync();
                var list = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
                return new PagedResult<SanPhamDTO>
                {
                    Data = _mapper.Map<List<SanPhamDTO>>(list),
                    Total = totalPage,
                    Page = page,
                    PageSize = pageSize
                };
            }
            catch (Exception e)
            {
                throw new Exception("Lỗi khi lấy danh sách sản phẩm: " + e.Message);
            }
        }

        public async Task<List<SanPhamDTO>> searchByCategoryAndSortOrderAndKeyword(int? categoryID, string? sortOrder, string? keyword)
        {
            try
            {
                var query = _context.SanPhams.Include(sp => sp.Category).Include(sp => sp.Supplier).AsQueryable();
                if (categoryID.HasValue)
                {
                    query = query.Where(x => x.CategoryID == categoryID);
                }
                if (!string.IsNullOrEmpty(sortOrder))
                {
                    query = sortOrder.ToLower() == "desc"
                        ? query.OrderByDescending(p => p.Price)
                        : query.OrderBy(p => p.Price);
                }
                if (!string.IsNullOrEmpty(keyword))
                {
                    query = query.Where(x => x.ProductName.ToLower().Contains(keyword.ToLower()) || x.Barcode.ToLower().Contains(keyword.ToLower()));
                }
                var list = await query.ToListAsync();
                return _mapper.Map<List<SanPhamDTO>>(list);
            }
            catch (Exception e)
            {
                throw new Exception("Lỗi khi lấy danh sách sản phẩm: " + e.Message);
            }
        }
        public async Task<SanPhamDTO> GetById(int id)
        {
            try
            {
                var sp = await _context.SanPhams.Include(x => x.Category).Include(x => x.Supplier).FirstOrDefaultAsync(x => x.ProductID == id);
                return _mapper.Map<SanPhamDTO>(sp);
            }
            catch (Exception e)
            {
                throw new Exception("Lỗi khi lấy sản phẩm theo ID: " + e.Message);
            }
        }

        public async Task<bool> checkExistBarcode(string barcode)
        {
            bool barcodeExists = await _context.SanPhams
                .AnyAsync(x => x.Barcode == barcode);

            return barcodeExists;
        }

        public async Task<bool> checkExistID(int ID)
        {
            bool exists = await _context.SanPhams
                .AnyAsync(x => x.ProductID == ID);

            return exists;
        }

        public async Task<bool> checkBarcodeExistForOtherProducts(int id, string barcode)
        {
            var sanpham = await GetById(id);
            var exist = await _context.SanPhams.AnyAsync(x => x.Barcode == sanpham.Barcode && x.ProductID != sanpham.ProductID);
            return exist;
        }

        private static string ToBase62(ulong number)
        {
            var result = new char[8];
            int index = 7;

            do
            {
                result[index--] = Base62Chars[number % 62];
                number /= 62;
            } while (number > 0 && index >= 0);

            return new string(result, index + 1, 7 - index);
        }

        private string generateAutoBarcode()
        {
            var guid = Guid.NewGuid();
            var bytes = guid.ToByteArray();

            // Lấy 8 byte đầu từ GUID
            var number = BitConverter.ToUInt64(bytes, 0);

            return "SP" + ToBase62(number);
        }

        public async Task<SanPhamDTO> Create(SanPhamRequestDTO sp)
        {
            try
            {

                string imageUrl = null;

                // Xử lý upload ảnh nếu có
                if (sp.ImageUrl != null && sp.ImageUrl.Size > 0)
                {
                    // GIẢ ĐỊNH: IImageService.SaveImageAsync đã được sửa để nhận IBrowserFile
                    var uploadResult = await _imageService.SaveImageAsync(sp.ImageUrl);
                    if (uploadResult.Success && uploadResult.Data != null)
                    {
                        imageUrl = uploadResult.Data.Url;
                    }
                }

                string bc = generateAutoBarcode();
                while (await checkExistBarcode(bc) == true)
                {
                    bc = generateAutoBarcode();
                }

                // Tạo entity sản phẩm mới
                SanPham sanpham = new SanPham
                {
                    ProductName = sp.ProductName,
                    Barcode = bc,
                    Price = sp.Price,
                    Unit = sp.Unit,
                    CreatedAt = DateTime.Now,
                    Status = sp.Status,
                    ImageUrl = imageUrl,
                    CategoryID = sp.CategoryID,
                    SupplierID = sp.SupplierID,
                };


                _context.SanPhams.Add(sanpham);
                await _context.SaveChangesAsync();

                // Load lại sản phẩm với đầy đủ thông tin liên quan
                var createdProduct = await _context.SanPhams
                    .Include(x => x.Category)
                    .Include(x => x.Supplier)
                    .FirstOrDefaultAsync(x => x.ProductID == sanpham.ProductID);

                var resultDTO = _mapper.Map<SanPhamDTO>(createdProduct);

                return resultDTO;
            }
            catch (Exception e)
            {
                throw new Exception("Lỗi khi thêm sản phẩm: " + e.Message);
            }
        }

        public async Task<bool> Delete(int id)
        {
            try
            {
                var sanpham = await _context.SanPhams
                    .FirstOrDefaultAsync(x => x.ProductID == id);


                if (sanpham != null)
                {
                    _context.SanPhams.Remove(sanpham);

                    // SỬA: Dùng async
                    await _context.SaveChangesAsync();

                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                throw new Exception("Lỗi khi xóa sản phẩm: " + e.Message);
            }
        }
        public async Task<SanPhamDTO> Update(int id, SanPhamRequestDTO sp)
        {
            try
            {
                // SỬA: Thêm await
                var existingProduct = await _context.SanPhams
                    .FirstOrDefaultAsync(x => x.ProductID == id);

                if (existingProduct == null)
                    return null;



                // QUAN TRỌNG: Update từng property thay vì dùng AutoMapper
                existingProduct.ProductName = sp.ProductName;
                existingProduct.Barcode = sp.Barcode;
                existingProduct.Price = sp.Price;
                existingProduct.Unit = sp.Unit;
                existingProduct.Status = sp.Status;

                // CHỈ update ID, không update navigation objects
                existingProduct.CategoryID = sp.CategoryID;
                existingProduct.SupplierID = sp.SupplierID;
                if (sp.ImageUrl != null && sp.ImageUrl.Size > 0)
                {
                    // 1. Xóa ảnh cũ nếu có
                    if (!string.IsNullOrEmpty(existingProduct.ImageUrl))
                    {
                        var oldFileName = Path.GetFileName(existingProduct.ImageUrl);
                        await _imageService.DeleteImageAsync(oldFileName);
                    }

                    // 2. Upload ảnh mới và lấy URL
                    var uploadResult = await _imageService.SaveImageAsync(sp.ImageUrl);
                    if (uploadResult.Success && uploadResult.Data != null)
                    {
                        existingProduct.ImageUrl = uploadResult.Data.Url; // Lưu URL vào database
                        Console.WriteLine($"✅ Đã upload ảnh mới: {uploadResult.Data.Url}");
                    }
                    else
                    {
                        Console.WriteLine($"❌ Lỗi upload ảnh: {uploadResult.Message}");
                        // Có thể giữ nguyên ảnh cũ hoặc xử lý lỗi
                    }
                }

                // KHÔNG cần gọi Update() vì Entity Framework đang track entity
                await _context.SaveChangesAsync();

                // Load lại với đầy đủ thông tin để trả về
                var updatedProduct = await _context.SanPhams
                    .Include(x => x.Category)
                    .Include(x => x.Supplier)
                    .FirstOrDefaultAsync(x => x.ProductID == id);

                var resultDTO = _mapper.Map<SanPhamDTO>(updatedProduct);

                return resultDTO;
            }
            catch (Exception e)
            {
                throw new Exception("Lỗi khi cập nhật sản phẩm: " + e.Message);
            }
        }

        public async Task<List<SanPhamDTO>> getBySupplierID(int? supplier_id)
        {
            try
            {
                var list = await _context.SanPhams.Include(sp => sp.Category)
                .Include(sp => sp.Supplier).Where(x => x.SupplierID == supplier_id).ToListAsync();
                return _mapper.Map<List<SanPhamDTO>>(list);
            }
            catch (Exception e)
            {
                throw new Exception("Lỗi khi tìm sản phẩm theo nhà cung cấp: " + e.Message);
            }
        }

        public async Task<List<SanPhamDTO>> GetAllSP()
        {
            try
            {
                var list = await _context.SanPhams.Include(x => x.Category).Include(x => x.Supplier).ToListAsync();
                return _mapper.Map<List<SanPhamDTO>>(list);
            }
            catch (Exception e)
            {
                throw new Exception("Lỗi khi lấy danh sách sản phẩm: " + e.Message);
            }
        }

        public async Task<PagedResult<SanPhamDTO>> GetAll2(int page, int pageSize, string? keyword, string? order, int? categoryID, int? supplierID, decimal? minPrice, decimal? maxPrice)
        {
            try
            {
                //var list = await _context.SanPhams.Include(x => x.Category).Include(x => x.Supplier).ToListAsync();
                //return _mapper.Map<List<SanPhamDTO>>(list);i
                var query = _context.SanPhams.Include(sp => sp.Category).Include(sp => sp.Supplier).AsQueryable();
                if (!string.IsNullOrEmpty(keyword))
                {
                    query = query.Where(x => x.ProductName.ToLower().Contains(keyword.ToLower()) || x.Barcode.ToLower().Contains(keyword.ToLower()));
                }

                if (categoryID.HasValue)
                {
                    query = query.Where(x => x.CategoryID == categoryID);
                }
                if (supplierID.HasValue)
                {
                    query = query.Where(x => x.SupplierID == supplierID);
                }
                if (minPrice.HasValue)
                {
                    query = query.Where(x => x.Price >= minPrice.Value);
                }

                if (maxPrice.HasValue)
                {
                    query = query.Where(x => x.Price <= maxPrice.Value);
                }
                if (!string.IsNullOrEmpty(order))
                {
                    query = order.ToLower() == "desc"
                        ? query.OrderByDescending(p => p.Price)
                        : query.OrderBy(p => p.Price);
                }
                int totalPage = await query.CountAsync();
                var list = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
                return new PagedResult<SanPhamDTO>
                {
                    Data = _mapper.Map<List<SanPhamDTO>>(list),
                    Total = totalPage,
                    Page = page,
                    PageSize = pageSize
                };
            }
            catch (Exception e)
            {
                throw new Exception("Lỗi khi lấy danh sách sản phẩm: " + e.Message);
            }
        }

    }
}

