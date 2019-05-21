using System;
using System.Collections.Generic;
using System.Text;
using ModbusDirekt.Modbus.Protocol;

namespace ModbusDirekt.Modbus.Protocol
{
    class ModbusRequest
    {
        public byte[] startAddress;
        private readonly byte[] quantityToRead;
        private readonly byte[] pdu;
        private readonly byte functionCode;


        public ModbusRequest(ModbusFunctions functionCode, int startAddress, int quantityToRead)
        {
            this.startAddress = BitConverter.GetBytes(Convert.ToInt16(startAddress));
            
            this.functionCode = (byte)functionCode;


            this.quantityToRead = BitConverter.GetBytes((short)quantityToRead);

            this.pdu = new byte[]
                            {
                            this.functionCode,
                            this.startAddress[1],
                            this.startAddress[0],
                            this.quantityToRead[1],
                            this.quantityToRead[0]
                            };
        }

        internal byte[] GetPDU()
        {
            return pdu;
        }

        internal static ModbusRequest DiscriteInputRequest(int startingAddress, int quantity)
        {
            return new ModbusRequest(ModbusFunctions.ReadDiscreteInputs, startingAddress, quantity);
        }
    }
}
