using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
namespace ModbusDirekt.Modbus.Channel
{

    struct MBAPHeader
    {
        

        //All fields are encoded in Big-endian

        byte[] TransactionIdentifierByte; // 2 bytes, server repeats this to pair responses to requests
        byte[] ProtocolIdentifierByte; // 2 bytes, always 0x00 for MODBUS Protocol
        public byte[] LengthByte; // 2 bytes, number of bytes to follow
        byte UnitIdentifierByte; // 1 byte

        public int TransactionIdentifier => _transactionIdentifier;
        public int ProtocolIdentifier => _protocolIdentifier;
        public int Length => _length;
        public int UnitIdentifier => _unitIdentifier;

        private int _transactionIdentifier;
        private int _length;
        private int _unitIdentifier;
        private int _protocolIdentifier;

        private byte[] mbapBytes;

        internal byte[] ToByte()
        {
            return mbapBytes;

            return new byte[]
            {
                this.TransactionIdentifierByte[1],
                this.TransactionIdentifierByte[0],
                this.ProtocolIdentifierByte[1],
                this.ProtocolIdentifierByte[0],
                this.LengthByte[1],
                this.LengthByte[0],
                UnitIdentifierByte
            };

        }

        public MBAPHeader(int transactionIdentifier, int length, int unitIdentifier)
        {
            this._transactionIdentifier = transactionIdentifier;
            this._length = length;
            this._unitIdentifier = unitIdentifier;
            this._protocolIdentifier = 0;

            TransactionIdentifierByte = BitConverter.GetBytes((short)transactionIdentifier);
            Array.Reverse(TransactionIdentifierByte);

            this.ProtocolIdentifierByte = new byte[] { 0x00, 0x00 };

            this.LengthByte = BitConverter.GetBytes((short)length);
            Array.Reverse(LengthByte);

            this.UnitIdentifierByte = BitConverter.GetBytes(unitIdentifier)[0];

            mbapBytes = new byte[7];
            TransactionIdentifierByte.CopyTo(mbapBytes, 0);
            ProtocolIdentifierByte.CopyTo(mbapBytes, 2);
            LengthByte.CopyTo(mbapBytes, 4);
            mbapBytes[6] = UnitIdentifierByte;
        }

        internal static MBAPHeader FromByte(byte[] response)
        {
            if (response.Length != 7) throw new ArgumentException("The MBAP frame needs to be exactly 7 bytes.");

            var newMBAP = default(MBAPHeader);
            newMBAP.mbapBytes = response;
            newMBAP.TransactionIdentifierByte = response.Take(2).ToArray();
            newMBAP.ProtocolIdentifierByte = response.Skip(2).Take(2).ToArray();
            newMBAP.LengthByte = response.Skip(4).Take(2).ToArray();
            newMBAP.UnitIdentifierByte = response[6];

            newMBAP._transactionIdentifier = BitConverter.ToInt16(newMBAP.TransactionIdentifierByte.Reverse().ToArray(), 0);
            newMBAP._protocolIdentifier = BitConverter.ToInt16(newMBAP.ProtocolIdentifierByte.Reverse().ToArray(), 0);
            newMBAP._length = BitConverter.ToInt16(newMBAP.LengthByte.Reverse().ToArray(), 0);
            newMBAP._unitIdentifier = (int)newMBAP.UnitIdentifier;

            return newMBAP;
        }
    }
}
