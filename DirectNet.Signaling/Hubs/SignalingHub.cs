using System.Collections.Concurrent;
using DirectNet.Signaling.Models;
using Microsoft.AspNetCore.SignalR;

namespace DirectNet.Signaling.Hubs;

/// <summary>
/// SignalR hub responsible for peer discovery and exchanging WAN/LAN endpoints.
/// </summary>
public class SignalingHub : Hub
{
    // In-memory storage for active lobby sessions
    private static readonly ConcurrentDictionary<string, LobbySession> Lobbies = new();

    /// <summary>
    /// Creates a new matchmaking lobby and returns a unique 6-character code to the host.
    /// </summary>
    public string CreateLobby(PeerEndpointInfo hostEndpoints)
    {
        string lobbyCode;

        // Generate a unique 6-character alphanumeric code
        do
        {
            string guid = Guid.NewGuid().ToString("N");     // Generate a GUID and remove hyphens
            lobbyCode = guid[..6].ToUpper();                // Only take first 6 characters and make uppercase
        }
        while (Lobbies.ContainsKey(lobbyCode));             // Check for duplicate

        var session = new LobbySession(lobbyCode, Context.ConnectionId, hostEndpoints);
        Lobbies[lobbyCode] = session;

        return lobbyCode;
    }

    /// <summary>
    /// Joins an existing lobby and exchanges network endpoints between host and joining peer.
    /// </summary>
    public async Task<bool> JoinLobby(string lobbyCode, PeerEndpointInfo clientEndpoints)
    {
        if (!Lobbies.TryRemove(lobbyCode.ToUpper(), out var session))
        {
            // Lobby not found or already matched
            return false;
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
    /// <param name="exception">The exception that caused the disconnection, if any (self close == no exception).</param>
    /// <returns>A task that represents the asynchronous disconnection operation.</returns>
    public override Task OnDisconnectedAsync(Exception? exception)
    {
        // Look up all stale lobbies by Connection Id
        var staleLobbies = Lobbies.Where(kvp => kvp.Value.HostConnectionId == Context.ConnectionId).ToList();

        // Loop through and remove any stale lobbies by the host
        foreach (var (code, _) in staleLobbies)
        {
            Lobbies.TryRemove(code, out _);
        }

        // Let SignalR close own ressources down
        return base.OnDisconnectedAsync(exception);
    }
}