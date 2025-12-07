using BlazorStoreManagementWebApp.Services.Interfaces;

namespace BlazorStoreManagementWebApp.Services.Implements
{
    public class ImageService : IImageService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;
        private readonly string _imageFolder;
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private readonly long _maxFileSize = 5 * 1024 * 1024; // 5MB

        public ImageService(IWebHostEnvironment environment, IConfiguration configuration)
        {
            _environment = environment;
            _configuration = configuration;
            _imageFolder = Path.Combine(_environment.WebRootPath, "images");

            // Tạo thư mục nếu chưa tồn tại
            if (!Directory.Exists(_imageFolder))
            {
                Directory.CreateDirectory(_imageFolder);
            }
        }

        public async Task<ImageUploadResult> SaveImageAsync(IFormFile file)
        {
            try
            {
                // Validate file
                if (file == null || file.Length == 0)
                {
                    return new ImageUploadResult
                    {
                        Success = false,
                        Message = "File không hợp lệ"
                    };
                }

                // Validate file size
                if (file.Length > _maxFileSize)
                {
                    return new ImageUploadResult
                    {
                        Success = false,
                        Message = $"Kích thước file không được vượt quá {_maxFileSize / 1024 / 1024}MB"
                    };
                }

                // Validate file type
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (string.IsNullOrEmpty(extension) || !_allowedExtensions.Contains(extension))
                {
                    return new ImageUploadResult
                    {
                        Success = false,
                        Message = "Chỉ chấp nhận file ảnh (jpg, jpeg, png, gif, webp)"
                    };
                }

                // Tạo tên file unique
                var fileName = $"{Guid.NewGuid():N}{extension}";
                var filePath = Path.Combine(_imageFolder, fileName);

                // Lưu file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Tạo URL để truy cập
                var imageUrl = GetImageUrl(fileName);

                return new ImageUploadResult
                {
                    Success = true,
                    Message = "Upload ảnh thành công",
                    Data = new ImageData
                    {
                        FileName = fileName,
                        FilePath = filePath,
                        Url = imageUrl,
                        FileSize = file.Length,
                        ContentType = GetContentType(extension)
                    }
                };
            }
            catch (Exception ex)
            {
                return new ImageUploadResult
                {
                    Success = false,
                    Message = $"Lỗi khi upload ảnh: {ex.Message}"
                };
            }
        }

        public async Task<bool> DeleteImageAsync(string fileName)
        {
            try
            {
                if (string.IsNullOrEmpty(fileName))
                {
                    return false;
                }

                var filePath = Path.Combine(_imageFolder, fileName);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                // Có thể log lỗi ở đây
                Console.WriteLine($"Lỗi khi xóa ảnh: {ex.Message}");
                return false;
            }
        }

        public async Task<(byte[] data, string contentType)> GetImageAsync(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new FileNotFoundException("Tên file không hợp lệ");
            }

            var filePath = Path.Combine(_imageFolder, fileName);

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Ảnh không tồn tại");
            }

            var data = await File.ReadAllBytesAsync(filePath);
            var contentType = GetContentType(Path.GetExtension(filePath));

            return (data, contentType);
        }

        public string GetImageUrl(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return string.Empty;
            }

            var baseUrl = _configuration["BaseUrl"] ?? "https://localhost:7009";
            return $"{baseUrl}/images/{fileName}";
        }

        private string GetContentType(string extension)
        {
            return extension.ToLowerInvariant() switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                _ => "application/octet-stream"
            };
        }

        // Helper method để xóa ảnh cũ khi cập nhật
        public async Task DeleteOldImageIfExists(string? oldImageUrl)
        {
            if (!string.IsNullOrEmpty(oldImageUrl))
            {
                var oldFileName = Path.GetFileName(oldImageUrl);
                if (!string.IsNullOrEmpty(oldFileName))
                {
                    await DeleteImageAsync(oldFileName);
                }
            }
        }
    }


}
