using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Distributed;
//using Microsoft.Extensions.Caching.Memory; //.IMemoryCache
using Chat.Models;
using System.Diagnostics;



namespace Chat.Hubs
{
    public interface IChatClient
    {
        public Task ReceiveMessage(string userName, string message);
    }
    // 'Hub' is from ..signalr
    public class ChatHub : Hub<IChatClient>
    {
        private readonly IDistributedCache _cache;
        public ChatHub(IDistributedCache  cache)  
        {
            _cache = cache;
        }

        public async Task JoinChat(UserConnection connection)
        {
            // Одна группа это чат. Тут добавление юзера в чат
            await Groups.AddToGroupAsync(
                "111", //Context.ConnectionId,           // from Hub
                connection.ChatRoom);           // from client
            // данные надо сначала сериализовать
            var stringConnection = JsonSerializer.Serialize(connection);
            // сохранение в кеш: (ключ, данные)
            // error
            await _cache.SetStringAsync("111", stringConnection);  //Context.ConnectionId,
            // оповещение что новый юзер в чате
            Debug.WriteLine("___4___");
            await Clients.
                Group(connection.ChatRoom).
                ReceiveMessage("Admin", $"{connection.UserName} вошёл в чат");
        }



        public async Task SendMessage(string message)
        {
            // вынимаем из кеша по ключу
            var stringConnection = await _cache.GetAsync(Context.ConnectionId);
            var connection = JsonSerializer.Deserialize<UserConnection>(stringConnection);
            if (connection is not null)
            {
                await Clients
                    .Group(connection.ChatRoom)
                    .ReceiveMessage(connection.UserName, message);
            }
        }

        // разрыв соединения -> очистка кеша
        // пергрузка (переопределение) базовой реализации
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var stringConnection = await _cache.GetAsync(Context.ConnectionId);
            var connection = JsonSerializer.Deserialize<UserConnection>(stringConnection);

            if (connection is not null)
            {
                // удалить кеш
                await _cache.RemoveAsync(Context.ConnectionId);
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
