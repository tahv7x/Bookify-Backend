using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using System.Net.NetworkInformation;
using System.Security.Cryptography.X509Certificates;
using static System.Net.Mime.MediaTypeNames;
namespace Bookify_API.Services
{
    public class CloudinaryService
    {
        private readonly Cloudinary cloudinary;

        public CloudinaryService(IConfiguration configuration)
        {
            var section = configuration.GetSection("Cloudinary");
            var account = new Account(
                section["CloudName"],
                section["ApiKey"],
                section["ApiSecret"]
            );
            cloudinary = new Cloudinary(account);
        }
        public async Task<String> UploadImageAsync(IFormFile file)
        {
            var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
            if(!allowedTypes.Contains(file.ContentType))
                throw new InvalidOperationException("Format non support. JPG,PNG et WEBP sont autorisés.");
            if(file.Length > 5*1024*1024)
                throw new InvalidOperationException("Fichier trop grand. Maximum 5MB.");
            using var stream = file.OpenReadStream();

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = "bookify/avatars",
                Transformation = new Transformation()
                    .Width(300).Height(300).Crop("fill").Gravity("face").Quality("auto")
            };

            var result = await cloudinary.UploadAsync(uploadParams);

            if (result.Error != null)
                throw new InvalidOperationException(result.Error.Message);
            return result.SecureUrl.ToString();
        }
        public async Task DeleteImageAsync(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl)) return; 
            var uri = new Uri(imageUrl);
            var segments = uri.AbsolutePath.Split('/');
            var uploadIndex = Array.IndexOf(segments, "upload");
            if (uploadIndex < 0) return;

            var publicIdParts = segments.Skip(uploadIndex + 2).ToArray();

            var publicId = string.Join("/", publicIdParts);

            publicId = publicId[..publicId.LastIndexOf('.')];

            var deleteParams = new DeletionParams(publicId);
            await cloudinary.DestroyAsync(deleteParams);
        }
    }
}
