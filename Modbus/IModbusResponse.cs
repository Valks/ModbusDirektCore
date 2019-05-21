using System;
using System.Collections.Generic;
using System.Text;

namespace ModbusDirekt.Modbus
{
    public interface IModbusResponse
    {
        byte[] Data { get; }
    }
}
