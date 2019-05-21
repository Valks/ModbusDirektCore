using System;
using System.Collections.Generic;
using System.Text;

namespace ModbusDirekt.Modbus.Protocol
{
    enum ModbusFunctions
    {
        ReadDiscreteInputs = 0x02,
        ReadCoils = 0x01,
        WriteSingleCoil = 0x05,
        WriteMultipleCoils = 0x0F,
        ReadInputRegister = 0x04,
        ReadHoldingRegisters = 0x03,
        WriteSingleRegister = 0x06,
        WriteMultipleRegisters = 0x10,
        ReadWriteMultipleRegisters = 0x17,
        MaskWriteRegister = 0x16,
        ReadFIFOqueue = 0x18,
        ReadFilerecord = 0x14,
        WriteFilerecord = 0x15,
        ReadExceptionstatus = 0x07,
        Diagnostic = 0x08,
        GetComeventcounter = 0x0B,
        GetComEventLog = 0x0C,
        ReportServerID = 0x11,
        ReadDeviceIdentification = 0x2B,
        EncapsulatedInterfaceTransport = 0x2B
    }

    enum ModbusSubFunctionCode
    {
        ReturnQueryData = 0x00,
        RestartCommunicationsOption = 0x01,
        ReturnDiagnosticRegister = 0x02,
        ChangeASCIIInputDelimiter = 0x03,
        ForceListenOnlyMode = 0x04,
        ClearCountersandDiagnosticRegister = 0x0A,
        ReturnBusMessageCount = 0x0B,
        ReturnBusCommunicationErrorCount = 0x0C,
        ReturnBusExceptionErrorCount = 0x0D,
        ReturnServerMessageCount = 0x0E,
        ReturnServerNoResponseCount = 0x0F,
        ReturnServerNAKCount = 0x10,
        ReturnServerBusyCount = 0x11,
        ReturnBusCharacterOverrunCount = 0x12,
        ClearOverrunCounterandFlag = 0x14
    }
}
