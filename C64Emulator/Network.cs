using System;

namespace C64Emulator
{
    internal sealed class JoyEventArgs : EventArgs
    {
        public JoyEventArgs(int joystickNumber, JoystickFunction function, bool pressed)
        {
            JoystickNumber = joystickNumber;
            Function = function;
            Pressed = pressed;
        }

        public int JoystickNumber { get; }

        public JoystickFunction Function { get; }

        public bool Pressed { get; }
    }

    /// <summary>
    /// The host/client protocol. This class knows how to frame and decode
    /// packets; it deliberately does not reach into the emulated machine. The
    /// host turns screen bytes into pixels and applies input events.
    /// </summary>
    internal sealed class Network
    {
        public const int NetworkPort = 6464;

        private readonly Socket socket;
        private volatile bool hosting;
        private volatile bool client;
        private volatile bool imageReady;

        public Network()
        {
            socket = new Socket();
            socket.Connected += SocketConnected;
            socket.ConnectFailed += SocketConnectFailed;
            socket.IncomingConnection += SocketIncomingConnection;
            socket.ReceivedCommand += SocketReceivedCommand;
            socket.Disconnected += SocketDisconnected;
        }

        public event Action<byte[]>? ScreenDataReceived;

        public event Action<KeyboardEventArgs>? KeyEventReceived;

        public event Action<JoyEventArgs>? JoyEventReceived;

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

        public void Send(NetCommands command, byte[] data)
        {
            ArgumentNullException.ThrowIfNull(data);

            if (socket.ClientCount == 0)
                return;

            if (hosting && command == NetCommands.Image)
                imageReady = false;

            byte[] packet = new byte[5 + data.Length];
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

            byte[] packet = new byte[5];
            packet[0] = (byte)NetCommands.Disconnect;
            socket.SendToAll(packet);
        }

        private void SocketIncomingConnection()
        {
            IncomingClient?.Invoke();
        }

        private void SocketReceivedCommand(NetCommands command, byte[] data)
        {
            ProcessCommand(command, data);
        }

        private void SocketConnected()
        {
            if (!client)
                return;

            ClientConnected?.Invoke();
            socket.SendToAll(new byte[] { (byte)NetCommands.ImageReady, 0, 0, 0, 0 });
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

        private void ProcessCommand(NetCommands command, byte[] data)
        {
            switch (command)
            {
                case NetCommands.Image:
                    ScreenDataReceived?.Invoke(data);
                    socket.SendToAll(new byte[] { (byte)NetCommands.ImageReady, 0, 0, 0, 0 });
                    break;

                case NetCommands.ImageReady:
                    imageReady = true;
                    break;

                case NetCommands.KeyEvent when data.Length >= 3:
                    KeyEventReceived?.Invoke(
                        new KeyboardEventArgs(
                            (Key)data[0],
                            data[1] != 0 ? ModifierKeys.LeftShift : ModifierKeys.None,
                            data[2] == 1));
                    break;

                case NetCommands.JoyEvent when data.Length >= 3:
                    JoyEventReceived?.Invoke(
                        new JoyEventArgs(
                            data[0],
                            (JoystickFunction)data[1],
                            data[2] == 1));
                    break;
            }
        }
    }
}
