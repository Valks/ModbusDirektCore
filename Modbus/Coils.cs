/*
 * GNU GENERAL PUBLIC LICENSE Version 3, 29 June 2007
 * http://www.rossmann-engineering.de
 */

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
    /// Single bit read-write
    /// </summary>
    public class Coils : IModbusResponse
    {
        public byte[] Data { get; }

        private BitArray _coils;

        public Coils()
        {
        }

        public Coils(ModbusResponse response)
        {
            this.Data = response.Data;
            this._coils = new BitArray(Data);
        }

        public bool this[int i]
        {
            get { return _coils.Get(i); }
            set { _coils.Set(i, value);}
        }
    }
}