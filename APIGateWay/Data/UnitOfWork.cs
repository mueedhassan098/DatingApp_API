using APIGateWay.Interfaces;
using AutoMapper;

namespace APIGateWay.Data
{    
    public class UnitOfWork : IUnitOfWork
    {
        private readonly IMapper _mapper;
        private readonly DataContextClass _dataContext;

        public UnitOfWork(IMapper mapper,DataContextClass dataContext)
        {
            this._mapper = mapper;
            this._dataContext = dataContext;
        }
        public IUserRepository UserRepository => new UserRepositroy(_dataContext,_mapper);

        public IMessageRepository MessageRepository => new MessageRepository(_dataContext, _mapper);

        public ILikesRepository LikesRepository => new LikesRepository(_dataContext);

        public IPhotoRepository PhotoRepository => new PhotoRepository(_dataContext);

        public async Task<bool> Complete()
        {
            return await _dataContext.SaveChangesAsync() > 0;
        }

        public bool HasChanges()
        {
            return _dataContext.ChangeTracker.HasChanges();
        }
    }
}
