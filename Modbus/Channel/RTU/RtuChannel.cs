using System;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace ModbusDirekt.Modbus.Channel
{
    public class RtuChannel : ModbusChannel
    {
        private SerialPort serialClient;
        private Stream stream;

        public string CommPort { get; }
        public int BaudRate { get; }
        public int DataBits { get; }
        public Parity Parity { get; }
        public StopBits StopBits { get; }
        public ushort UnitIdentifier { get; }

        public RtuChannel(string commPort, int baudRate = 9600, int dataBits = 8, Parity parity = Parity.None, StopBits stopBits = StopBits.One, int unitIdentifier = 1)
        {
            CommPort = commPort;
            BaudRate = baudRate;
            DataBits = dataBits;
            Parity = parity;
            StopBits = stopBits;
            UnitIdentifier = (ushort)unitIdentifier;
        }

        public RtuChannel(int comPort, int baudRate = 9600, int dataBits = 8, Parity parity = Parity.None, StopBits stopBits = StopBits.One, int unitIdentifier = 1)
            : this($"Com{comPort}", baudRate, dataBits, parity, stopBits, unitIdentifier)
        {

        }

        protected override byte[] SendReadCommand(byte[] pdu)
        {
            if (pdu.Length > 253) throw new ArgumentOutOfRangeException(nameof(pdu), "Modbus RTU limits PDU size to 253 bytes.");

            if (!serialClient.IsOpen)
            {
                Connect();
            }

            var adu = new Modbus.Protocol.ADU(UnitIdentifier, pdu);

            Logger.Debug("Send ModbusRTU-Data: " + BitConverter.ToString(pdu));

            byte[] toSend = adu.GetBytes();

            stream.Flush();

            stream.Write(toSend, 0, toSend.Length);

            byte[] returnBuffer = new byte[ 253];

            int NumberOfBytes = stream.Read(returnBuffer, 0, 253);

            Logger.Info("Receive ModbusRTU-Data: " + BitConverter.ToString(returnBuffer));
            return returnBuffer;

        }

        internal override void Connect()
        {
            if (serialClient?.IsOpen ?? false == false)
            {
                if (!SerialPort.GetPortNames().Contains(CommPort)) throw new ArgumentOutOfRangeException(nameof(CommPort), $"Modbus RTU cannot find serial port: {CommPort}");

                this.serialClient = new SerialPort(CommPort, BaudRate, Parity, DataBits, StopBits);
                this.serialClient.Open();
            }
            this.stream = this.serialClient.BaseStream;
        }

    }
}
