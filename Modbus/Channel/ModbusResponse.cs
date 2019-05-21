using System.Linq;

namespace ModbusDirekt.Modbus.Channel
{
    public class ModbusResponse
    {
        public byte[] RawData { get; }
        public int FunctionCode { get; }
        public int ExceptionCode { get; }
        public int ByteCount { get; }
        public byte[] Data { get; }

        public ModbusResponse(byte[] response)
        {
            this.RawData = response;
            this.FunctionCode = (int)response[0];

            if (FunctionCode >= 0x80)
            {
                this.ExceptionCode = response[1];
                RawData = null;
                return;
            }

            this.ByteCount = (int)response[1];
            this.Data = response.Skip(2).Take(ByteCount).ToArray();
        }
    }
}