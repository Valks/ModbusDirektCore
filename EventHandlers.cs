using System;
using System.Collections.Generic;
using System.Text;

namespace ModbusDirekt
{
    public class CoilsChangedEventArgs : EventArgs
    {
        public int Coils { get; }
        public int NumberOfCoils { get; }

        public CoilsChangedEventArgs(int coils, int numberOfCoils)
        {
            this.Coils = coils;
            this.NumberOfCoils = numberOfCoils;
        }
    }

    public class HoldingRegistersChangedEventArgs : EventArgs
    {
        public int Registers { get; }
        public int NumberOfRegisters { get; }

        public HoldingRegistersChangedEventArgs(int registers, int numberOfRegisters)
        {
            this.Registers = registers;
            this.NumberOfRegisters = numberOfRegisters;
        }
    }
}
