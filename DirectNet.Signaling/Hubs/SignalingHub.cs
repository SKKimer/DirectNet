using DirectNet.Signaling.Models;
using DirectNet.Signaling.Services;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace DirectNet.Signaling.Hubs;

/// <summary>
/// SignalR hub responsible for peer discovery and exchanging WAN/LAN endpoints.
/// </summary>
public class SignalingHub : Hub
{
    // Use lobby manager to keep list of lobby codes and host endpoints (Dependency Injection)
    private readonly ILobbyManager _lobbyManager;

    public SignalingHub(ILobbyManager lobbyManager)
    {
        _lobbyManager = lobbyManager;
    }

    /// <summary>
    /// Creates a new matchmaking lobby and returns a unique 6-character code to the host.
    /// </summary>
    public string CreateLobby(PeerEndpointInfo hostEndpoints)
    {
        return _lobbyManager.CreateLobby(hostEndpoints, Context.ConnectionId);
    }

    /// <summary>
    /// Joins an existing lobby and exchanges network endpoints between host and joining peer.
    /// </summary>
    public async Task<bool> JoinLobby(string lobbyCode, PeerEndpointInfo clientEndpoints)
    {
        // Use the lobby manager to try and get and remove the lobby
        if (!_lobbyManager.TryGetAndRemoveLobby(lobbyCode, out var session))
        {
            return false;   // Return false if not found or already matched
        }

        // Send joining client's endpoints to the host (Using SignalR's client manager)
        await Clients.Client(session.HostConnectionId).SendAsync("PeerJoined", clientEndpoints);

        // Send host's endpoints back to the joining client
        await Clients.Caller.SendAsync("HostFound", session.HostEndpoints);

        return true;
    }

    /// <summary>
    /// Removes stale lobbies hosted by the disconnected client.
    /// </summary>
    public override Task OnDisconnectedAsync(Exception? exception)
    {
        // Use the lobby manager to remove stale connections for a given host
        _lobbyManager.RemoveLobbiesByHost(Context.ConnectionId);

        // Let SignalR close own resources down
        return base.OnDisconnectedAsync(exception);
    }
}