using System;
using C64Emulator.Devices;
using C64Emulator.Input;

namespace C64Emulator.Networking;

internal sealed class JoystickEventArgs(int joystickNumber, JoystickFunction function, bool pressed) : EventArgs
{
    public int JoystickNumber { get; } = joystickNumber;

    public JoystickFunction Function { get; } = function;

    public bool Pressed { get; } = pressed;
}

/// <summary>
/// The host/client protocol. This class knows how to frame and decode
/// packets; it deliberately does not reach into the emulated machine. The
/// host turns screen bytes into pixels and applies input events.
/// </summary>
internal sealed class Network
{
    public const int NetworkPort = 6464;

    private readonly NetworkTransport socket;
    private volatile bool hosting;
    private volatile bool client;
    private volatile bool imageReady;

    public Network()
    {
        socket = new NetworkTransport();
        socket.Connected += SocketConnected;
        socket.ConnectFailed += SocketConnectFailed;
        socket.IncomingConnection += SocketIncomingConnection;
        socket.ReceivedCommand += SocketReceivedCommand;
        socket.Disconnected += SocketDisconnected;
    }

    public event Action<byte[]>? ScreenDataReceived;

    public event Action<KeyboardEventArgs>? KeyEventReceived;

    public event Action<JoystickEventArgs>? JoyEventReceived;

    public event Action? ClientConnected;

    public event Action<Exception>? ClientConnectFailed;

    public event Action? ClientDisconnected;

    public event Action? IncomingClient;

    public event Action? PartedClient;

    public bool IsHosting => hosting;

    public bool IsClient => client;

    public bool ImageReady => imageReady;

    public int HostNumClients => socket.ClientCount;

    public void HostBegin(int port)
    {
        if (client)
            return;

        socket.Listen(port);
        hosting = true;
    }

    public void HostEnd()
    {
        if (client)
            return;

        socket.StopListening();
        SendDisconnect();
        socket.DisconnectAll();
        hosting = false;
    }

    public void Send(NetworkCommand command, byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);

        if (socket.ClientCount == 0)
            return;

        if (hosting && command == NetworkCommand.Image)
            imageReady = false;

        var packet = new byte[5 + data.Length];
        packet[0] = (byte)command;

        System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(
            packet.AsSpan(1, 4),
            data.Length);

        data.CopyTo(packet, 5);
        socket.SendToAll(packet);
    }

    public void ClientConnect(string server, int port)
    {
        if (hosting)
            return;

        client = true;
        socket.Connect(server, port);
    }

    public void ClientDisconnect()
    {
        if (hosting)
            return;

        SendDisconnect();
        socket.DisconnectAll();
        client = false;
    }

    private void SendDisconnect()
    {
        if (socket.ClientCount == 0)
            return;

        var packet = new byte[5];
        packet[0] = (byte)NetworkCommand.Disconnect;
        socket.SendToAll(packet);
    }

    private void SocketIncomingConnection()
    {
        IncomingClient?.Invoke();
    }

    private void SocketReceivedCommand(NetworkCommand command, byte[] data)
    {
        ProcessCommand(command, data);
    }

    private void SocketConnected()
    {
        if (!client)
            return;

        ClientConnected?.Invoke();
        socket.SendToAll([(byte)NetworkCommand.ImageReady, 0, 0, 0, 0]);
    }

    private void SocketConnectFailed(Exception exception)
    {
        if (!client)
            return;

        ClientConnectFailed?.Invoke(exception);
        client = false;
    }

    private void SocketDisconnected()
    {
        if (client)
        {
            ClientDisconnected?.Invoke();
            client = false;
        }
        else
        {
            PartedClient?.Invoke();
        }
    }

    private void ProcessCommand(NetworkCommand command, byte[] data)
    {
        switch (command)
        {
            case NetworkCommand.Image:
                ScreenDataReceived?.Invoke(data);
                socket.SendToAll([(byte)NetworkCommand.ImageReady, 0, 0, 0, 0]);
                break;

            case NetworkCommand.ImageReady:
                imageReady = true;
                break;

            case NetworkCommand.KeyEvent when data.Length >= 3:
                KeyboardEventArgs keyEvent =
                    new KeyboardEventArgs(
                        (Key)data[0],
                        data[1] != 0 ? ModifierKeys.LeftShift : ModifierKeys.None,
                        data[2] == 1);

                // The typed character is what selects the C64 key when the
                // client sends it; older clients send only the key position.
                if (data.Length >= 4 && data[3] != 0)
                    keyEvent.KeyboardCharacter = ((char)data[3]).ToString();

                KeyEventReceived?.Invoke(keyEvent);
                break;

            case NetworkCommand.JoyEvent when data.Length >= 3:
                JoyEventReceived?.Invoke(
                    new JoystickEventArgs(
                        data[0],
                        (JoystickFunction)data[1],
                        data[2] == 1));
                break;
        }
    }
}
