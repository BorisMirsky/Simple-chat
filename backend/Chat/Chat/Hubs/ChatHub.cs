using Chat.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
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
        private readonly IDistributedCache _cache;
        public ChatHub(IConnectionMultiplexer redis,
                        IDistributedCache cache)  
        {
            _redis = redis.GetDatabase();
            _cache = cache; 
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
            //await _cache.SetStringAsync(Context.ConnectionId, stringConnection);
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
            //var stringConnection = await _cache.GetAsync(Context.ConnectionId);
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
            //var stringConnection = await _cache.GetAsync(Context.ConnectionId);
            var connection = JsonSerializer.Deserialize<UserConnection>(stringConnection!);
            if (connection is not null)
            {
                // удалить кеш
                await _redis.ListRemoveAsync(Context.ConnectionId, stringConnection, 1); // connection.ChatRoom, 1);
                // удалить юзера из группы
                //await _cache.RemoveAsync(Context.ConnectionId);
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, connection.ChatRoom);
                await Clients
                    .Group(connection.ChatRoom)
                    .ReceiveMessage("Admin", $"{connection.UserName} покинул чат");
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}
