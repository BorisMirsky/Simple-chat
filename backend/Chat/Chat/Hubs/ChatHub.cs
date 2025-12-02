using Chat.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using System.Diagnostics;
using System.Text.Json;



namespace Chat.Hubs
{
    public interface IChatClient
    {
        public Task ReceiveMessage(string userName, string message);
    }

    public class ChatHub : Hub<IChatClient>
    {
        private readonly IDatabase _redis;
        public ChatHub(IConnectionMultiplexer redis)  
        {
            _redis = redis.GetDatabase();
        }

        public async Task JoinChat(UserConnection connection)
        {
            // Одна группа == один чат.
            await Groups.AddToGroupAsync(
                Context.ConnectionId,           // from Hub
                connection.ChatRoom);           // from client
            // данные сериализовать
            var stringConnection = JsonSerializer.Serialize(connection);
            // сохранение в кеш: (ключ, данные)
            await _redis.StringSetAsync(Context.ConnectionId, stringConnection, TimeSpan.FromMinutes(2));
            // оповещение что новый юзер в чате
            await Clients.
                Group(connection.ChatRoom).
                ReceiveMessage("Admin", $"{connection.UserName} вошёл в чат");
        }



        public async Task SendMessage(string message)
        {
            // вынимаем из кеша по ключу
            var stringConnection = await _redis.StringGetAsync(Context.ConnectionId);
            var connection = JsonSerializer.Deserialize<UserConnection>(stringConnection!);
            if (connection is not null)
            {
                await Clients
                    .Group(connection.ChatRoom)
                    .ReceiveMessage(connection.UserName, message);
            }
        }

        // разрыв соединения -> очистка кеша
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var stringConnection = await _redis.StringGetAsync(Context.ConnectionId);
            var connection = JsonSerializer.Deserialize<UserConnection>(stringConnection!);
            if (connection is not null)
            {
                Debug.WriteLine(stringConnection.ToString());
                Debug.WriteLine(connection.ToString());
                // удалить кеш
                await _redis.ListRemoveAsync(Context.ConnectionId, connection.ChatRoom);
                // удалить юзера из группы
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, connection.ChatRoom);
                await Clients
                    .Group(connection.ChatRoom)
                    .ReceiveMessage("Admin", $"{connection.UserName} покинул чат");
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}
