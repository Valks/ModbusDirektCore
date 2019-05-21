/*
 * GNU GENERAL PUBLIC LICENSE Version 3, 29 June 2007
 * http://www.rossmann-engineering.de
 */

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ModbusDirekt.Modbus.Protocol
{
    internal class TCPServer
    {
        public delegate void DataChanged(object networkConnectionParameter);

        public event DataChanged dataChanged;

        public delegate void NumberOfClientsChangedDelegate();

        public event NumberOfClientsChangedDelegate numberOfClientsChanged;

        private readonly TcpListener server = null;

        private readonly List<Client> tcpClientLastRequestList = new List<Client>();

        public int NumberOfConnectedClients { get; set; }

        public string ipAddress = null;

        public TCPServer(int port)
        {
            IPAddress localAddr = IPAddress.Any;
            server = new TcpListener(localAddr, port);
            server.Start();
            server.AcceptTcpClientAsync().Start();
        }

        public TCPServer(string ipAddress, int port)
        {
            this.ipAddress = ipAddress;
            IPAddress localAddr = IPAddress.Any;
            server = new TcpListener(localAddr, port);
            server.Start();
            server.AcceptTcpClientAsync().Start();
        }

        private async void AcceptTcpClientCallback(IAsyncResult asyncResult)
        {
            TcpClient tcpClient = new TcpClient();
            try
            {
                tcpClient.ReceiveTimeout = 4000;
                if (ipAddress != null)
                {
                    string ipEndpoint = tcpClient.Client.RemoteEndPoint.ToString();
                    ipEndpoint = ipEndpoint.Split(':')[0];
                    if (ipEndpoint != ipAddress)
                    {
                        tcpClient.Client.Dispose();
                        return;
                    }
                }
            }
            catch { }
            try
            {
                Client client = new Client(tcpClient);
                NetworkStream networkStream = client.NetworkStream;
                networkStream.ReadTimeout = 4000;
                byte[] result = new byte[client.Buffer.Length];
                await networkStream.ReadAsync(result, 0, client.Buffer.Length);
                this.ReadCallback(result);
            }
            catch { }
        }

        private int GetAndCleanNumberOfConnectedClients(Client client)
        {
            lock (this)
            {
                bool objetExists = false;
                foreach (Client clientLoop in tcpClientLastRequestList)
                {
                    if (client.Equals(clientLoop))
                        objetExists = true;
                }
                try
                {
                    tcpClientLastRequestList.RemoveAll((Client c) => DateTime.Now.Ticks - c.Ticks > 40000000);
                }
                catch { }
                if (!objetExists)
                    tcpClientLastRequestList.Add(client);

                return tcpClientLastRequestList.Count;
            }
        }

        private void ReadCallback(byte[] result)
        {

            numberOfClientsChanged?.Invoke();

        }

        public void Disconnect()
        {
                foreach (Client clientLoop in tcpClientLastRequestList)
                {
                    try
                    {
                        clientLoop.NetworkStream.Dispose();
                    }
                    catch { }
                }
            server.Stop();
        }

        internal class Client
        {
            public long Ticks { get; set; }

            public Client(TcpClient tcpClient)
            {
                this.TcpClient = tcpClient;
                int bufferSize = tcpClient.ReceiveBufferSize;
                Buffer = new byte[bufferSize];
            }

            public TcpClient TcpClient { get; }

            public byte[] Buffer { get; }

            public NetworkStream NetworkStream => TcpClient.GetStream();
        }
    }
}