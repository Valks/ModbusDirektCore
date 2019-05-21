/*
 * GNU GENERAL PUBLIC LICENSE Version 3, 29 June 2007
 * http://www.rossmann-engineering.de
 */

using ModbusDirekt.Modbus;
using ModbusDirekt.Modbus.Channel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ModbusDirekt.Modbus
{
    /// <summary>
    /// Single bit read-only data
    /// </summary>
    public class DiscreteInputs : IModbusResponse
    {
        public BitArray Bits { get; }
        public byte[] Data { get; }

        public DiscreteInputs()
        {
        }

        public DiscreteInputs(byte[] data)
        {
            this.Data = data;
            this.Bits = new System.Collections.BitArray(data);
        }

        public DiscreteInputs(ModbusResponse response): this(response.Data)
        {
            
        }

        public bool this[int i]
        {
            
            get { return Bits[i]; }
            set
            {
                this.Bits[i] = value;
            }
        }
    }
}