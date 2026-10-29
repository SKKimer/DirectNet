using DirectNet.Signaling.Hubs;
using DirectNet.Signaling.Models;
using DirectNet.Signaling.Services;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Moq;

namespace DirectNet.Signaling.Tests;

public class SignalingHubTests
{
    // Mock the client calling the hub (for example for ConnectionId)
    private readonly Mock<IHubCallerClients> _clientsMock;
    // Mock the client proxy
    private readonly Mock<ISingleClientProxy> _clientProxyMock;
    // Mock the client receiving (used when for example calling Caller.SendAsync(...))
    private readonly Mock<HubCallerContext> _contextMock;
    // The instance of the hub we are testing on
    private readonly SignalingHub _hub;
    // Fresh LobbyManager instance for isolated states for each test case.
    private readonly LobbyManager _lobbyManager;

    public SignalingHubTests()
    {
        // Initiate the mocks
        _clientsMock = new Mock<IHubCallerClients>();
        _clientProxyMock = new Mock<ISingleClientProxy>();
        _contextMock = new Mock<HubCallerContext>();

        // Set up the mock context with a fictive ConnectionId
        _contextMock.Setup(c => c.ConnectionId).Returns("test-connection-123");

        // Set up the client proxy to return our mocked caller
        _clientsMock.Setup(c => c.Caller).Returns(_clientProxyMock.Object);

        // Create a new Lobby Manager for each test case
        _lobbyManager = new LobbyManager();

        // Initiate the SignalingHub with the mocked dependencies and or lobby manager
        _hub = new SignalingHub(_lobbyManager)
        {
            Context = _contextMock.Object,
            Clients = _clientsMock.Object
        };
    }

    [Fact]
    public void CreateLobby_ShouldReturnValidLobbyCode_WhenHostEndpointProvided()
    {
        // Arrange
        var hostEndpoints = new PeerEndpointInfo(
            LocalIP: "192.168.1.50",
            PublicIP: "80.160.1.1",
            Port: 5000
        );

        // Act
        var code = _hub.CreateLobby(hostEndpoints);

        // Assert
        code.Should().NotBeNullOrEmpty();
        code.Length.Should().Be(6);
    }

    [Fact]
    public async Task JoinLobby_ShouldReturnFalse_WhenLobbyCodeDoesNotExist()
    {
        // Arrange
        var peerEndpoints = new PeerEndpointInfo(
            LocalIP: "192.168.1.51",
            PublicIP: "80.160.1.2",
            Port: 5001
        );

        // Act
        var result = await _hub.JoinLobby("INVALID", peerEndpoints);

        // Assert
        result.Should().BeFalse();
    }
}