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

        [Test]
        public void GivenInvalidRootElement_WhenParse_ThrowsInvalidPacketSpecException()
        {
            var doc = new XmlDocument();
            doc.LoadXml(@"<?xml version=""1.0"" encoding=""utf-8""?>
<Case />");

            var parser = new PacketSpecParser(doc);
            Assert.That(() => parser.Parse(), Throws.InstanceOf<InvalidPacketSpecException>()
                                                        .With.Property(nameof(InvalidPacketSpecException.Result)).EqualTo(ValidationResult.SchemaError));
        }

        [Test]
        public void GivenInvalidRootElement_WhenParseWithoutSchemaValidation_ThrowsExceptionWithInvalidRootElement()
        {
            var doc = new XmlDocument();
            doc.LoadXml(@"<?xml version=""1.0"" encoding=""utf-8""?>
<Case />");

            var parser = new PacketSpecParser(doc);
            Assert.That(() => parser.Parse(ParseOptions.SkipSchemaValidation),
                        Throws.InstanceOf<InvalidPacketSpecException>().With.Property(nameof(InvalidPacketSpecException.Result))
                                                                            .EqualTo(ValidationResult.InvalidRootElement));
        }

        [Test]
        public void GivenPacketWithBase_WhenParse_ReferencesBasePacket()
        {
            var doc = new XmlDocument();
            doc.Load("PacketCompiler/Samples/derived.xml");

            var expectedBasePacket = doc.SelectSingleNode("/packet/@base").Value;

            var parser = new PacketSpecParser(doc);
            var state = parser.Parse();

            Assert.That(state.BasePacketName, Is.EqualTo(expectedBasePacket));
        }

        private static PacketDataType GetEnum(string name) => Enum.Parse<PacketDataType>(char.ToUpper(name[0]) + name.Substring(1));
    }
}
