using System;

namespace ModbusDirekt.Modbus.Protocol
{
    internal class ADU
    {
        private ushort unitIdentifier;
        private byte[] pdu;

        public ADU(ushort unitIdentifier, byte[] pdu)
        {
            this.unitIdentifier = unitIdentifier;
            this.pdu = pdu;
        }

        internal byte[] GetBytes()
        {

            var b = new byte[pdu.Length + 1 + 2];
            b[0] = (byte)unitIdentifier;
            pdu.CopyTo(b, 1);

            byte[] data = new byte[pdu.Length + 1];
            data[0] = (byte)unitIdentifier;
            pdu.CopyTo(data, 1);
            

            var crc = ModbusProtocolHelpers.CRC16(data, data.Length, 0);
            var crc2 = ModbusProtocolHelpers.CRC16(data);

            if(crc.Item1 != crc2.Item1 ||crc.Item2 != crc2.Item2)
            {
                bool error = true;
                Console.WriteLine(error);
            }

            b[b.Length - 1] = crc.Item1;
            b[b.Length - 2] = crc.Item2;

            return b;
        }
    }
}