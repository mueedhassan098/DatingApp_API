using APIGateWay.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace APIGateWay.SignalR
{
    [Authorize]
    public class PresenceHub : Hub
    {
        private readonly PresenceTracker _presenceTracker;

        public PresenceHub(PresenceTracker presenceTracker)
        {
            this._presenceTracker = presenceTracker;
        }
        public override async Task OnConnectedAsync()
        {
           var isOnline = await _presenceTracker.UserConnected(Context.User.GetUsername(), Context.ConnectionId);
            if(isOnline)
            {
                await Clients.Others.SendAsync("UserIsOnline", Context.User.GetUsername());
            }
            await Clients.Others.SendAsync("UserIsOnline", Context.User.GetUsername());
            
            var currentUser = await _presenceTracker.GetOnlineUsers();

            await Clients.Caller.SendAsync("GetOnlineUsers", currentUser);


        }
        public override async Task OnDisconnectedAsync(Exception exception)
        {
           var isOfline= await _presenceTracker.UserDisconnected(Context.User.GetUsername(), Context.ConnectionId);
             
            if (isOfline)
            {
                await Clients.Others.SendAsync("UserIsOffline", Context.User.GetUsername());
            }
            await Clients.Others.SendAsync("UserIsOffline", Context.User.GetUsername());

            //var currentUser = await _presenceTracker.GetOnlineUsers();

            //await Clients.All.SendAsync("GetOnlineUsers", currentUser);

            await base.OnDisconnectedAsync(exception);
        }
    }

}
