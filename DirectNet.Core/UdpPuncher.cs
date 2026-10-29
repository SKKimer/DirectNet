using System.Net;
using System.Net.Sockets;

namespace DirectNet.Core;

internal class UdpPuncher
{
    // Sent repeatedly to open the local NAT firewall
    private const byte PunchMagicByte = 0x01;

    // Sent back to confirm a punch packet was received
    private const byte AckMagicByte = 0x02;

    /// <summary>
    /// Attempts to establish a direct P2P connection via UDP hole punching.
    /// </summary>
    /// <param name="localPort">The local port bound during signaling.</param>
    /// <param name="remoteEndpoint">The public IP and port of the peer.</param>
    /// <param name="timeoutMs">Maximum time to attempt hole punching in milliseconds.</param>
    /// <param name="cancellationToken">Cancellationtoken that gives the caller full control to cancel at any time.</param>
    /// <returns>The established Socket ready for P2P transport, or null if failed.</returns>
    public static async Task<Socket?> PunchAsync(
        int localPort,
        IPEndPoint remoteEndpoint,
        int timeoutMs = 5000,
        CancellationToken cancellationToken = default)
    {
        // Create an IPv4 UDP socket for sending and receiving datagrams
        var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        // Allow reusing the local port immediately after Signaling without waiting for OS cooldown
        socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

        // Bind locally so outbound and inbound traffic share the exact same port gotten from Signaling
        socket.Bind(new IPEndPoint(IPAddress.Any, localPort));

        // Combine external cancellation with an internal timeout
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(timeoutMs);

        try
        {
            // SEND PUNCH PACKETS:
            // Start a background task to periodically send punch packets to the peer
            var sendTask = Task.Run(async () =>
            {
                // Pack the Punch Byte into a byte array leaving buffer for the socket
                byte[] punchPacket = [PunchMagicByte];

                while (!cts.Token.IsCancellationRequested)
                {
                    await socket.SendToAsync(punchPacket, SocketFlags.None, remoteEndpoint, cts.Token);
                    await Task.Delay(200, cts.Token);
                }
            }, cts.Token);

            // RECEIVE INCOMING PACKETS:
            // Buffer to receive incoming handshake packets
            byte[] buffer = new byte[1];

            // Receive packets from any IP on this port - later sort random noise away
            EndPoint senderRemote = new IPEndPoint(IPAddress.Any, 0);

            while (!cts.Token.IsCancellationRequested)
            {
                // Receive incoming data into the buffer and capture sender details in result
                var result = await socket.ReceiveFromAsync(buffer, SocketFlags.None, senderRemote, cts.Token);

                // Verify that the packet came from our expected peer
                if (result.ReceivedBytes > 0 && result.RemoteEndPoint.Equals(remoteEndpoint))
                {
                    // Peer sent a punch - acknowledge it by sending ACK back, so they know we are open
                    if (buffer[0] == PunchMagicByte)
                    {
                        byte[] ackPacket = [AckMagicByte];
                        await socket.SendToAsync(ackPacket, SocketFlags.None, remoteEndpoint, CancellationToken.None);

                        cts.Cancel();
                        return socket;
                    }
                    // Peer received our punch and sent an ACK back - hole punch complete!
                    else if (buffer[0] == AckMagicByte)
                    {
                        cts.Cancel();
                        return socket;
                    }
                }
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Caller explicitly requested cancellation; propagate the cancellation up
            throw;
        }
        catch (OperationCanceledException)
        {
            // Internal timeout reached; hole punch failed gracefully
        }
        catch (SocketException)
        {
            // Socket error occurred during punching; treat as hole punch failure
        }

        // Clean up socket if hole punching failed or timed out
        socket.Dispose();
        return null;
    }
}