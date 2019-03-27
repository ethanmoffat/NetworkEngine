// Original Work Copyright (c) Ethan Moffat 2014-2019

using System;
using System.Collections.Generic;
using System.Xml;
using NetworkEngine.PacketCompiler;
using NUnit.Framework;

namespace NetworkEngine.Test.PacketCompiler
{
    [TestFixture]
    public class PacketSpecParserTest
    {
        [Test]
        public void GivenBasicPacket_WhenParse_GetsPacketName()
        {
            var doc = new XmlDocument();
            doc.Load("PacketCompiler/Samples/basic.xml");

            var expectedPacketName = doc.SelectSingleNode("/packet/@name").Value;

            var parser = new PacketSpecParser(doc);
            var state = parser.Parse();

            Assert.That(state.PacketName, Is.EqualTo(expectedPacketName));
        }

        [Test]
        public void GivenBasicPacket_WhenParse_GetsExpectedDataTypesAndNames()
        {
            var doc = new XmlDocument();
            doc.Load("PacketCompiler/Samples/basic.xml");

            var dataTypeNodes = doc.SelectNodes("/packet/*");
            var expectedTypesAndNames = new List<(PacketDataType Type, string Name)>();
            for (int i = 0; i < dataTypeNodes.Count; ++i)
                expectedTypesAndNames.Add((GetEnum(dataTypeNodes[i].Name), dataTypeNodes[i].InnerText));

            var parser = new PacketSpecParser(doc);
            var state = parser.Parse();

            for (int i = 0; i < state.Data.Count; ++i)
            {
                var nextElement = state.Data[i];
                Assert.That(nextElement.DataType, Is.EqualTo(expectedTypesAndNames[i].Type));
            }
        }

        private static PacketDataType GetEnum(string name) => Enum.Parse<PacketDataType>(char.ToUpper(name[0]) + name.Substring(1));
    }
}
