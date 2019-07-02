## About

NetworkEngine is a XML-based DSL for describing the format of packets in plaintext.

## Build Status

[![Build status](https://ethanmoffat.visualstudio.com/EndlessClient/_apis/build/status/NetworkEngine%20CI)](https://ethanmoffat.visualstudio.com/EndlessClient/_build/latest?definitionId=8)  ![Azure DevOps tests](https://img.shields.io/azure-devops/tests/ethanmoffat/EndlessClient/8.svg?logo=azure-pipelines)  ![Azure DevOps coverage](https://img.shields.io/azure-devops/coverage/ethanmoffat/EndlessClient/8.svg?logo=azure-pipelines)

## Supported Data Types and Constructs

The DSL is defined as an XSD schema [here](schema/NetworkPacket.xsd)

The DSL supports the following data types:

- Byte (unencoded)
- Char (encoded, one byte)
- Short (two bytes)
- Three (three bytes)
- Int (four bytes)
- Skip (seek over the bytes)
- String (ASCII-encoded string with specified length)
- BreakString (ASCII-encoded string with a terminating `255` byte value)
- EndString (ASCII-encoded string running to the end of the packet)
- Structure (named group of simple types)

Additionally, a `length` parameter can be added to simple data types to specify a fixed-length array of that type.

The following constructs are suppoted:

- Conditions (choose data based on the value of a simple type)
- Loops (groups of structures based on certain parameters)
- Inheritance (simple implementation of deriving packets from other packets)

## Sample Packet

Taken from [here](NetworkEngine.Test/PacketCompiler/Samples/group.xml)

```xml
<?xml version="1.0" encoding="utf-8" ?>
<!-- Sample packet for the ACCOUNT_LOGIN packet response from Endless Online -->
<packet name="AccountLoginCharacters">
  <!--
    Group has
    - a count of type char (number of characters)
    - a delimiter between each entry of type byte that should be value 255
    - the delimiter is *not* peeked but is consumed from the packet stream
  -->
  <group countType="char" breakOn="255" breakType="byte" peek="false" >
    <preLoop>
      <!-- One byte is skipped before starting looping -->
      <byte>SkippedByte</byte>
    </preLoop>
    <structure name="AccountCharacter">
      <breakString>Name</breakString>
      <int>Id</int>
      <char>Level</char>

      <char>Gender</char>
      <char>HairStyle</char>
      <char>HairColor</char>
      <char>Race</char>

      <char>AdminLevel</char>

      <short>Boots</short>
      <short>Armor</short>
      <short>Hat</short>
      <short>Shield</short>
      <short>Weapon</short>
    </structure>
    <postLoop>
      <!-- Final byte is expected to be equal to 255 -->
      <byte>FinalDelimiter</byte>
    </postLoop>
  </group>
</packet>
```

## How to Use

Packets are parsed from the XML into state objects and "compiled" into the appropriate C# source code files.

1. A project takes a dependency on NetworkEngine
1. The project defines one or more "packets" in XML format
1. As a build step, PacketCompiler is invoked
1. PacketCompiler reads the XML and creates an internal state object
1. The internal state object is used to generate an annotated packet source code file in C#
1. When sending: PacketTranslator gets passed a C# state object and converts to a packet for transmission
1. When receiving: PacketTranslator reads a packet and populates a C# state object from the bytes in the packet
