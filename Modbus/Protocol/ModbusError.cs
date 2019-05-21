using System;
using System.Collections.Generic;
using System.Text;

namespace ModbusDirekt.Modbus.Protocol
{
    
    partial class ModbusError
    {
        public ModbusError(object mBExceptionCode)
        {
            this.ExceptionCode = (int)mBExceptionCode;
            this.ProtocolException = (ModbusProtocolException)ExceptionCode;
        }

        public int ExceptionCode { get; internal set; }
        public ModbusProtocolException ProtocolException { get; }
    }
    
    public enum ModbusProtocolException
    {
        ILLEGAL_FUNCTION = 0x01,
        ILLEGAL_DATA_ADDRESS = 0x02,
        ILLEGAL_DATA_VALUE = 0x03,
        SERVER_DEVICE_FAILURE = 0x04,
        ACKNOWLEDGE = 0x05,
        SERVER_DEVICE_BUSY = 0x06,
        MEMORY_PARITY_ERROR = 0x08,
        GATEWAY_PATH_UNAVAILABLE = 0x0A,
        GATEWAY_TARGET_DEVICE_FAILED_TO_RESPOND = 0x0B
    }

}
