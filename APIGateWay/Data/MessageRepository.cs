using APIGateWay.Dtos;
using APIGateWay.Entities;
using APIGateWay.Helpers;
using APIGateWay.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace APIGateWay.Data
{
    public class MessageRepository : IMessageRepository
    {
        private readonly DataContextClass _dataContext;
        private readonly IMapper _mapper;

        public MessageRepository(DataContextClass dataContext,IMapper mapper)
        {
            this._dataContext = dataContext;
            this._mapper = mapper;
        }

        public void AddGroup(Group group)
        {
            _dataContext.Groups.Add(group); //is nay bohot ziyada zleeel kia
        }

        public void AddMessage(Message message)
        {
            _dataContext.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
           _dataContext.Messages.Remove(message);
        }
        public async Task<Connection> GetConnection(string connectionId)
        {
            return await _dataContext.Connections.FindAsync(connectionId);
        }
        public async Task<Group> GetGroupForConnection(string connectionId)
        {
            return await _dataContext.Groups
                .Include(g => g.Connections)
                .Where(g => g.Connections.Any(c => c.ConnectionId == connectionId))
                .FirstOrDefaultAsync();
        }
        public async Task<Message> GetMessage(int id)
        {
           return await _dataContext.Messages.FindAsync(id);
        }
        public async Task<PagedList<MessageDto>> GetMessageForUser(MessageParams messageParams)
        {
            var query = _dataContext.Messages
                .OrderBy(m => m.MessageSent)
                .AsQueryable();
            query = messageParams.Container switch
            {
                "Inbox" => query.Where(m => m.RecipientUsername == messageParams.Username
                    && m.RecipientDeleted == false),
                "Outbox" => query.Where(m => m.SenderUsername == messageParams.Username
                    && m.SenderDeleted==false),
                _ => query.Where(m => m.Recipient.UserName == messageParams.Username
                    && m.RecipientDeleted ==false && m.DateRead == null)
            };
            var messages = query.ProjectTo<MessageDto>(_mapper.ConfigurationProvider);
            return await PagedList<MessageDto>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
        }
        public async Task<Group> GetMessageGroup(string groupName)
        {
            return await _dataContext.Groups
                .Include(g => g.Connections)
                .FirstOrDefaultAsync(g => g.Name == groupName);
        }
        public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUserName, string recipientUserName)
        {
            var messages =await _dataContext.Messages
                .Include(m => m.Sender).ThenInclude(s => s.Photos)
                .Include(m => m.Recipient).ThenInclude(r => r.Photos)
                .Where(
                    m => m.RecipientUsername == currentUserName &&
                    m.RecipientDeleted == false &&
                    m.SenderUsername == recipientUserName ||
                    m.RecipientUsername == recipientUserName &&
                    m.SenderDeleted == false &&
                    m.SenderUsername == currentUserName
                )

                .OrderBy(m => m.MessageSent)
                .ToListAsync();

            var unreadMessages = messages
                .Where(m => m.DateRead == null && m.RecipientUsername == currentUserName)
                .ToList();

            if (unreadMessages.Any())
            {
                foreach (var message in unreadMessages)
                {
                    message.DateRead = DateTime.UtcNow;
                }
                // _dataContext.Messages.UpdateRange(unreadMessages);
                await _dataContext.SaveChangesAsync();
            }
            return _mapper.Map<IEnumerable<MessageDto>>(messages);
        }
        public void RemoveConnection(Connection connection)
        {
            _dataContext.Connections.Remove(connection);  
        }
        public async Task<bool> SaveAllAsync()
        {
            return await _dataContext.SaveChangesAsync() >0;
        }
    } 
}
