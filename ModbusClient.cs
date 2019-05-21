using ModbusDirekt.Exceptions;
using ModbusDirekt.Modbus;
using ModbusDirekt.Modbus.Channel;
using ModbusDirekt.Modbus.Protocol;
using System;

namespace ModbusDirekt
{
    /// <summary>
    /// Implements a ModbusClient
    /// </summary>
    public partial class ModbusClient
    {

        private ModbusChannel modbusChannel;

        public int NumberOfRetries { get; set; } = 3;

        /// <summary>
        /// Constructor which determines the Master ip-address and the Master Port.
        /// </summary>
        /// <param name="ipAddress">IP-Address of the Master device</param>
        /// <param name="port">Listening port of the Master device (should be 502)</param>
        public ModbusClient(ModbusChannel channel)
        {
            this.modbusChannel = channel;
        }

        /// <summary>
        /// Read Discrete Inputs from Server device (FC2).
        /// </summary>
        /// <param name="startingAddress">First discrete input to read</param>
        /// <param name="quantity">Number of discrete Inputs to read</param>
        /// <returns>Boolean Array which contains the discrete Inputs</returns>
        public Modbus.DiscreteInputs ReadDiscreteInputs(int startingAddress, int quantity)
        {
            Logger.Info($"FC2 (Read Discrete Inputs from StartingAddress: { startingAddress }, Quantity: { quantity }");
            
            if (this.modbusChannel == null)
            {
                Logger.Error("ConnectionException Throwed");
                throw new InvalidOperationException("No ModbusChannel initialized.");
            }

            if (startingAddress > 65535 || quantity > 2000)
            {
                Logger.Error("ArgumentException Throwed");
                throw new ArgumentException("Starting address must be 0 - 65535; quantity must be 0 - 2000");
            }
            
            var request = ModbusRequest.DiscriteInputRequest(startingAddress, quantity);

            var error = modbusChannel.SendReadCommand<DiscreteInputs>(request, out DiscreteInputs inputs);

            if(error != null)
            {
                throw ModbusException.FromProtocolException(error.ProtocolException);
            }

            return inputs;
        }

        /// <summary>
        /// Read Coils from Server device (FC1).
        /// </summary>
        /// <param name="startingAddress">First coil to read</param>
        /// <param name="quantity">Numer of coils to read</param>
        /// <returns>Boolean Array which contains the coils</returns>
        public Coils ReadCoils(int startingAddress, int quantity)
        {
            Logger.Info("FC1 (Read Coils from Master device), StartingAddress: " + startingAddress + ", Quantity: " + quantity);
            
            if (modbusChannel == null)
            {
                throw new  InvalidOperationException("No ModbusChannel initialized.");
            }
            if (startingAddress > 65535 || quantity > 2000)
            {
                throw new ArgumentException("Starting address must be 0 - 65535; quantity must be 0 - 2000");
            }

            var request = new ModbusRequest(ModbusFunctions.ReadCoils, startingAddress, quantity);
            var error = modbusChannel.SendReadCommand(request, out Coils coils);

            if (error != null) throw ModbusException.FromProtocolException(error.ProtocolException);

            return coils;
        }

        /// <summary>
        /// Read Holding Registers from Master device (FC3).
        /// </summary>
        /// <param name="startingAddress">First holding register to be read. Integer between 0 and 65535</param>
        /// <param name="quantity">Number of holding registers to be read. Integer between 0 and 125.</param>
        /// <returns>Int Array which contains the holding registers</returns>
        public HoldingRegisters ReadHoldingRegisters(int startingAddress, int quantity)
        {
            Logger.Info("FC3 Read Holding Registers from Master device), StartingAddress: " + startingAddress + ", Quantity: " + quantity);
            if (modbusChannel == null)
            {
                Logger.Error("ConnectionException Throwed");
                throw new InvalidOperationException("No ModbusChannel initialized.");
            }
            if (startingAddress > 65535 || quantity > 125)
            {
                Logger.Error("ArgumentException Throwed");
                throw new ArgumentException("Starting address must be 0 - 65535; quantity must be 0 - 125");
            }

            var request = new ModbusRequest( ModbusFunctions.ReadHoldingRegisters, startingAddress, quantity);
            var error = modbusChannel.SendReadCommand(request, out HoldingRegisters holdingReg);

            if (error != null) throw ModbusException.FromProtocolException(error.ProtocolException);

            return holdingReg;
        }

        /// <summary>
        /// Read Input Registers from Master device (FC4).
        /// </summary>
        /// <param name="startingAddress">First input register to be read</param>
        /// <param name="quantity">Number of input registers to be read</param>
        /// <returns>Int Array which contains the input registers</returns>
        public InputRegisters ReadInputRegisters(int startingAddress, int quantity)
        {
            Logger.Info("FC4 (Read Input Registers from Master device), StartingAddress: " + startingAddress + ", Quantity: " + quantity);

            if (modbusChannel == null)
            {
                Logger.Error("ConnectionException Throwed");
                throw new InvalidOperationException("No ModbusChannel initialized.");
            }
            if (startingAddress > 65535 || quantity > 125)
            {
                Logger.Error("ArgumentException Throwed");
                throw new ArgumentException("Starting address must be 0 - 65535; quantity must be 0 - 125");
            }
            var request = new ModbusRequest(ModbusFunctions.ReadHoldingRegisters, startingAddress, quantity);
            var error = modbusChannel.SendReadCommand(request, out InputRegisters inputRegs);

            if (error != null) throw ModbusException.FromProtocolException(error.ProtocolException);

            return inputRegs;
        }
    }
}