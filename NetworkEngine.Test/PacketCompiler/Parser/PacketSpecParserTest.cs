// Original Work Copyright (c) Ethan Moffat 2014-2019

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml;
using NetworkEngine.PacketCompiler;
using NetworkEngine.PacketCompiler.Parser;
using NetworkEngine.PacketCompiler.State;
using NUnit.Framework;

namespace NetworkEngine.Test.PacketCompiler.Parser
{
    [TestFixture]
    [ExcludeFromCodeCoverage]
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

                if (dataTypeNodes[i].Attributes.Count > 0 && dataTypeNodes[i].Attributes[0].Name.Equals("length"))
                {
                    Assert.That(nextElement.Length, Is.EqualTo(int.Parse(dataTypeNodes[i].Attributes[0].Value)));
                }
            }
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

        [Test]
        public void GivenPacketWithStructure_WhenParse_StructureIsParsed()
        {
            var doc = new XmlDocument();
            doc.Load("PacketCompiler/Samples/structure.xml");

            var structureNode = doc.SelectSingleNode("/packet/structure");
            var expectedStructureName = structureNode.Attributes["name"].Value;
            var expectedTypesAndNames = new List<(PacketDataType Type, string Name)>();
            for (int i = 0; i < structureNode.ChildNodes.Count; ++i)
                expectedTypesAndNames.Add((GetEnum(structureNode.ChildNodes[i].Name), structureNode.ChildNodes[i].InnerText));

            var parser = new PacketSpecParser(doc);
            var state = parser.Parse();

            Assert.That(state.Data, Has.All.With.Property(nameof(PacketDataElement.DataType)).EqualTo(PacketDataType.Structure));

            var structureToValidate = state.Data.Single();
            Assert.That(structureToValidate.Name, Is.EqualTo(expectedStructureName));

            var members = structureToValidate.MemberState.Members;
            for (int i = 0; i < members.Count; ++i)
            {
                var nextElement = members[i];
                Assert.That(nextElement.DataType, Is.EqualTo(expectedTypesAndNames[i].Type));
                Assert.That(nextElement.Name, Is.EqualTo(expectedTypesAndNames[i].Name));
            }
        }

        [Test]
        public void GivenPacketWithCondition_WhenParse_ConditionIsParsed()
        {
            var doc = new XmlDocument();
            doc.Load("PacketCompiler/Samples/condition.xml");

            var expectedFirstConditionPeek = Convert.ToBoolean(doc.SelectSingleNode("/packet/condition[1]/@peek").Value);
            var expectedSecondConditionPeek = Convert.ToBoolean(doc.SelectSingleNode("/packet/condition[2]/@peek").Value);

            var expectedFirstConditionType = doc.SelectSingleNode("/packet/condition[1]/*[1]");
            var expectedSecondConditionType = doc.SelectSingleNode("/packet/condition[2]/*[1]");

            var parser = new PacketSpecParser(doc);
            var state = parser.Parse();

            Assert.That(state.Data, Has.Count.EqualTo(2));
            Assert.That(state.Data, Has.All.With.Property(nameof(PacketDataElement.DataType)).EqualTo(PacketDataType.Condition));

            var firstCondition = state.Data[0];
            var secondCondition = state.Data[1];

            Assert.That(firstCondition.Name, Is.Null.Or.Empty);
            Assert.That(secondCondition.Name, Is.Null.Or.Empty);

            var firstMem = (ConditionState)firstCondition.MemberState;
            Assert.That(firstMem.Peek, Is.EqualTo(expectedFirstConditionPeek));
            Assert.That(firstMem.Members[0].Name, Is.EqualTo(expectedFirstConditionType.InnerText));
            Assert.That(firstMem.Members[0].DataType, Is.EqualTo(GetEnum(expectedFirstConditionType.Name)));

            var casesList = firstMem.Cases;
            Assert.That(casesList, Has.Count.EqualTo(2));
            Assert.That(casesList.First().TestValue, Is.EqualTo("0"));
            Assert.That(casesList.First().Members, Has.Count.EqualTo(2));
            Assert.That(casesList.Last().TestValue, Is.EqualTo("1"));
            Assert.That(casesList.Last().Members, Has.Count.EqualTo(2));

            var secondMem = (ConditionState)secondCondition.MemberState;
            Assert.That(secondMem.Peek, Is.EqualTo(expectedSecondConditionPeek));
            Assert.That(secondMem.Members[0].Name, Is.EqualTo(expectedSecondConditionType.InnerText));
            Assert.That(secondMem.Members[0].DataType, Is.EqualTo(GetEnum(expectedSecondConditionType.Name)));

            casesList = secondMem.Cases;
            Assert.That(casesList, Has.Count.EqualTo(2));
            Assert.That(casesList.First().TestValue, Is.EqualTo("1"));
            Assert.That(casesList.First().Members, Has.Count.EqualTo(2));
            Assert.That(casesList.Last().TestValue, Is.EqualTo("0"));
            Assert.That(casesList.Last().Members, Has.Count.EqualTo(2));
        }

        [Test]
        public void GivenPacketWithGroup_WhenParse_GroupIsParsed()
        {
            var doc = new XmlDocument();
            doc.Load("PacketCompiler/Samples/group.xml");

            var parser = new PacketSpecParser(doc);
            var state = parser.Parse();

            Assert.That(state.Data, Has.Count.EqualTo(1));
            Assert.That(state.Data, Has.Exactly(1).With.Property(nameof(PacketDataElement.DataType)).EqualTo(PacketDataType.Group));

            var group = doc.SelectSingleNode("/packet/group");
            var groupState = (GroupState)state.Data[0].MemberState;

            var expectedCountType = GetEnum(group.Attributes["countType"].Value);
            Assert.That(groupState.CountType, Is.EqualTo(expectedCountType));

            var expectedBreakOn = int.Parse(group.Attributes["breakOn"].Value);
            Assert.That(groupState.BreakOn, Is.EqualTo(expectedBreakOn));

            var expectedBreakType = GetEnum(group.Attributes["breakType"].Value);
            Assert.That(groupState.BreakType, Is.EqualTo(expectedBreakType));

            var expectedPeek = bool.Parse(group.Attributes["peek"].Value);
            Assert.That(groupState.Peek, Is.EqualTo(expectedPeek));

            var expectedPreLoop = GetEnum(doc.SelectSingleNode("/packet/group/preLoop/*[1]").Name);
            Assert.That(groupState.PreLoop.DataType, Is.EqualTo(expectedPreLoop));

            var expectedPostLoop = GetEnum(doc.SelectSingleNode("/packet/group/postLoop/*[1]").Name);
            Assert.That(groupState.PostLoop.DataType, Is.EqualTo(expectedPostLoop));

            var expectedStructure = doc.SelectSingleNode("/packet/group/structure/@name").Value;
            Assert.That(groupState.Structure.Name, Is.EqualTo(expectedStructure));
        }

        [Test]
        public void GivenBasicGroup_NoOptions_GroupIsParsed()
        {
            var parser = new PacketSpecParser("PacketCompiler/Samples/basicgroup.xml");
            var state = parser.Parse();

            Assert.That(state.Data, Has.Count.EqualTo(1));
            Assert.That(state.Data, Has.Exactly(1).With.Property(nameof(PacketDataElement.DataType)).EqualTo(PacketDataType.Group));

            var groupState = (GroupState)state.Data[0].MemberState;

            Assert.That(groupState.Structure.MemberState.Members,
                Has.One.With.Property(nameof(PacketDataElement.Name)).EqualTo("Hello"));
            Assert.That(groupState.Structure.MemberState.Members,
                Has.One.With.Property(nameof(PacketDataElement.DataType)).EqualTo(PacketDataType.Byte));
        }

        [TestFixture]
        public class InvalidPackets
        {
            [Test]
            public void GivenInvalidRootElement_WhenParse_ThrowsExceptionWithSchemaError()
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
                    Throws.InstanceOf<InvalidPacketSpecException>().With
                        .Property(nameof(InvalidPacketSpecException.Result)).EqualTo(ValidationResult.InvalidRootElement));
            }

            [Test]
            public void GivenDuplicateElementNames_WhenParse_ThrowsException()
            {
                var doc = new XmlDocument();
                doc.LoadXml(@"<?xml version=""1.0"" encoding=""utf-8""?>
<packet name=""test"">
  <byte>duplicate</byte>
  <byte>duplicate</byte>
</packet>");

                var parser = new PacketSpecParser(doc);
                Assert.That(() => parser.Parse(),
                    Throws.InstanceOf<InvalidPacketSpecException>().With
                        .Property(nameof(InvalidPacketSpecException.Result)).EqualTo(ValidationResult.ElementRedefinition));
                Assert.That(() => parser.Parse(ParseOptions.SkipSchemaValidation),
                    Throws.InstanceOf<InvalidPacketSpecException>().With
                        .Property(nameof(InvalidPacketSpecException.Result)).EqualTo(ValidationResult.ElementRedefinition));
            }

            [Test]
            public void GivenDuplicateElementNames_DifferentScopes_WhenParse_DoesNotThrowException()
            {
                var doc = new XmlDocument();
                doc.LoadXml(@"<?xml version=""1.0"" encoding=""utf-8""?>
<packet name=""test"">
  <byte>duplicate</byte>
  <structure name=""teststruct"">
    <byte>duplicate</byte>
  </structure>
</packet>");

                var parser = new PacketSpecParser(doc);
                parser.Parse();
                parser.Parse(ParseOptions.SkipSchemaValidation);
            }

            [Test]
            public void GivenInvalidNodeType_ThrowsArgumentException()
            {
                const string invalidXml = "<packet name=\"thing\"><wrong>thing</wrong></packet>";
                var doc = new XmlDocument();
                doc.LoadXml(invalidXml);

                var parser = new PacketSpecParser(doc);
                Assert.That(() => parser.Parse(ParseOptions.SkipSchemaValidation), Throws.ArgumentException);
            }
        }

        private static PacketDataType GetEnum(string name) => Enum.Parse<PacketDataType>(char.ToUpper(name[0]) + name.Substring(1));
    }
}
