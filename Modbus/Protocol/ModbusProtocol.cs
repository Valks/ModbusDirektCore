/*
 * GNU GENERAL PUBLIC LICENSE Version 3, 29 June 2007
 * http://www.rossmann-engineering.de
 */

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ModbusDirekt
{
    public enum ProtocolType { ModbusTCP = 0, ModbusUDP = 1, ModbusRTU = 2 };

    /// <summary>
    /// Modbus Protocol informations.
    /// </summary>
    public class ModbusProtocol
    {
        public bool Request { get; set; }
        public bool Response { get; set; }
        public ushort TransactionIdentifier { get; set; }
        public ushort ProtocolIdentifier { get; set; }
        public ushort Length { get; set; }
        public byte UnitIdentifier { get; set; }
        public byte FunctionCode { get; set; }
        public ushort StartingAdress { get; set; }
        public ushort StartingAddressRead { get; set; }
        public ushort StartingAddressWrite { get; set; }
        public ushort Quantity { get; set; }
        public ushort QuantityRead { get; set; }
        public ushort QuantityWrite { get; set; }
        public byte ByteCount { get; set; }
        public byte ExceptionCode { get; set; }
        public byte ErrorCode { get; set; }
        public ushort[] ReceiveCoilValues { get; set; }
        public ushort[] ReceiveRegisterValues { get; set; }
        public short[] SendRegisterValues { get; set; }
        public bool[] SendCoilValues { get; set; }
        public ushort Crc { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}