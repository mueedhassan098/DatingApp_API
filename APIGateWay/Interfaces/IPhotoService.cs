using CloudinaryDotNet.Actions;

namespace APIGateWay.Interfaces
{
    public interface IPhotoService
    {
        Task<ImageUploadResult> AddPhotoAsync(IFormFile file);
        Task<DeletionResult> Update_OR_Delete_PhotoAsync(string publicId);
       // Task<DeletionResult> DeletePhotoAsync(string publicId);
    }
}
