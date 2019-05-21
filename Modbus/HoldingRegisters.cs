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

namespace ModbusDirekt
{
    /// <summary>
    /// 16-bit Read-Only
    /// </summary>
    public class HoldingRegisters : IModbusResponse
    {
        public Int16[] localArray;
        public byte[] Data { get; }

        public HoldingRegisters()
        {

        }

        public HoldingRegisters(ModbusResponse response)
        {
            Data = response.Data;
        }

        public Int16 this[int x]
        {
            get { return this.localArray[x]; }
            set
            {
                this.localArray[x] = value;
            }
        }

    }
}