using ModbusDirekt.Modbus;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModbusDirekt
{
    public static class ResponseHelpers
    {
        public static string GetString(this IModbusResponse reg, bool changeEndian = true)
        {
            return Encoding.ASCII.GetString(reg.Data);
        }

        public static float GetFloat(this IModbusResponse reg, int offset = 0, bool changeEndian = false)
        {
            offset *= 4;
            var ret = new byte[]
            {
                reg.Data[2 + offset],
                reg.Data[3 + offset],
                reg.Data[0 + offset],
                reg.Data[1 + offset]
            };

            Array.Reverse(ret);

            return BitConverter.ToSingle(ret, 0);
        }

        public static UInt32 GetUint32(this IModbusResponse reg)
        {
            byte[] b = (byte[])reg.Data.Clone();
            Array.Reverse(b);

            return BitConverter.ToUInt32(b, 0);
        }

        public static ushort GetUint16(this IModbusResponse reg)
        {
            return BitConverter.ToUInt16(reg.Data, 0);
        }
    }
}
