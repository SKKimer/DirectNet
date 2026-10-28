namespace DirectNet.Signaling.Models;

/// <summary>
/// Represents an active matchmaking lobby session waiting for a peer connection.
/// </summary>
/// <param name="LobbyCode">A 6-character alphanumeric code (caps) used by clients to join the lobby (e.g., "I7JS25").</param>
/// <param name="HostConnectionId">The SignalR connection ID associated with the host's WebSocket connection.</param>
/// <param name="HostEndpoints">The host's network endpoints (local/public IP and port) used for P2P connection setup.</param>
public record LobbySession(
    string LobbyCode,
    string HostConnectionId,
    PeerEndpointInfo HostEndpoints
);