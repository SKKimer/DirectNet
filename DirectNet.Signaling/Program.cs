using DirectNet.Signaling.Hubs;
using DirectNet.Signaling.Services;

var builder = WebApplication.CreateBuilder(args);

// Register SignalR
builder.Services.AddSignalR();

// Register LobbyManager as SingleTon
builder.Services.AddSingleton<ILobbyManager, LobbyManager>();

// Build the WebAplication 
var app = builder.Build();

// Basic confirmation on base URL - e.g. localhost:5000/ -> "DirectNet Sign..."
app.MapGet("/", () => "DirectNet Signaling Server is running!");

// Map the hub to (localhost:5000)/signaling
app.MapHub<SignalingHub>("/signaling");

// Start the Web Server
app.Run();