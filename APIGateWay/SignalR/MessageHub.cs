using APIGateWay.Data;
using APIGateWay.Dtos;
using APIGateWay.Entities;
using APIGateWay.Extensions;
using APIGateWay.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace APIGateWay.SignalR
{
    [Authorize]
    public class MessageHub : Hub
    {       
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IHubContext<PresenceHub> _hubContext;

        public MessageHub(IUnitOfWork uow, IMapper mapper,
            IHubContext<PresenceHub> hubContext)
        {           
            this._uow = uow;
            this._mapper = mapper;
            this._hubContext = hubContext;
        }
        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var otherUser = httpContext.Request.Query["user"];
            var groupName = GetGroupName(Context.User.GetUsername(), otherUser);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            var group = await AddToGroup(groupName);

            await Clients.Group(groupName).SendAsync("UpdatedGroup", group);

            var messages = await _uow.MessageRepository
                .GetMessageThread(Context.User.GetUsername(), otherUser);
            if(_uow.HasChanges())
            {
                await _uow.Complete();
            }
            await Clients.Caller.SendAsync("ReceiveMessageThread", messages);

        }
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var group = await RemoveFromGroup();
            await Clients.Group(group.Name).SendAsync("UpdatedGroup");
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(CreateMessageDto createMessage)
        {
            var username = Context.User.GetUsername();
            if (username == createMessage.RecipientUsername.ToLower())
            {
                throw new HubException("You cannot send message to yourself");
            }
            var sender = await _uow.UserRepository.GetUserByNameAsync(username);
            var recipient = await _uow.UserRepository.GetUserByNameAsync(createMessage.RecipientUsername);

            if (recipient == null) throw new HubException("Recipient not found");
            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.UserName,
                RecipientUsername = recipient.UserName,
                Content = createMessage.Content
            };
            var groupName = GetGroupName(sender.UserName, recipient.UserName);
            var group = await _uow.MessageRepository.GetMessageGroup(groupName);

            if (group.Connections.Any(c => c.Username != sender.UserName))
            {
                message.DateRead = DateTime.UtcNow;
            }
            else
            {
                var connections= await PresenceTracker.GetConnectionsForUser(recipient.UserName);
                if (connections != null)
                {
                    await _hubContext.Clients.Clients(connections).SendAsync("NewMessageReceived",
                        new { username = sender.UserName, knownAs = sender.KnownAs });
                }
            }

            _uow.MessageRepository.AddMessage(message);

            if (await _uow.Complete())
            {
                await Clients.Group(groupName).SendAsync("NewMessage", _mapper.Map<MessageDto>(message));
            }
        }

        private string GetGroupName(string caller, string otherUser)
        {
           // var isCallerFirst = string.CompareOrdinal(caller, otherUser) < 0;
            return string.CompareOrdinal(caller, otherUser) < 0
                ? $"{caller}-{otherUser}"
                : $"{otherUser}-{caller}";
        }

        private async Task<Group> AddToGroup(string groupName)
        {
            var group = await _uow.MessageRepository.GetMessageGroup(groupName);
            var connection = new Connection(Context.ConnectionId, Context.User.GetUsername());

            if (group == null)
            {
                group = new Group(groupName);
                _uow.MessageRepository.AddGroup(group);
            }
            group.Connections.Add(connection);
            if(await _uow.Complete()) return group;

            throw new HubException("Failed to join group");
        }

        private async Task<Group> RemoveFromGroup()
        {
            var group = await _uow.MessageRepository.GetGroupForConnection(Context.ConnectionId);
            var connection = group.Connections.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            _uow.MessageRepository.RemoveConnection(connection);
            if (await _uow.Complete()) return group;
            throw new HubException("Failed to remove from group");

        }
    }
}

