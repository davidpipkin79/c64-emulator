using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace C64Emulator
{
    public enum NetCommands : byte
    {
        None = 0,
        Image = 1,
        ImageReady = 2,
        Sound = 3,
        Chat = 4,
        KeyEvent = 5,
        JoyEvent = 6,
        Disconnect = 7
    }

    /// <summary>
    /// Length-prefixed TCP transport shared by host and client. Framing is
    /// intentionally small: one command byte, a 32-bit little-endian payload
    /// length, then the payload.
    /// </summary>
    internal sealed class Socket
    {
        private const int MaximumFrameSize = 1024 * 1024;
        private const int ReceiveBufferSize = 8192;

        private readonly object clientsLock = new object();
        private readonly List<Client> clients = new List<Client>();

        private TcpListener? listener;
        private CancellationTokenSource? listenCancellation;

        public event Action? IncomingConnection;

        public event Action<NetCommands, byte[]>? ReceivedCommand;

        public event Action? Connected;

        public event Action<Exception>? ConnectFailed;

        public event Action? Disconnected;

        public int ClientCount
        {
            get
            {
                lock (clientsLock)
                    return clients.Count;
            }
        }

        public void Listen(int port)
        {
            DisconnectAll();

            listenCancellation = new CancellationTokenSource();
            listener = new TcpListener(IPAddress.Any, port);
            listener.Start();

            _ = AcceptLoopAsync(listener, listenCancellation.Token);
        }

        public void StopListening()
        {
            CancellationTokenSource? cancellation = listenCancellation;
            listenCancellation = null;

            cancellation?.Cancel();
            cancellation?.Dispose();

            listener?.Stop();
            listener = null;
        }

        public void DisconnectAll()
        {
            Client[] snapshot;

            lock (clientsLock)
            {
                snapshot = clients.ToArray();
                clients.Clear();
            }

            foreach (Client client in snapshot)
                CloseClient(client);
        }

        public void Connect(string server, int port)
        {
            _ = ConnectAsync(server, port);
        }

        public void SendToAll(byte[] data)
        {
            Client[] snapshot;

            lock (clientsLock)
                snapshot = clients.ToArray();

            foreach (Client client in snapshot)
                _ = SendAsync(client, data);
        }

        private async Task AcceptLoopAsync(
            TcpListener activeListener,
            CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                TcpClient tcpClient;

                try
                {
                    tcpClient = await activeListener.AcceptTcpClientAsync(cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (ObjectDisposedException)
                {
                    return;
                }
                catch (SocketException)
                {
                    return;
                }

                AddClient(tcpClient);
                IncomingConnection?.Invoke();
            }
        }

        private async Task ConnectAsync(string server, int port)
        {
            TcpClient tcpClient = new TcpClient();

            try
            {
                await tcpClient.ConnectAsync(server, port);
            }
            catch (Exception ex)
            {
                tcpClient.Dispose();
                ConnectFailed?.Invoke(ex);
                return;
            }

            AddClient(tcpClient);
            Connected?.Invoke();
        }

        private void AddClient(TcpClient tcpClient)
        {
            Client client = new Client(tcpClient);

            lock (clientsLock)
                clients.Add(client);

            _ = ReceiveLoopAsync(client);
        }

        private async Task ReceiveLoopAsync(Client client)
        {
            byte[] buffer = new byte[ReceiveBufferSize];

            try
            {
                while (!client.Cancellation.IsCancellationRequested)
                {
                    int length = await client.Stream.ReadAsync(
                        buffer.AsMemory(0, buffer.Length),
                        client.Cancellation.Token);

                    if (length == 0)
                        break;

                    client.Buffer.Write(buffer, 0, length);

                    if (!ParseFrames(client))
                        break;
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (ObjectDisposedException)
            {
            }
            catch (IOException)
            {
            }
            catch (SocketException)
            {
            }
            finally
            {
                RemoveClient(client, true);
            }
        }

        private bool ParseFrames(Client client)
        {
            while (client.Buffer.Length >= 5)
            {
                byte[] data = client.Buffer.GetBuffer();
                NetCommands command = (NetCommands)data[0];
                int length = BinaryPrimitives.ReadInt32LittleEndian(
                    new ReadOnlySpan<byte>(data, 1, 4));

                if (length < 0 || length > MaximumFrameSize)
                    return false;

                if (client.Buffer.Length < length + 5)
                    return true;

                if (command == NetCommands.Disconnect)
                    return false;

                byte[] payload = new byte[length];

                if (length > 0)
                    Buffer.BlockCopy(data, 5, payload, 0, length);

                int remaining = (int)client.Buffer.Length - length - 5;

                if (remaining > 0)
                    Buffer.BlockCopy(data, length + 5, data, 0, remaining);

                client.Buffer.SetLength(remaining);

                try
                {
                    ReceivedCommand?.Invoke(command, payload);
                }
                catch
                {
                    return false;
                }
            }

            return true;
        }

        private async Task SendAsync(Client client, byte[] data)
        {
            try
            {
                await client.SendLock.WaitAsync(client.Cancellation.Token);

                try
                {
                    await client.Stream.WriteAsync(
                        data.AsMemory(0, data.Length),
                        client.Cancellation.Token);
                }
                finally
                {
                    client.SendLock.Release();
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (ObjectDisposedException)
            {
            }
            catch (IOException)
            {
                RemoveClient(client, true);
            }
            catch (SocketException)
            {
                RemoveClient(client, true);
            }
        }

        private void RemoveClient(Client client, bool notify)
        {
            bool removed;

            lock (clientsLock)
                removed = clients.Remove(client);

            CloseClient(client);

            if (removed && notify)
                Disconnected?.Invoke();
        }

        private static void CloseClient(Client client)
        {
            try
            {
                client.Cancellation.Cancel();
            }
            catch (ObjectDisposedException)
            {
            }

            client.TcpClient.Close();
        }
    }

    internal sealed class Client
    {
        public Client(TcpClient tcpClient)
        {
            TcpClient = tcpClient;
            Stream = tcpClient.GetStream();
        }

        public TcpClient TcpClient { get; }

        public NetworkStream Stream { get; }

        public MemoryStream Buffer { get; } = new MemoryStream();

        public SemaphoreSlim SendLock { get; } = new SemaphoreSlim(1, 1);

        public CancellationTokenSource Cancellation { get; } = new CancellationTokenSource();
    }
}
