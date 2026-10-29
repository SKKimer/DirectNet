using DirectNet.Signaling.Models;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace DirectNet.Signaling.Services;

public class LobbyManager : ILobbyManager
{
    // List of lobby codes matched to the lobby session object
    private readonly ConcurrentDictionary<string, LobbySession> _lobbies = new();

    public string CreateLobby(PeerEndpointInfo hostEndpoints, string connectionId)
    {
        string lobbyCode;

        // Generate a unique 6-character alphanumeric code
        do
        {
            string guid = Guid.NewGuid().ToString("N");     // Generate a GUID and remove hyphens
            lobbyCode = guid[..6].ToUpper();                // Only take first 6 characters and make uppercase
        }
        while (_lobbies.ContainsKey(lobbyCode));            // Check for duplicate

        var session = new LobbySession(lobbyCode, connectionId, hostEndpoints);
        _lobbies[lobbyCode] = session;

        return lobbyCode;
    }

    public bool TryGetAndRemoveLobby(string code, [NotNullWhen(true)] out LobbySession? session)
    {
        return _lobbies.TryRemove(code.ToUpper(), out session);
    }

    public void RemoveLobbiesByHost(string connectionId)
    {
        // Find all stale lobbies for the given host
        var staleLobbies = _lobbies.Where(kvp => kvp.Value.HostConnectionId == connectionId).ToList();

        // Remove them from lobbies
        foreach (var (code, _) in staleLobbies)
        {
            _lobbies.TryRemove(code, out _);
        }
    }
}