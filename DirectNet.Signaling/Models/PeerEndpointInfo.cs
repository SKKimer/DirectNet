namespace DirectNet.Signaling.Models;

/// <summary>
/// Represents network endpoint information for a peer (local/public IP and UDP port).
/// </summary>
/// <param name="LocalIP">The peer's local network IP address (e.g., "192.168.1.17").</param>
/// <param name="PublicIP">The peer's public WAN IP address (e.g., "97.2.4.73").</param>
/// <param name="Port">The bound UDP port for incoming P2P connections (e.g., 50000).</param>
public record PeerEndpointInfo(string LocalIP, string PublicIP, int Port);