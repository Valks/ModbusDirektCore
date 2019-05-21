using System;
using System.Net;
using System.Net.Sockets;

namespace ModbusDirekt.Modbus.Channel
{
    public class RtuTCPChannel : ModbusChannel
    {
        private readonly IPEndPoint serverEndPoint;
        private TcpClient tcpClient;
        private NetworkStream stream;

        public ushort UnitIdentifier { get; }

        public RtuTCPChannel(System.Net.IPAddress iPAddress, int port = 502, int unitIdentifier = 1)
        {
            this.serverEndPoint = new System.Net.IPEndPoint(iPAddress, port);
            this.UnitIdentifier = (ushort)unitIdentifier;
        }

        public RtuTCPChannel(string host, int port = 502, int unitIdentifier = 1) : this(IPAddress.Parse(host), port, unitIdentifier )
        {

        }

        protected override byte[] SendReadCommand(byte[] pdu)
        {
            if (pdu.Length > 253) throw new ArgumentOutOfRangeException("pdu", "Modbus RTU limits PDU size to 253 bytes.");

            if (!tcpClient.Connected)
            {
                Connect();
            }

            var adu = new Modbus.Protocol.ADU(UnitIdentifier, pdu);

            Logger.Debug("Send ModbusTCP-Data: " + BitConverter.ToString(pdu));

            byte[] toSend = adu.GetBytes();

            stream.Flush();

            stream.Write(toSend, 0, toSend.Length);

            byte[] returnBuffer = new byte[ 253];

            int NumberOfBytes = stream.Read(returnBuffer, 0, 253);

            Logger.Info("Receive ModbusTCP-Data: " + BitConverter.ToString(returnBuffer));
            return returnBuffer;

        }

        internal override void Connect()
        {
            if (this.tcpClient?.Connected ?? false == false)
            {
                this.tcpClient = new System.Net.Sockets.TcpClient();
                tcpClient.Connect(serverEndPoint);
            }
            this.stream = tcpClient.GetStream();
        }

    }
}
