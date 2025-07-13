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
        private readonly IMessageRepository _messageRepository;
        private readonly IMapper _mapper;
        private readonly IUserRepository _userRepository;
        private readonly IHubContext<PresenceHub> _hubContext;

        public MessageHub(IMessageRepository messageRepository, IMapper mapper, IUserRepository userRepository,
            IHubContext<PresenceHub> hubContext)
        {
            this._messageRepository = messageRepository;
            this._mapper = mapper;
            this._userRepository = userRepository;
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

            var messages = await _messageRepository.GetMessageThread(Context.User.GetUsername(), otherUser);
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
            var sender = await _userRepository.GetUserByNameAsync(username);
            var recipient = await _userRepository.GetUserByNameAsync(createMessage.RecipientUsername);

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
            var group = await _messageRepository.GetMessageGroup(groupName);

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

            _messageRepository.AddMessage(message);

            if (await _messageRepository.SaveAllAsync())
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
            var group = await _messageRepository.GetMessageGroup(groupName);
            var connection = new Connection(Context.ConnectionId, Context.User.GetUsername());

            if (group == null)
            {
                group = new Group(groupName);
                _messageRepository.AddGroup(group);
            }
            group.Connections.Add(connection);
            if(await _messageRepository.SaveAllAsync()) return group;

            throw new HubException("Failed to join group");
        }

        private async Task<Group> RemoveFromGroup()
        {
            var group = await _messageRepository.GetGroupForConnection(Context.ConnectionId);
            var connection = group.Connections.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            _messageRepository.RemoveConnection(connection);
            if (await _messageRepository.SaveAllAsync()) return group;
            throw new HubException("Failed to remove from group");

        }
    }
}

