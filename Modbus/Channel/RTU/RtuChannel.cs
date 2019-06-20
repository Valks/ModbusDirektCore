using System;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace ModbusDirekt.Modbus.Channel
{
    public class RtuChannel : ModbusChannel, IDisposable
    {
        private SerialPort _serialClient;
        private Stream _stream;

        public string CommPort { get; }
        public int BaudRate { get; }
        public int DataBits { get; }
        public Parity Parity { get; }
        public StopBits StopBits { get; }
        public bool KeepOpen { get; }
        public ushort UnitIdentifier { get; }

        /// <summary>
        /// Create a Serial RTU connection
        /// </summary>
        /// <param name="commPort">Serial port to connect to</param>
        /// <param name="baudRate">Baud rate</param>
        /// <param name="dataBits">Data bits</param>
        /// <param name="parity">Parity</param>
        /// <param name="stopBits">Stop bits</param>
        /// <param name="unitIdentifier">Modbus Id</param>
        /// <param name="keepOpen">Keep the serial port open after command issued</param>
        public RtuChannel(string commPort, int baudRate = 9600, int dataBits = 8, Parity parity = Parity.None, StopBits stopBits = StopBits.One, int unitIdentifier = 1, bool keepOpen = false)
        {
            CommPort = commPort;
            BaudRate = baudRate;
            DataBits = dataBits;
            Parity = parity;
            StopBits = stopBits;
            KeepOpen = keepOpen;
            UnitIdentifier = (ushort)unitIdentifier;
        }

        /// <summary>
        /// Create a Serial RTU connection
        /// </summary>
        /// <param name="commPort">Serial port to connect to</param>
        /// <param name="baudRate">Baud rate</param>
        /// <param name="dataBits">Data bits</param>
        /// <param name="parity">Parity</param>
        /// <param name="stopBits">Stop bits</param>
        /// <param name="unitIdentifier">Modbus Id</param>
        /// <param name="keepOpen">Keep the serial port open after command issued</param>
        public RtuChannel(int comPort, int baudRate = 9600, int dataBits = 8, Parity parity = Parity.None, StopBits stopBits = StopBits.One, int unitIdentifier = 1, bool keepOpen = false)
            : this($"Com{comPort}", baudRate, dataBits, parity, stopBits, unitIdentifier, keepOpen)
        {

        }

        protected override byte[] SendReadCommand(byte[] pdu)
        {
            if (pdu.Length > 253) throw new ArgumentOutOfRangeException(nameof(pdu), "Modbus RTU limits PDU size to 253 bytes.");

            if (!_serialClient.IsOpen)
            {
                Connect();
            }

            var adu = new Modbus.Protocol.ADU(UnitIdentifier, pdu);

            Logger.Debug("Send ModbusRTU-Data: " + BitConverter.ToString(pdu));

            byte[] toSend = adu.GetBytes();

            _stream.Flush();

            _stream.Write(toSend, 0, toSend.Length);

            byte[] returnBuffer = new byte[253];
            _ = _stream.Read(returnBuffer, 0, 253);

            Logger.Info("Receive ModbusRTU-Data: " + BitConverter.ToString(returnBuffer));

            if (!KeepOpen) Disconnect();

            return returnBuffer;

        }

        internal override void Connect()
        {
            if (_serialClient?.IsOpen ?? false == false)
            {
                if (!SerialPort.GetPortNames().Contains(CommPort)) throw new ArgumentOutOfRangeException(nameof(CommPort), $"Modbus RTU cannot find serial port: {CommPort}");

                _serialClient = new SerialPort(CommPort, BaudRate, Parity, DataBits, StopBits);
                _serialClient.Open();
            }
            _stream = _serialClient.BaseStream;
        }

        public void Disconnect()
        {
            if (_serialClient?.IsOpen ?? false == false) return;

            _serialClient.Close();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _stream?.Dispose();
                    _serialClient?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~RtuChannel()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion

    }
}