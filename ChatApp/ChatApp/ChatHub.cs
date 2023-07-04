using Microsoft.AspNetCore.SignalR;

namespace ChatApp
{
    public class ChatHub:Hub
    {
        public async Task SendMessage(string user,string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }


        public async Task SendMessage(string connectionId,string user, string message)
        {
            await Clients.Client(connectionId).SendAsync("ReceiveMessage", user, message);
        }
    }
}
