// Starts ASP.NET Core - Load configuarations, set up WebApplication...
using DirectNet.Signaling.Hubs;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();      // Add SignalR as requirement 

var app = builder.Build();          // Build the WebAplication 

// Basic confirmation on base URL - e.g. localhost:5000/ -> "DirectNet Sign..."
app.MapGet("/", () => "DirectNet Signaling Server is running!");

// Map the hub to (localhost:5000)/signaling
app.MapHub<SignalingHub>("/signaling"); 

app.Run();                          // Starts the Web Server