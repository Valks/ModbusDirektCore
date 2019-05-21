using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ModbusDirekt.Modbus.Channel
{
    public class ModbusTCPChannel : ModbusChannel
    {
        static int transactionIdentifier = 0; //change to 1

        private TcpClient tcpClient;
        private NetworkStream stream;

        public IPAddress IPAddress { get; }
        public int Port { get; } = 502;
        public int UnitIdentifier { get; }
        public int ConnectionTimeout { get; set; } = 10000;

        public ModbusTCPChannel(IPAddress iPAddress, int unitId, int port = 502)
        {
            this.IPAddress = iPAddress;
            this.Port = port;
            this.UnitIdentifier = unitId;
        }

        public ModbusTCPChannel(string host, int unitId, int port = 502) : this(IPAddress.Parse(host), unitId, port)
        {

        }

        /// <summary>
        /// Establish connection to Master device in case of Modbus TCP. Opens COM-Port in case of Modbus RTU
        /// </summary>
        override internal void Connect()
        {
            if (IPAddress == null) throw new ArgumentNullException("IP-Address cannot be null.");

            Logger.Info("Open TCP-Socket, IP-Address: " + IPAddress + ", Port: " + Port);
            tcpClient = new TcpClient();

            Task result = this.tcpClient.ConnectAsync(IPAddress, this.Port);
            result.Wait();

            stream = tcpClient.GetStream();
            stream.ReadTimeout = ConnectionTimeout;
        }

        /// <summary>
        /// Establish connection to Master device in case of Modbus TCP.
        /// </summary>
        public void Connect(string ipAddress, int port)
        {
            Logger.Info("Open TCP-Socket, IP-Address: " + ipAddress + ", Port: " + port);
            tcpClient = new TcpClient();

            tcpClient.ConnectAsync(ipAddress, port).Wait();

            stream = tcpClient.GetStream();
            stream.ReadTimeout = ConnectionTimeout;

            //ConnectedChanged?.Invoke(this, EventArgs.Empty);
        }

        protected override byte[] SendReadCommand( byte[] pdu)
        {
            MBAPHeader mbap = new MBAPHeader(transactionIdentifier++, pdu.Length + 1, this.UnitIdentifier);

            if (!(tcpClient?.Connected ?? false))
            {
                try
                {
                    tcpClient = new TcpClient();
                    tcpClient.ReceiveTimeout = tcpClient.SendTimeout = ConnectionTimeout;
                    tcpClient.Connect(this.IPAddress, this.Port);
                }
                catch(Exception ex)
                {
                    tcpClient = new TcpClient();
                    tcpClient.Connect(this.IPAddress, this.Port);
                    Logger.Error("Failed to connect to " + this.IPAddress + " : " + ex.Message);
                    throw;
                }
            }
            this.stream = tcpClient.GetStream();

            if (tcpClient.Client.Connected)
            {
                Logger.Debug("Send ModbusTCP-Data: " + BitConverter.ToString(pdu));

                byte[] toSend = new byte[7 + pdu.Length];
                var bMBAP = mbap.ToByte();

                bMBAP.CopyTo(toSend, 0);
                pdu.CopyTo(toSend, 7);

                //Array.Copy(bMBAP, toSend, 7);
                //Array.Copy(pdu, 0, toSend, 7, pdu.Length);

                stream.Flush();

                stream.Write(toSend, 0, toSend.Length);

                byte[] response = new byte[7];
                stream.Read(response, 0, 7);

                var responseMBAP = MBAPHeader.FromByte(response);

                if (pdu.Length < responseMBAP.Length) pdu = new byte[responseMBAP.Length];

                int NumberOfBytes = stream.Read(pdu, 0, responseMBAP.Length);
                
                byte[] returnValue = new byte[NumberOfBytes];
                Array.Copy(pdu, 0, returnValue, 0, NumberOfBytes);

                Logger.Info("Receive ModbusTCP-Data: " + BitConverter.ToString(returnValue));
                return returnValue;
            }
            else
            {
                throw new SocketException();
            }
        }
    }
}
