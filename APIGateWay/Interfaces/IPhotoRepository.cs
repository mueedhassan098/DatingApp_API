using APIGateWay.Dtos;
using APIGateWay.Entities;

namespace APIGateWay.Interfaces
{
    public interface IPhotoRepository
    {
        Task<IEnumerable<AddForPhotoApprovalDto>> GetUnapprovedPhotos();
        Task<Photo> GetPhotoById(int id);
        void RemovePhoto(Photo photo);  

    }
}
