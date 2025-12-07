namespace BlazorStoreManagementWebApp.Services.Interfaces
{
    public interface IImageService
    {
        Task<ImageUploadResult> SaveImageAsync(IFormFile file);
        Task<bool> DeleteImageAsync(string fileName);
        string GetImageUrl(string fileName);
        Task<(byte[] data, string contentType)> GetImageAsync(string fileName);
    }
    public class ImageUploadResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public ImageData? Data { get; set; }
    }

    public class ImageData
    {
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string ContentType { get; set; } = string.Empty;
    }

}
