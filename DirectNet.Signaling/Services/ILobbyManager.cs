using DirectNet.Signaling.Models;
using System.Diagnostics.CodeAnalysis;

namespace DirectNet.Signaling.Services;

public interface ILobbyManager
{
    string CreateLobby(PeerEndpointInfo hostEndpoints, string connectionId);
    bool TryGetAndRemoveLobby(string code, [NotNullWhen(true)] out LobbySession? session);
    void RemoveLobbiesByHost(string connectionId);
}
