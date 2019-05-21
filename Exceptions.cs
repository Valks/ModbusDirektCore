using ModbusDirekt.Modbus.Protocol;
using System;

namespace ModbusDirekt.Exceptions {  
    public class ModbusException : Exception
    {

        public ModbusException(int errorCode, string exMesssage)
            :base (exMesssage)
        {
            this.ExceptionCode = errorCode;
        }

        public ModbusException() : base()
        {
        }

        public ModbusException(string message) : base(message)
        {
        }

        public ModbusException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public int ExceptionCode { get; internal set; }
        

        public static ModbusException FromProtocolException(ModbusProtocolException modbusError)
        {
            string exMesssage;

            switch (modbusError)
            {
                case ModbusProtocolException.ILLEGAL_FUNCTION: exMesssage = ModbusError.ILLEGAL_FUNCTION; break;
                case ModbusProtocolException.ILLEGAL_DATA_ADDRESS: exMesssage = ModbusError.ILLEGAL_DATA_ADDRESS; break;
                case ModbusProtocolException.ILLEGAL_DATA_VALUE: exMesssage = ModbusError.ILLEGAL_DATA_VALUE; break;
                case ModbusProtocolException.SERVER_DEVICE_FAILURE: exMesssage = ModbusError.SERVER_DEVICE_FAILURE; break;
                case ModbusProtocolException.ACKNOWLEDGE: exMesssage = ModbusError.ACKNOWLEDGE; break;
                case ModbusProtocolException.SERVER_DEVICE_BUSY: exMesssage = ModbusError.SERVER_DEVICE_BUSY; break;
                case ModbusProtocolException.MEMORY_PARITY_ERROR: exMesssage = ModbusError.MEMORY_PARITY_ERROR; break;
                case ModbusProtocolException.GATEWAY_PATH_UNAVAILABLE: exMesssage = ModbusError.GATEWAY_PATH_UNAVAILABLE; break;
                case ModbusProtocolException.GATEWAY_TARGET_DEVICE_FAILED_TO_RESPOND: exMesssage = ModbusError.GATEWAY_TARGET_DEVICE_FAILED_TO_RESPOND; break;
                default:
                    return new ModbusException("Unknown error");
            }

            return new ModbusException((int)modbusError, exMesssage);
        }
    }
}