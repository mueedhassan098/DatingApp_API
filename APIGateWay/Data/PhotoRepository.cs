using APIGateWay.Dtos;
using APIGateWay.Entities;
using APIGateWay.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace APIGateWay.Data
{
    public class PhotoRepository : IPhotoRepository
    {
        private readonly DataContextClass _dataContext;

        public PhotoRepository(DataContextClass dataContext)
        {
            _dataContext = dataContext;
        }
        public async Task<IEnumerable<AddForPhotoApprovalDto>> GetUnapprovedPhotos()
        {
            return await _dataContext.Photos
                 .IgnoreQueryFilters()
                 .Where(p => p.IsApproved == false)
                 .Select(u => new AddForPhotoApprovalDto
                 {
                     Id = u.Id,
                     Username = u.AppUser.UserName,
                     Url = u.Url,
                     IsApproved = u.IsApproved

                 }).ToListAsync();

        }
        public async Task<Photo> GetPhotoById(int id)
        {
            return await _dataContext.Photos
             .IgnoreQueryFilters()
             .SingleOrDefaultAsync(x => x.Id == id);
        }
        public void RemovePhoto(Photo photo)
        {
           _dataContext.Photos.Remove(photo);
        }
    }
}
