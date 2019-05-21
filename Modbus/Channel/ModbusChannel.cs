using ModbusDirekt.Exceptions;
using ModbusDirekt.Modbus.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModbusDirekt.Modbus.Channel
{
    public abstract class ModbusChannel
    {
        public delegate void ResponseHandler();

        internal ModbusError SendReadCommand<T>(ModbusRequest request, out T responseType) where T : class, IModbusResponse
        {
            byte[] data = request.GetPDU();
            var responseData = SendReadCommand(data);
            var response = new ModbusResponse(responseData);
            if (response.ExceptionCode > 0)
            {
                var err = new ModbusError(response.ExceptionCode);
                responseType = null;
                return err;
            }
            else
            {
                responseType = (T)Activator.CreateInstance(typeof(T), response);
                return null;
            }
        }

        protected abstract byte[] SendReadCommand(byte[] pdu);

        internal abstract void Connect();
    }
}
