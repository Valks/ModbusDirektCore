## ModbusDC

ModbusDC is a communication library for the modbus protocol.

This project was largely inspired by the EasyModbusTCP.NET library. https://github.com/rossmann-engineering/EasyModbusTCP.NET but was rebuilt from scratch to add flexibility
and solve some limitations in the implementation.


## About Modbus

Modbus is a communication protocol for serial devices. It was built to enable communication over long distances over a two wire bus cable using the RS-485 standard.
A traditional Modbus "system" consists of a 'Master' device and up to 32 slave devices along a single BUS cable. The master consisted of a PC with a RS-485
PCIE card, or a RS-485 modem connected to the PC`s Serial port.

When multiple serial devices are connected to a single bus line, the communication needs to be synchronized to prevent two devices transmitting at  the same time.
In modbus this is handled by only allowing the master to initiate communication. The master sends out a Read or Write command containting a Unit Identification number,
and the slave device whith the corresponding id then responds to the message.

Modbus later added a protocol standard to enable communication over TCP/IP which added great flexibility to the modbus topology. Using a Modbus TCP/IP Gateway, 
Masters and slaves no longer have to reside on the same physical network and a single master can communicate with an unlimited number of slaves.

