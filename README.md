# DirectNet

>**Status:** Under active development. The matchmaking server is complete; the direct device-to-device connection logic is currently being built.

![C#](https://img.shields.io/badge/C%23-12.0-512BD4?style=flat-square&logo=csharp&logoColor=white)
![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat-square&logo=dotnet&logoColor=white)
![Protocol](https://img.shields.io/badge/Protocol-UDP%20P2P-orange?style=flat-square)
![Platform](https://img.shields.io/badge/Platform-Cross--Platform-blue?style=flat-square)
![License](https://img.shields.io/badge/License-MIT-green?style=flat-square)

Lightweight P2P networking library for C# using UDP NAT hole punching. <br> <br>
Designed to establish direct device-to-device connections for multiplayer games, eliminating the need for  dedicated game servers. By utilizing a lightweight signaling server only during initial matchmaking, infrastructure overhead is kept to an absolute minimum.


## Sequence Diagram
<picture>
  <source media="(prefers-color-scheme: dark)" srcset="docs/sequence-diagram-dark.png">
  <source media="(prefers-color-scheme: light)" srcset="docs/sequence-diagram-light.png">
  <img alt="DirectNet Protocol Sequence Diagram" src="docs/sequence-diagram-light.png" width="650">
</picture>

<br>

1. **CreateLobby:** Host creates a lobby on the Signaling Server and receives a unique join code.
2. **JoinLobby:** Peer joins using the code, registering their public endpoint with the Signaling Server.
3. **Exchange Endpoints:** Signaling Server exchanges endpoint metadata between Host and Peer, after which peers disconnect from the server.
4. **Send Punch (`0x01`):** Both peers simultaneously send UDP punch datagrams to open outbound firewall mappings.
5. **Send ACK (`0x02`):** Upon receiving a punch, each peer responds with an ACK packet to confirm bidirectional path connectivity.
6. **Send Game Data / Events:** Direct P2P transport channel is established for low-latency game data transmission.

## Technical Specifications

- **Transport:** Low-latency UDP (`SocketType.Dgram`) for direct P2P messaging.
- **Signaling:** ASP.NET Core SignalR for lobby management and endpoint exchange.
- **Port Reuse:** `SocketOptionName.ReuseAddress` enabled to bind the signaling port immediately without OS cooldown.
- **Handshake Protocol:** Single-byte payloads - `0x01` (Punch) and `0x02` (ACK).
- **Security & Lifecycle:** Endpoint verification against signaling data and `CancellationToken` timeout management.