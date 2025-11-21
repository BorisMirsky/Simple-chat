using Microsoft.AspNetCore.SignalR;
using Chat.Models;

namespace Chat.Hubs
{


    public interface IChatClient
    {
        public Task ReceiveMessage(string userName, string message);
    }
    public class ChatHub : Hub<IChatClient>
    {
        public async Task JoinChat(UserConnection connection)
        {
            // Одна группа это чат. Тут добавление юзера в чат
            await Groups.AddToGroupAsync(
                Context.ConnectionId,           // from Hub
                connection.ChatRoom);           // from client

            // оповещение что новый юзер в чате
            await Clients.
                Group(connection.ChatRoom).
                ReceiveMessage("Admin", $"{connection.UserName} вошёл в чат");

        }
    }
}
