namespace Application.Interfaces
{
    public interface ICloudinaryService
    {
        /// <summary>
        /// Upload image stream to Cloudinary and return the secure URL.
        /// </summary>
        Task<string> UploadImageAsync(Stream fileStream, string fileName, CancellationToken ct = default);

        /// <summary>
        /// Delete image from Cloudinary by its public ID.
        /// </summary>
        Task DeleteImageAsync(string publicId, CancellationToken ct = default);
    }
}
