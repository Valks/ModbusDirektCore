/*
 * GNU GENERAL PUBLIC LICENSE Version 3, 29 June 2007
 * http://www.rossmann-engineering.de
 */

using ModbusDirekt.Modbus;
using ModbusDirekt.Modbus.Protocol;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ModbusDirekt
{
    /// <summary>
    /// Modbus TCP Server.
    /// </summary>
    public class ModbusServer
    {
        public event EventHandler<CoilsChangedEventArgs> CoilsChanged;

        public event EventHandler<HoldingRegistersChangedEventArgs> HoldingRegistersChanged;

        public event EventHandler NumberOfConnectedClientsChanged;

        public event EventHandler LogDataChanged;

        public HoldingRegisters holdingRegisters;
        public InputRegisters inputRegisters;
        public Coils coils;
        public DiscreteInputs discreteInputs;

        public bool FunctionCode1Disabled { get; set; }
        public bool FunctionCode2Disabled { get; set; }
        public bool FunctionCode3Disabled { get; set; }
        public bool FunctionCode4Disabled { get; set; }
        public bool FunctionCode5Disabled { get; set; }
        public bool FunctionCode6Disabled { get; set; }
        public bool FunctionCode15Disabled { get; set; }
        public bool FunctionCode16Disabled { get; set; }
        public bool FunctionCode23Disabled { get; set; }
        public bool PortChanged { get; set; }
        private int numberOfConnections = 0;
        private TCPServer tcpHandler;
        private Thread listenerThread;
        private readonly object lockCoils = new object();
        private readonly object lockHoldingRegisters = new object();

        public ModbusServer()
        {
            holdingRegisters = new HoldingRegisters();
            inputRegisters = new InputRegisters();
            coils = new Coils();
            discreteInputs = new DiscreteInputs();
        }

        public void Listen()
        {
            listenerThread = new Thread(ListenerThread);
            listenerThread.Start();
        }

        public void StopListening()
        {
            try
            {
                tcpHandler.Disconnect();
            }
            catch  { }
            listenerThread.Join();
        }

        private void ListenerThread()
        {
            tcpHandler = new TCPServer(Port);
            Logger.Info("ModbusDirekt Server listing for incomming data on port " + Port);
            tcpHandler.dataChanged += new TCPServer.DataChanged(ProcessReceivedData);
            tcpHandler.numberOfClientsChanged += new TCPServer.NumberOfClientsChangedDelegate(NumberOfClientsChanged);
        }

        private void NumberOfClientsChanged()
        {
            numberOfConnections = tcpHandler.NumberOfConnectedClients;
            NumberOfConnectedClientsChanged?.Invoke(this,EventArgs.Empty);
        }

        private readonly object lockProcessReceivedData = new object();

        private void ProcessReceivedData(object networkConnectionParameter)
        {
            lock (lockProcessReceivedData)
            {
                byte[] bytes = new byte[2];
                /*
                Byte[] bytes = new byte[((NetworkConnectionParameter)networkConnectionParameter).bytes.Length];
                Logger.Info("Received Data: " + BitConverter.ToString(bytes));
                NetworkStream stream = ((NetworkConnectionParameter)networkConnectionParameter).stream;
                int portIn = ((NetworkConnectionParameter)networkConnectionParameter).portIn;
                IPAddress ipAddressIn = ((NetworkConnectionParameter)networkConnectionParameter).ipAddressIn;

                Array.Copy(((NetworkConnectionParameter)networkConnectionParameter).bytes, 0, bytes, 0, ((NetworkConnectionParameter)networkConnectionParameter).bytes.Length);
                */
                ModbusProtocol receiveDataThread = new ModbusProtocol();
                ModbusProtocol sendDataThread = new ModbusProtocol();

                try
                {
                    UInt16[] wordData = new UInt16[1];
                    byte[] byteData = new byte[2];
                    receiveDataThread.TimeStamp = DateTime.Now;
                    receiveDataThread.Request = true;
                    //Lese Transaction identifier
                    byteData[1] = bytes[0];
                    byteData[0] = bytes[1];
                    Buffer.BlockCopy(byteData, 0, wordData, 0, 2);
                    receiveDataThread.TransactionIdentifier = wordData[0];

                    //Lese Protocol identifier
                    byteData[1] = bytes[2];
                    byteData[0] = bytes[3];
                    Buffer.BlockCopy(byteData, 0, wordData, 0, 2);
                    receiveDataThread.ProtocolIdentifier = wordData[0];

                    //Lese length
                    byteData[1] = bytes[4];
                    byteData[0] = bytes[5];
                    Buffer.BlockCopy(byteData, 0, wordData, 0, 2);
                    receiveDataThread.Length = wordData[0];

                    //Lese unit identifier
                    receiveDataThread.UnitIdentifier = bytes[6];
                    //Check UnitIdentifier
                    if ((receiveDataThread.UnitIdentifier != this.UnitIdentifier) && (receiveDataThread.UnitIdentifier != 0))
                        return;

                    // Lese function code
                    receiveDataThread.FunctionCode = bytes[7];

                    // Lese starting address
                    byteData[1] = bytes[8];
                    byteData[0] = bytes[9];
                    Buffer.BlockCopy(byteData, 0, wordData, 0, 2);
                    receiveDataThread.StartingAdress = wordData[0];

                    if (receiveDataThread.FunctionCode <= 4)
                    {
                        // Lese quantity
                        byteData[1] = bytes[10];
                        byteData[0] = bytes[11];
                        Buffer.BlockCopy(byteData, 0, wordData, 0, 2);
                        receiveDataThread.Quantity = wordData[0];
                    }
                    if (receiveDataThread.FunctionCode == 5)
                    {
                        receiveDataThread.ReceiveCoilValues = new ushort[1];
                        // Lese Value
                        byteData[1] = bytes[10];
                        byteData[0] = bytes[11];
                        Buffer.BlockCopy(byteData, 0, receiveDataThread.ReceiveCoilValues, 0, 2);
                    }
                    if (receiveDataThread.FunctionCode == 6)
                    {
                        receiveDataThread.ReceiveRegisterValues = new ushort[1];
                        // Lese Value
                        byteData[1] = bytes[10];
                        byteData[0] = bytes[11];
                        Buffer.BlockCopy(byteData, 0, receiveDataThread.ReceiveRegisterValues, 0, 2);
                    }
                    if (receiveDataThread.FunctionCode == 15)
                    {
                        // Lese quantity
                        byteData[1] = bytes[10];
                        byteData[0] = bytes[11];
                        Buffer.BlockCopy(byteData, 0, wordData, 0, 2);
                        receiveDataThread.Quantity = wordData[0];

                        receiveDataThread.ByteCount = bytes[12];

                        if ((receiveDataThread.ByteCount % 2) != 0)
                            receiveDataThread.ReceiveCoilValues = new ushort[receiveDataThread.ByteCount / 2 + 1];
                        else
                            receiveDataThread.ReceiveCoilValues = new ushort[receiveDataThread.ByteCount / 2];
                        // Lese Value
                        Buffer.BlockCopy(bytes, 13, receiveDataThread.ReceiveCoilValues, 0, receiveDataThread.ByteCount);
                    }
                    if (receiveDataThread.FunctionCode == 16)
                    {
                        // Lese quantity
                        byteData[1] = bytes[10];
                        byteData[0] = bytes[11];
                        Buffer.BlockCopy(byteData, 0, wordData, 0, 2);
                        receiveDataThread.Quantity = wordData[0];

                        receiveDataThread.ByteCount = bytes[12];
                        receiveDataThread.ReceiveRegisterValues = new ushort[receiveDataThread.Quantity];
                        for (int i = 0; i < receiveDataThread.Quantity; i++)
                        {
                            // Lese Value
                            byteData[1] = bytes[13 + i * 2];
                            byteData[0] = bytes[14 + i * 2];
                            Buffer.BlockCopy(byteData, 0, receiveDataThread.ReceiveRegisterValues, i * 2, 2);
                        }
                    }
                    if (receiveDataThread.FunctionCode == 23)
                    {
                        // Lese starting Address Read
                        byteData[1] = bytes[8];
                        byteData[0] = bytes[9];
                        Buffer.BlockCopy(byteData, 0, wordData, 0, 2);
                        receiveDataThread.StartingAddressRead = wordData[0];
                        // Lese quantity Read
                        byteData[1] = bytes[10];
                        byteData[0] = bytes[11];
                        Buffer.BlockCopy(byteData, 0, wordData, 0, 2);
                        receiveDataThread.QuantityRead = wordData[0];
                        // Lese starting Address Write
                        byteData[1] = bytes[12];
                        byteData[0] = bytes[13];
                        Buffer.BlockCopy(byteData, 0, wordData, 0, 2);
                        receiveDataThread.StartingAddressWrite = wordData[0];
                        // Lese quantity Write
                        byteData[1] = bytes[14];
                        byteData[0] = bytes[15];
                        Buffer.BlockCopy(byteData, 0, wordData, 0, 2);
                        receiveDataThread.QuantityWrite = wordData[0];

                        receiveDataThread.ByteCount = bytes[16];
                        receiveDataThread.ReceiveRegisterValues = new ushort[receiveDataThread.QuantityWrite];
                        for (int i = 0; i < receiveDataThread.QuantityWrite; i++)
                        {
                            // Lese Value
                            byteData[1] = bytes[17 + i * 2];
                            byteData[0] = bytes[18 + i * 2];
                            Buffer.BlockCopy(byteData, 0, receiveDataThread.ReceiveRegisterValues, i * 2, 2);
                        }
                    }
                }
                catch { }
                NetworkStream stream = null;
                this.CreateAnswer(receiveDataThread, sendDataThread, stream);
                this.CreateLogData(receiveDataThread, sendDataThread);

                LogDataChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void CreateAnswer(ModbusProtocol receiveData, ModbusProtocol sendData, NetworkStream stream)
        {
            switch (receiveData.FunctionCode)
            {
                // Read Coils
                case 1:
                    if (!FunctionCode1Disabled)
                    {
                        this.ReadCoils(receiveData, sendData, stream);
                    }
                    else
                    {
                        sendData.ErrorCode = (byte)(receiveData.FunctionCode + 0x80);
                        sendData.ExceptionCode = 1;
                        SendException(sendData.ErrorCode, sendData.ExceptionCode, receiveData, sendData, stream);
                    }
                    break;
                // Read Input Registers
                case 2:
                    if (!FunctionCode2Disabled)
                    {
                        this.ReadDiscreteInputs(receiveData, sendData, stream);
                    }
                    else
                    {
                        sendData.ErrorCode = (byte)(receiveData.FunctionCode + 0x80);
                        sendData.ExceptionCode = 1;
                        SendException(sendData.ErrorCode, sendData.ExceptionCode, receiveData, sendData, stream);
                    }

                    break;
                // Read Holding Registers
                case 3:
                    if (!FunctionCode3Disabled)
                    {
                        this.ReadHoldingRegisters(receiveData, sendData, stream);
                    }
                    else
                    {
                        sendData.ErrorCode = (byte)(receiveData.FunctionCode + 0x80);
                        sendData.ExceptionCode = 1;
                        SendException(sendData.ErrorCode, sendData.ExceptionCode, receiveData, sendData, stream);
                    }

                    break;
                // Read Input Registers
                case 4:
                    if (!FunctionCode4Disabled)
                    {
                        this.ReadInputRegisters(receiveData, sendData, stream);
                    }
                    else
                    {
                        sendData.ErrorCode = (byte)(receiveData.FunctionCode + 0x80);
                        sendData.ExceptionCode = 1;
                        SendException(sendData.ErrorCode, sendData.ExceptionCode, receiveData, sendData, stream);
                    }

                    break;
                // Write single coil
                case 5:
                    if (!FunctionCode5Disabled)
                    {
                        this.WriteSingleCoil(receiveData, sendData, stream);
                    }
                    else
                    {
                        sendData.ErrorCode = (byte)(receiveData.FunctionCode + 0x80);
                        sendData.ExceptionCode = 1;
                        SendException(sendData.ErrorCode, sendData.ExceptionCode, receiveData, sendData, stream);
                    }

                    break;
                // Write single register
                case 6:
                    if (!FunctionCode6Disabled)
                    {
                        this.WriteSingleRegister(receiveData, sendData, stream);
                    }
                    else
                    {
                        sendData.ErrorCode = (byte)(receiveData.FunctionCode + 0x80);
                        sendData.ExceptionCode = 1;
                        SendException(sendData.ErrorCode, sendData.ExceptionCode, receiveData, sendData, stream);
                    }

                    break;
                // Write Multiple coils
                case 15:
                    if (!FunctionCode15Disabled)
                    {
                        this.WriteMultipleCoils(receiveData, sendData, stream);
                    }
                    else
                    {
                        sendData.ErrorCode = (byte)(receiveData.FunctionCode + 0x80);
                        sendData.ExceptionCode = 1;
                        SendException(sendData.ErrorCode, sendData.ExceptionCode, receiveData, sendData, stream);
                    }

                    break;
                // Write Multiple registers
                case 16:
                    if (!FunctionCode16Disabled)
                    {
                        this.WriteMultipleRegisters(receiveData, sendData, stream);
                    }
                    else
                    {
                        sendData.ErrorCode = (byte)(receiveData.FunctionCode + 0x80);
                        sendData.ExceptionCode = 1;
                        SendException(sendData.ErrorCode, sendData.ExceptionCode, receiveData, sendData, stream);
                    }

                    break;
                // Error: Function Code not supported
                case 23:
                    if (!FunctionCode23Disabled)
                    {
                        this.ReadWriteMultipleRegisters(receiveData, sendData, stream);
                    }
                    else
                    {
                        sendData.ErrorCode = (byte)(receiveData.FunctionCode + 0x80);
                        sendData.ExceptionCode = 1;
                        SendException(sendData.ErrorCode, sendData.ExceptionCode, receiveData, sendData, stream);
                    }

                    break;
                // Error: Function Code not supported
                default:
                    sendData.ErrorCode = (byte)(receiveData.FunctionCode + 0x80);
                    sendData.ExceptionCode = 1;
                    SendException(sendData.ErrorCode, sendData.ExceptionCode, receiveData, sendData, stream);
                    break;
            }
            sendData.TimeStamp = DateTime.Now;
        }

        private void ReadCoils(ModbusProtocol receiveData, ModbusProtocol sendData, NetworkStream stream)
        {
            sendData.Response = true;

            sendData.TransactionIdentifier = receiveData.TransactionIdentifier;
            sendData.ProtocolIdentifier = receiveData.ProtocolIdentifier;

            sendData.UnitIdentifier = this.UnitIdentifier;
            sendData.FunctionCode = receiveData.FunctionCode;
            if ((receiveData.Quantity < 1) || (receiveData.Quantity > 0x07D0))  //Invalid quantity
            {
                sendData.ErrorCode = (byte)(receiveData.FunctionCode + 0x80);
                sendData.ExceptionCode = 3;
            }
            if ((receiveData.StartingAdress + 1 + receiveData.Quantity) > 65_535)     //Invalid Starting adress or Starting address + quantity
            {
                sendData.ErrorCode = (byte)(receiveData.FunctionCode + 0x80);
                sendData.ExceptionCode = 2;
            }
            if (sendData.ExceptionCode == 0)
            {
                if ((receiveData.Quantity % 8) == 0)
                    sendData.ByteCount = (byte)(receiveData.Quantity / 8);
                else
                    sendData.ByteCount = (byte)(receiveData.Quantity / 8 + 1);

                sendData.SendCoilValues = new bool[receiveData.Quantity];
                lock (lockCoils)
                    Array.Copy(coils.Data, receiveData.StartingAdress + 1, sendData.SendCoilValues, 0, receiveData.Quantity);
            }
            if (true)
            {
                Byte[] data;

                if (sendData.ExceptionCode > 0)
                    data = new byte[9];
                else
                    data = new byte[9];

                Byte[] byteData = new byte[2];

                sendData.Length = (byte)(data.Length - 6);

                //Send Transaction identifier
                byteData = BitConverter.GetBytes((int)sendData.TransactionIdentifier);
                data[0] = byteData[1];
                data[1] = byteData[0];

                //Send Protocol identifier
                byteData = BitConverter.GetBytes((int)sendData.ProtocolIdentifier);
                data[2] = byteData[1];
                data[3] = byteData[0];

                //Send length
                byteData = BitConverter.GetBytes((int)sendData.Length);
                data[4] = byteData[1];
                data[5] = byteData[0];
                //Unit Identifier
                data[6] = sendData.UnitIdentifier;

                //Function Code
                data[7] = sendData.FunctionCode;

                //ByteCount
                data[8] = sendData.ByteCount;

                if (sendData.ExceptionCode > 0)
                {
                    data[7] = sendData.ErrorCode;
                    data[8] = sendData.ExceptionCode;
                    sendData.SendCoilValues = null;
                }

                if (sendData.SendCoilValues != null)
                {
                    for (int i = 0; i < (sendData.ByteCount); i++)
                    {
                        byteData = new byte[2];
                        for (int j = 0; j < 8; j++)
                        {
                            byte boolValue;
                            if (sendData.SendCoilValues[i * 8 + j] == true)
                                boolValue = 1;
                            else
                                boolValue = 0;
                            byteData[1] = (byte)((byteData[1]) | (boolValue << j));
                            if ((i * 8 + j + 1) >= sendData.SendCoilValues.Length)
                                break;
                        }
                        data[9 + i] = byteData[1];
                    }
                }

                try
                {
                    stream.Write(data, 0, data.Length);
                    Logger.Info("Send Data: " + BitConverter.ToString(data));
                }
                catch { }
            }
        }

        private void ReadDiscreteInputs(ModbusProtocol receiveData, ModbusProtocol sendData, NetworkStream stream)
        {
            sendData.Response = true;

            sendData.TransactionIdentifier = receiveData.TransactionIdentifier;
            sendData.ProtocolIdentifier = receiveData.ProtocolIdentifier;

            sendData.UnitIdentifier = this.UnitIdentifier;
            sendData.FunctionCode = receiveData.FunctionCode;
            if ((receiveData.Quantity < 1) || (receiveData.Quantity > 0x07D0))  //Invalid quantity
            {
                sendData.ErrorCode = (byte)(receiveData.FunctionCode + 0x80);
                sendData.ExceptionCode = 3;
            }
            if ((receiveData.StartingAdress + 1 + receiveData.Quantity) > 65535)   //Invalid Starting adress or Starting address + quantity
            {
                sendData.ErrorCode = (byte)(receiveData.FunctionCode + 0x80);
                sendData.ExceptionCode = 2;
            }
            if (sendData.ExceptionCode == 0)
            {
                if ((receiveData.Quantity % 8) == 0)
                    sendData.ByteCount = (byte)(receiveData.Quantity / 8);
                else
                    sendData.ByteCount = (byte)(receiveData.Quantity / 8 + 1);

                sendData.SendCoilValues = new bool[receiveData.Quantity];
                Array.Copy(null, receiveData.StartingAdress + 1, sendData.SendCoilValues, 0, receiveData.Quantity);
            }
            if (true)
            {
                Byte[] data;
                if (sendData.ExceptionCode > 0)
                    data = new byte[9];
                else
                    data = new byte[9 + sendData.ByteCount];
                Byte[] byteData = new byte[2];
                sendData.Length = (byte)(data.Length - 6);

                //Send Transaction identifier
                byteData = BitConverter.GetBytes((int)sendData.TransactionIdentifier);
                data[0] = byteData[1];
                data[1] = byteData[0];

                //Send Protocol identifier
                byteData = BitConverter.GetBytes((int)sendData.ProtocolIdentifier);
                data[2] = byteData[1];
                data[3] = byteData[0];

                //Send length
                byteData = BitConverter.GetBytes((int)sendData.Length);
                data[4] = byteData[1];
                data[5] = byteData[0];

                //Unit Identifier
                data[6] = sendData.UnitIdentifier;

                //Function Code
                data[7] = sendData.FunctionCode;

                //ByteCount
                data[8] = sendData.ByteCount;

                if (sendData.ExceptionCode > 0)
                {
                    data[7] = sendData.ErrorCode;
                    data[8] = sendData.ExceptionCode;
                    sendData.SendCoilValues = null;
                }

                if (sendData.SendCoilValues != null)
                {
                    for (int i = 0; i < (sendData.ByteCount); i++)
                    {
                        byteData = new byte[2];
                        for (int j = 0; j < 8; j++)
                        {
                            byte boolValue;
                            if (sendData.SendCoilValues[i * 8 + j] == true)
                                boolValue = 1;
                            else
                                boolValue = 0;
                            byteData[1] = (byte)((byteData[1]) | (boolValue << j));
                            if ((i * 8 + j + 1) >= sendData.SendCoilValues.Length)
                                break;
                        }
                        data[9 + i] = byteData[1];
                    }
                }

                try
                {
                    stream.Write(data, 0, data.Length);
                    Logger.Info("Send Data: " + BitConverter.ToString(data));
                }
                catch { }
            }
        }

        private void ReadHoldingRegisters(ModbusProtocol receiveData, ModbusProtocol sendData, NetworkStream stream)
        {
            sendData.Response = true;

            sendData.TransactionIdentifier = receiveData.TransactionIdentifier;
            sendData.ProtocolIdentifier = receiveData.ProtocolIdentifier;

            sendData.UnitIdentifier = this.UnitIdentifier;
            sendData.FunctionCode = receiveData.FunctionCode;
            if ((receiveData.Quantity < 1) || (receiveData.Quantity > 0x007D))  //Invalid quantity
            {
                sendData.ErrorCode = (byte)(receiveData.FunctionCode + 0x80);
                sendData.ExceptionCode = 3;
            }
            if ((receiveData.StartingAdress + 1 + receiveData.Quantity) > 65535)   //Invalid Starting adress or Starting address + quantity
            {
                sendData.ErrorCode = (byte)(receiveData.FunctionCode + 0x80);
                sendData.ExceptionCode = 2;
            }
            if (sendData.ExceptionCode == 0)
            {
                sendData.ByteCount = (byte)(2 * receiveData.Quantity);
                sendData.SendRegisterValues = new Int16[receiveData.Quantity];
                lock (lockHoldingRegisters)
                    Buffer.BlockCopy(holdingRegisters.localArray, receiveData.StartingAdress * 2 + 2, sendData.SendRegisterValues, 0, receiveData.Quantity * 2);
            }
            if (sendData.ExceptionCode > 0)
                sendData.Length = 0x03;
            else
                sendData.Length = (ushort)(0x03 + sendData.ByteCount);

            byte[] data = sendData.ExceptionCode > 0 ? new byte[9] : new byte[9 + sendData.ByteCount];

                byte[] byteData = new byte[2];
                sendData.Length = (byte)(data.Length - 6);

                //Send Transaction identifier
                byteData = BitConverter.GetBytes((int)sendData.TransactionIdentifier);
                data[0] = byteData[1];
                data[1] = byteData[0];

                //Send Protocol identifier
                byteData = BitConverter.GetBytes((int)sendData.ProtocolIdentifier);
                data[2] = byteData[1];
                data[3] = byteData[0];

                //Send length
                byteData = BitConverter.GetBytes((int)sendData.Length);
                data[4] = byteData[1];
                data[5] = byteData[0];

                //Unit Identifier
                data[6] = sendData.UnitIdentifier;

                //Function Code
                data[7] = sendData.FunctionCode;

                //ByteCount
                data[8] = sendData.ByteCount;

                if (sendData.ExceptionCode > 0)
                {
                    data[7] = sendData.ErrorCode;
                    data[8] = sendData.ExceptionCode;
                    sendData.SendRegisterValues = null;
                }

                if (sendData.SendRegisterValues != null)
                {
                    for (int i = 0; i < (sendData.ByteCount / 2); i++)
                    {
                        byteData = BitConverter.GetBytes((Int16)sendData.SendRegisterValues[i]);
                        data[9 + i * 2] = byteData[1];
                        data[10 + i * 2] = byteData[0];
                    }
                }

                try
                {
                    stream.Write(data, 0, data.Length);
                    Logger.Info("Send Data: " + BitConverter.ToString(data));
                }
                catch { }
        }

        private void ReadInputRegisters(ModbusProtocol receiveData, ModbusProtocol sendData, NetworkStream stream)
        {
            sendData.Response = true;

            sendData.TransactionIdentifier = receiveData.TransactionIdentifier;
            sendData.ProtocolIdentifier = receiveData.ProtocolIdentifier;

            sendData.UnitIdentifier = this.UnitIdentifier;
            sendData.FunctionCode = receiveData.FunctionCode;
            if ((receiveData.Quantity < 1) || (receiveData.Quantity > 0x007D))  //Invalid quantity
            {
                sendData.ErrorCode = (byte)(receiveData.FunctionCode + 0x80);
                sendData.ExceptionCode = 3;
            }
            if ((receiveData.StartingAdress + 1 + receiveData.Quantity) > 65535)   //Invalid Starting adress or Starting address + quantity
            {
                sendData.ErrorCode = (byte)(receiveData.FunctionCode + 0x80);
                sendData.ExceptionCode = 2;
            }
            if (sendData.ExceptionCode == 0)
            {
                sendData.ByteCount = (byte)(2 * receiveData.Quantity);
                sendData.SendRegisterValues = new Int16[receiveData.Quantity];
                Buffer.BlockCopy(inputRegisters.Data, receiveData.StartingAdress * 2 + 2, sendData.SendRegisterValues, 0, receiveData.Quantity * 2);
            }
            if (sendData.ExceptionCode > 0)
                sendData.Length = 0x03;
            else
                sendData.Length = (ushort)(0x03 + sendData.ByteCount);

            if (true)
            {
                Byte[] data;
                if (sendData.ExceptionCode > 0)
                    data = new byte[9];
                else
                    data = new byte[9 + sendData.ByteCount];
                Byte[] byteData = new byte[2];
                sendData.Length = (byte)(data.Length - 6);

                //Send Transaction identifier
                byteData = BitConverter.GetBytes((int)sendData.TransactionIdentifier);
                data[0] = byteData[1];
                data[1] = byteData[0];

                //Send Protocol identifier
                byteData = BitConverter.GetBytes((int)sendData.ProtocolIdentifier);
                data[2] = byteData[1];
                data[3] = byteData[0];

                //Send length
                byteData = BitConverter.GetBytes((int)sendData.Length);
                data[4] = byteData[1];
                data[5] = byteData[0];

                //Unit Identifier
                data[6] = sendData.UnitIdentifier;

                //Function Code
                data[7] = sendData.FunctionCode;

                //ByteCount
                data[8] = sendData.ByteCount;

                if (sendData.ExceptionCode > 0)
                {
                    data[7] = sendData.ErrorCode;
                    data[8] = sendData.ExceptionCode;
                    sendData.SendRegisterValues = null;
                }

                if (sendData.SendRegisterValues != null)
                {
                    for (int i = 0; i < (sendData.ByteCount / 2); i++)
                    {
                        byteData = BitConverter.GetBytes((Int16)sendData.SendRegisterValues[i]);
                        data[9 + i * 2] = byteData[1];
                        data[10 + i * 2] = byteData[0];
                    }
                }

                try
                {
                    stream.Write(data, 0, data.Length);
                    Logger.Info("Send Data: " + BitConverter.ToString(data));
                }
                catch { }
            }
        }

        private void WriteSingleCoil(ModbusProtocol receiveData, ModbusProtocol sendData, NetworkStream stream)
        {
            sendData.Response = true;

            sendData.TransactionIdentifier = receiveData.TransactionIdentifier;
            sendData.ProtocolIdentifier = receiveData.ProtocolIdentifier;

            sendData.UnitIdentifier = this.UnitIdentifier;
            sendData.FunctionCode = receiveData.FunctionCode;
            sendData.StartingAdress = receiveData.StartingAdress;
            sendData.ReceiveCoilValues = receiveData.ReceiveCoilValues;
            if ((receiveData.ReceiveCoilValues[0] != 0x0000) && (receiveData.ReceiveCoilValues[0] != 0xFF00))  //Invalid Value
            {
                sendData.ErrorCode = (byte)(receiveData.FunctionCode + 0x80);
                sendData.ExceptionCode = 3;
            }
            if ((receiveData.StartingAdress + 1) > 65535)    //Invalid Starting adress or Starting address + quantity
            {
                sendData.ErrorCode = (byte)(receiveData.FunctionCode + 0x80);
                sendData.ExceptionCode = 2;
            }
            if (sendData.ExceptionCode == 0)
            {
                if (receiveData.ReceiveCoilValues[0] == 0xFF00)
                {
                    lock (lockCoils)
                        coils[receiveData.StartingAdress + 1] = true;
                }
                if (receiveData.ReceiveCoilValues[0] == 0x0000)
                {
                    lock (lockCoils)
                        coils[receiveData.StartingAdress + 1] = false;
                }
            }
            if (sendData.ExceptionCode > 0)
                sendData.Length = 0x03;
            else
                sendData.Length = 0x06;

            if (true)
            {
                Byte[] data;
                if (sendData.ExceptionCode > 0)
                    data = new byte[9];
                else
                    data = new byte[12];

                Byte[] byteData = new byte[2];
                sendData.Length = (byte)(data.Length - 6);

                //Send Transaction identifier
                byteData = BitConverter.GetBytes((int)sendData.TransactionIdentifier);
                data[0] = byteData[1];
                data[1] = byteData[0];

                //Send Protocol identifier
                byteData = BitConverter.GetBytes((int)sendData.ProtocolIdentifier);
                data[2] = byteData[1];
                data[3] = byteData[0];

                //Send length
                byteData = BitConverter.GetBytes((int)sendData.Length);
                data[4] = byteData[1];
                data[5] = byteData[0];

                //Unit Identifier
                data[6] = sendData.UnitIdentifier;

                //Function Code
                data[7] = sendData.FunctionCode;

                if (sendData.ExceptionCode > 0)
                {
                    data[7] = sendData.ErrorCode;
                    data[8] = sendData.ExceptionCode;
                    sendData.SendRegisterValues = null;
                }
                else
                {
                    byteData = BitConverter.GetBytes((int)receiveData.StartingAdress);
                    data[8] = byteData[1];
                    data[9] = byteData[0];
                    byteData = BitConverter.GetBytes((int)receiveData.ReceiveCoilValues[0]);
                    data[10] = byteData[1];
                    data[11] = byteData[0];
                }

                try
                {
                    stream.Write(data, 0, data.Length);
                    Logger.Info("Send Data: " + BitConverter.ToString(data));
                }
                catch { }

                CoilsChanged?.Invoke(this, new CoilsChangedEventArgs(receiveData.StartingAdress + 1, 1));
            }
        }

        private void WriteSingleRegister(ModbusProtocol receiveData, ModbusProtocol sendData, NetworkStream stream)
        {
            sendData.Response = true;

            sendData.TransactionIdentifier = receiveData.TransactionIdentifier;
            sendData.ProtocolIdentifier = receiveData.ProtocolIdentifier;

            sendData.UnitIdentifier = this.UnitIdentifier;
            sendData.FunctionCode = receiveData.FunctionCode;
            sendData.StartingAdress = receiveData.StartingAdress;
            sendData.ReceiveRegisterValues = receiveData.ReceiveRegisterValues;

            if ((receiveData.ReceiveRegisterValues[0] < 0x0000) || (receiveData.ReceiveRegisterValues[0] > 0xFFFF))  //Invalid Value
            {
                sendData.ErrorCode = (byte)(receiveData.FunctionCode + 0x80);
                sendData.ExceptionCode = 3;
            }
            if ((receiveData.StartingAdress + 1) > 65535)    //Invalid Starting adress or Starting address + quantity
            {
                sendData.ErrorCode = (byte)(receiveData.FunctionCode + 0x80);
                sendData.ExceptionCode = 2;
            }
            if (sendData.ExceptionCode == 0)
            {
                lock (lockHoldingRegisters)
                    holdingRegisters[receiveData.StartingAdress + 1] = unchecked((short)receiveData.ReceiveRegisterValues[0]);
            }
            if (sendData.ExceptionCode > 0)
                sendData.Length = 0x03;
            else
                sendData.Length = 0x06;

            if (true)
            {
                Byte[] data;
                if (sendData.ExceptionCode > 0)
                    data = new byte[9];
                else
                    data = new byte[12];

                Byte[] byteData = new byte[2];
                sendData.Length = (byte)(data.Length - 6);

                //Send Transaction identifier
                byteData = BitConverter.GetBytes((int)sendData.TransactionIdentifier);
                data[0] = byteData[1];
                data[1] = byteData[0];

                //Send Protocol identifier
                byteData = BitConverter.GetBytes((int)sendData.ProtocolIdentifier);
                data[2] = byteData[1];
                data[3] = byteData[0];

                //Send length
                byteData = BitConverter.GetBytes((int)sendData.Length);
                data[4] = byteData[1];
                data[5] = byteData[0];

                //Unit Identifier
                data[6] = sendData.UnitIdentifier;

                //Function Code
                data[7] = sendData.FunctionCode;

                if (sendData.ExceptionCode > 0)
                {
                    data[7] = sendData.ErrorCode;
                    data[8] = sendData.ExceptionCode;
                    sendData.SendRegisterValues = null;
                }
                else
                {
                    byteData = BitConverter.GetBytes((int)receiveData.StartingAdress);
                    data[8] = byteData[1];
                    data[9] = byteData[0];
                    byteData = BitConverter.GetBytes((int)receiveData.ReceiveRegisterValues[0]);
                    data[10] = byteData[1];
                    data[11] = byteData[0];
                }

                try
                {
                    stream.Write(data, 0, data.Length);
                    Logger.Info("Send Data: " + BitConverter.ToString(data));
                }
                catch { }
                HoldingRegistersChanged?.Invoke(this, new HoldingRegistersChangedEventArgs( receiveData.StartingAdress + 1, 1));
            }
        }

        private void WriteMultipleCoils(ModbusProtocol receiveData, ModbusProtocol sendData, NetworkStream stream)
        {
            sendData.Response = true;

            sendData.TransactionIdentifier = receiveData.TransactionIdentifier;
            sendData.ProtocolIdentifier = receiveData.ProtocolIdentifier;

            sendData.UnitIdentifier = this.UnitIdentifier;
            sendData.FunctionCode = receiveData.FunctionCode;
            sendData.StartingAdress = receiveData.StartingAdress;
            sendData.Quantity = receiveData.Quantity;

            if ((receiveData.Quantity == 0x0000) || (receiveData.Quantity > 0x07B0))  //Invalid Quantity
            {
                sendData.ErrorCode = (byte)(receiveData.FunctionCode + 0x80);
                sendData.ExceptionCode = 3;
            }
            if ((int)receiveData.StartingAdress + 1 + receiveData.Quantity > 65535)    //Invalid Starting adress or Starting address + quantity
            {
                sendData.ErrorCode = (byte)(receiveData.FunctionCode + 0x80);
                sendData.ExceptionCode = 2;
            }

            if (sendData.ExceptionCode == 0)
            {
                lock (lockCoils)
                {
                    for (int i = 0; i < receiveData.Quantity; i++)
                    {
                        int shift = i % 16;

                        int mask = 0x1;
                        mask = mask << shift;

                        if ((receiveData.ReceiveCoilValues[i / 16] & (ushort)mask) == 0)
                            coils[receiveData.StartingAdress + i + 1] = false;
                        else
                            coils[receiveData.StartingAdress + i + 1] = true;
                    }
                }
            }

            sendData.Length = sendData.ExceptionCode > 0 ? (ushort)0x03 : (ushort)0x06;

                byte[] data;
                if (sendData.ExceptionCode > 0)
                    data = new byte[9];
                else
                    data = new byte[12];

                byte[] byteData = new byte[2];
                sendData.Length = (byte)(data.Length - 6);

                //Send Transaction identifier
                byteData = BitConverter.GetBytes((int)sendData.TransactionIdentifier);
                data[0] = byteData[1];
                data[1] = byteData[0];

                //Send Protocol identifier
                byteData = BitConverter.GetBytes((int)sendData.ProtocolIdentifier);
                data[2] = byteData[1];
                data[3] = byteData[0];

                //Send length
                byteData = BitConverter.GetBytes((int)sendData.Length);
                data[4] = byteData[1];
                data[5] = byteData[0];

                //Unit Identifier
                data[6] = sendData.UnitIdentifier;

                //Function Code
                data[7] = sendData.FunctionCode;

                if (sendData.ExceptionCode > 0)
                {
                    data[7] = sendData.ErrorCode;
                    data[8] = sendData.ExceptionCode;
                    sendData.SendRegisterValues = null;
                }
                else
                {
                    byteData = BitConverter.GetBytes((int)receiveData.StartingAdress);
                    data[8] = byteData[1];
                    data[9] = byteData[0];
                    byteData = BitConverter.GetBytes((int)receiveData.Quantity);
                    data[10] = byteData[1];
                    data[11] = byteData[0];
                }

                try
                {
                    stream.Write(data, 0, data.Length);
                    Logger.Info("Send Data: " + BitConverter.ToString(data));
                }
                catch { }

                CoilsChanged?.Invoke(this, new CoilsChangedEventArgs( receiveData.StartingAdress + 1, receiveData.Quantity));
        }

        private void WriteMultipleRegisters(ModbusProtocol receiveData, ModbusProtocol sendData, NetworkStream stream)
        {
            sendData.Response = true;

            sendData.TransactionIdentifier = receiveData.TransactionIdentifier;
            sendData.ProtocolIdentifier = receiveData.ProtocolIdentifier;

            sendData.UnitIdentifier = this.UnitIdentifier;
            sendData.FunctionCode = receiveData.FunctionCode;
            sendData.StartingAdress = receiveData.StartingAdress;
            sendData.Quantity = receiveData.Quantity;

            if ((receiveData.Quantity == 0x0000) || (receiveData.Quantity > 0x07B0))  //Invalid Quantity
            {
                sendData.ErrorCode = (byte)(receiveData.FunctionCode + 0x80);
                sendData.ExceptionCode = 3;
            }
            if ((((int)receiveData.StartingAdress + 1 + receiveData.Quantity) > 65535) )   //Invalid Starting adress or Starting address + quantity
            {
                sendData.ErrorCode = (byte)(receiveData.FunctionCode + 0x80);
                sendData.ExceptionCode = 2;
            }
            if (sendData.ExceptionCode == 0)
            {
                lock (lockHoldingRegisters)
                {
                    for (int i = 0; i < receiveData.Quantity; i++)
                    {
                        holdingRegisters[receiveData.StartingAdress + i + 1] = unchecked((short)receiveData.ReceiveRegisterValues[i]);
                    }
                }
            }
            if (sendData.ExceptionCode > 0)
                sendData.Length = 0x03;
            else
                sendData.Length = 0x06;
            if (true)
            {
                Byte[] data;
                if (sendData.ExceptionCode > 0)
                    data = new byte[9];
                else
                    data = new byte[12];

                Byte[] byteData = new byte[2];
                sendData.Length = (byte)(data.Length - 6);

                //Send Transaction identifier
                byteData = BitConverter.GetBytes((int)sendData.TransactionIdentifier);
                data[0] = byteData[1];
                data[1] = byteData[0];

                //Send Protocol identifier
                byteData = BitConverter.GetBytes((int)sendData.ProtocolIdentifier);
                data[2] = byteData[1];
                data[3] = byteData[0];

                //Send length
                byteData = BitConverter.GetBytes((int)sendData.Length);
                data[4] = byteData[1];
                data[5] = byteData[0];

                //Unit Identifier
                data[6] = sendData.UnitIdentifier;

                //Function Code
                data[7] = sendData.FunctionCode;

                if (sendData.ExceptionCode > 0)
                {
                    data[7] = sendData.ErrorCode;
                    data[8] = sendData.ExceptionCode;
                    sendData.SendRegisterValues = null;
                }
                else
                {
                    byteData = BitConverter.GetBytes((int)receiveData.StartingAdress);
                    data[8] = byteData[1];
                    data[9] = byteData[0];
                    byteData = BitConverter.GetBytes((int)receiveData.Quantity);
                    data[10] = byteData[1];
                    data[11] = byteData[0];
                }

                try
                {
                    stream.Write(data, 0, data.Length);
                    Logger.Info("Send Data: " + BitConverter.ToString(data));
                }
                catch { }
                HoldingRegistersChanged?.Invoke(this, new HoldingRegistersChangedEventArgs( receiveData.StartingAdress + 1, receiveData.Quantity));
            }
        }

        private void ReadWriteMultipleRegisters(ModbusProtocol receiveData, ModbusProtocol sendData, NetworkStream stream)
        {
            sendData.Response = true;

            sendData.TransactionIdentifier = receiveData.TransactionIdentifier;
            sendData.ProtocolIdentifier = receiveData.ProtocolIdentifier;

            sendData.UnitIdentifier = this.UnitIdentifier;
            sendData.FunctionCode = receiveData.FunctionCode;

            if ((receiveData.QuantityRead < 0x0001) || (receiveData.QuantityRead > 0x007D) || (receiveData.QuantityWrite < 0x0001) || (receiveData.QuantityWrite > 0x0079) || (receiveData.ByteCount != (receiveData.QuantityWrite * 2)))  //Invalid Quantity
            {
                sendData.ErrorCode = (byte)(receiveData.FunctionCode + 0x80);
                sendData.ExceptionCode = 3;
            }
            if ((((int)receiveData.StartingAddressRead + 1 + (int)receiveData.QuantityRead) > 65535) || (((int)receiveData.StartingAddressWrite + 1 + receiveData.QuantityWrite) > 65535) )    //Invalid Starting adress or Starting address + quantity
            {
                sendData.ErrorCode = (byte)(receiveData.FunctionCode + 0x80);
                sendData.ExceptionCode = 2;
            }
            if (sendData.ExceptionCode == 0)
            {
                sendData.SendRegisterValues = new Int16[receiveData.QuantityRead];
                lock (lockHoldingRegisters)
                    Buffer.BlockCopy(holdingRegisters.localArray, receiveData.StartingAddressRead * 2 + 2, sendData.SendRegisterValues, 0, receiveData.QuantityRead * 2);

                lock (holdingRegisters)
                {
                    for (int i = 0; i < receiveData.QuantityWrite; i++)
                    {
                        holdingRegisters[receiveData.StartingAddressWrite + i + 1] = unchecked((short)receiveData.ReceiveRegisterValues[i]);
                    }
                }

                sendData.ByteCount = (byte)(2 * receiveData.QuantityRead);
            }
            if (sendData.ExceptionCode > 0)
                sendData.Length = 0x03;
            else
                sendData.Length = Convert.ToUInt16(3 + 2 * receiveData.QuantityRead);
            if (true)
            {
                Byte[] data;
                if (sendData.ExceptionCode > 0)
                    data = new byte[9];
                else
                    data = new byte[9 + sendData.ByteCount];

                Byte[] byteData = new byte[2];

                //Send Transaction identifier
                byteData = BitConverter.GetBytes((int)sendData.TransactionIdentifier);
                data[0] = byteData[1];
                data[1] = byteData[0];

                //Send Protocol identifier
                byteData = BitConverter.GetBytes((int)sendData.ProtocolIdentifier);
                data[2] = byteData[1];
                data[3] = byteData[0];

                //Send length
                byteData = BitConverter.GetBytes((int)sendData.Length);
                data[4] = byteData[1];
                data[5] = byteData[0];

                //Unit Identifier
                data[6] = sendData.UnitIdentifier;

                //Function Code
                data[7] = sendData.FunctionCode;

                //ByteCount
                data[8] = sendData.ByteCount;

                if (sendData.ExceptionCode > 0)
                {
                    data[7] = sendData.ErrorCode;
                    data[8] = sendData.ExceptionCode;
                    sendData.SendRegisterValues = null;
                }
                else
                {
                    if (sendData.SendRegisterValues != null)
                    {
                        for (int i = 0; i < (sendData.ByteCount / 2); i++)
                        {
                            byteData = BitConverter.GetBytes((Int16)sendData.SendRegisterValues[i]);
                            data[9 + i * 2] = byteData[1];
                            data[10 + i * 2] = byteData[0];
                        }
                    }
                }

                try
                {
                    stream.Write(data, 0, data.Length);
                    Logger.Info("Send Data: " + BitConverter.ToString(data));
                }
                catch { }
                HoldingRegistersChanged?.Invoke(this, new HoldingRegistersChangedEventArgs( receiveData.StartingAddressWrite + 1, receiveData.QuantityWrite));
            }
        }

        private void SendException(int errorCode, int exceptionCode, ModbusProtocol receiveData, ModbusProtocol sendData, NetworkStream stream)
        {
            sendData.Response = true;

            sendData.TransactionIdentifier = receiveData.TransactionIdentifier;
            sendData.ProtocolIdentifier = receiveData.ProtocolIdentifier;

            sendData.UnitIdentifier = receiveData.UnitIdentifier;
            sendData.ErrorCode = (byte)errorCode;
            sendData.ExceptionCode = (byte)exceptionCode;

            if (sendData.ExceptionCode > 0)
                sendData.Length = 0x03;
            else
                sendData.Length = (ushort)(0x03 + sendData.ByteCount);

            if (true)
            {
                Byte[] data;
                if (sendData.ExceptionCode > 0)
                    data = new byte[9];
                else
                    data = new byte[9 + sendData.ByteCount];
                Byte[] byteData = new byte[2];
                sendData.Length = (byte)(data.Length - 6);

                //Send Transaction identifier
                byteData = BitConverter.GetBytes((int)sendData.TransactionIdentifier);
                data[0] = byteData[1];
                data[1] = byteData[0];

                //Send Protocol identifier
                byteData = BitConverter.GetBytes((int)sendData.ProtocolIdentifier);
                data[2] = byteData[1];
                data[3] = byteData[0];

                //Send length
                byteData = BitConverter.GetBytes((int)sendData.Length);
                data[4] = byteData[1];
                data[5] = byteData[0];

                //Unit Identifier
                data[6] = sendData.UnitIdentifier;

                data[7] = sendData.ErrorCode;
                data[8] = sendData.ExceptionCode;

                try
                {
                    stream.Write(data, 0, data.Length);
                    Logger.Info("Send Data: " + BitConverter.ToString(data));
                }
                catch { }
            }
        }

        private void CreateLogData(ModbusProtocol receiveData, ModbusProtocol sendData)
        {
            for (int i = 0; i < 98; i++)
            {
                ModbusLogData[99 - i] = ModbusLogData[99 - i - 2];
            }
            ModbusLogData[0] = receiveData;
            ModbusLogData[1] = sendData;
        }

        public int NumberOfConnections
        {
            get
            {
                return numberOfConnections;
            }
        }

        public ModbusProtocol[] ModbusLogData { get; } = new ModbusProtocol[100];

        public int Port { get; set; } = 502;

        public byte UnitIdentifier { get; set; } = 1;
    }
}