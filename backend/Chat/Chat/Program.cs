using Chat.Hubs;
using Chat;
using StackExchange.Redis;


// redis
string host = CredentialsSettings.hostname;
string password = CredentialsSettings.password;
ConfigurationOptions conf = new ConfigurationOptions
{
    EndPoints = { host },
    User = "default",
    Password = password,
};
ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(conf);


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddStackExchangeRedisCache(options =>
{
    //var connection = builder.Configuration.GetConnectionString("Redis");
    options.Configuration = host;  
    options.InstanceName = "default";
});
builder.Services.AddMemoryCache();   // need ?
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(conf));
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});
builder.Services.AddSignalR();

var app = builder.Build();
app.MapHub<ChatHub>("/chat");
app.UseCors();
app.Run();