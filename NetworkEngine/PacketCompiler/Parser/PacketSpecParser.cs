// Original Work Copyright (c) Ethan Moffat 2014-2019

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using NetworkEngine.Extensions;
using NetworkEngine.PacketCompiler.State;

namespace NetworkEngine.PacketCompiler.Parser
{
    public class PacketSpecParser : IPacketSpecParser
    {
        private const string SchemaUri = "NetworkPacket.xsd";
        private const string PacketElement = "packet";
        private const string NameAttribute = "name";
        private const string BaseAttribute = "base";
        private const string LengthAttribute = "length";
        private const string ValueAttribute = "value";
        private const string PeekName = "peek";
        private const string CountTypeName = "countType";
        private const string BreakOnName = "breakOn";
        private const string BreakTypeName = "breakType";

        private readonly XmlDocument _specXml;
        private readonly string _filePath;

        public PacketSpecParser(string filePath)
        {
            _filePath = filePath;

            _specXml = new XmlDocument();
            _specXml.Load(filePath);
            RemoveComments(_specXml);
        }

        internal PacketSpecParser(XmlDocument specXml)
        {
            _specXml = specXml;
            RemoveComments(_specXml);

            _filePath = string.Empty;
        }

        public PacketState Parse(ParseOptions options = ParseOptions.None)
        {
            var validationResult = ValidatePacketStructure(_specXml, options);
            if (validationResult.Status != ValidationResult.Ok)
            {
                throw new InvalidPacketSpecException(validationResult);
            }

            var packetName = GetPacketName(_specXml);
            var packetState = PacketState.Create(packetName);

            var basePacketName = GetBasePacket(_specXml);
            if (!string.IsNullOrEmpty(basePacketName))
                packetState = packetState.WithBasePacket(basePacketName);

            var node = _specXml?.DocumentElement?.FirstChild;
            while (node != null)
            {
                if (node.NodeType == XmlNodeType.Element)
                {
                    var dataElement = XmlNodeToDataElement(node);
                    packetState = packetState.WithData(dataElement);
                }

                node = node.NextSibling;
            }

            return packetState;
        }

        private static PacketDataElement XmlNodeToDataElement(XmlNode node)
        {
            var parseResult = Enum.TryParse(node.Name, result: out PacketDataType dataType, ignoreCase: true);
            if (!parseResult)
            {
                throw new ArgumentException($"Unable to parse packet data type of {node.Name}");
            }

            var attribute = node.Attributes[LengthAttribute];
            var length = int.Parse(attribute?.Value ?? "0");

            var elementName = node.InnerText;
            switch (dataType)
            {
                case PacketDataType.Structure:
                    elementName = node.Attributes[NameAttribute].Value;
                    break;
                case PacketDataType.Condition:
                case PacketDataType.Group:
                    elementName = string.Empty;
                    break;
            }

            IMemberState memberState = null;
            var childNodes = node.ChildNodes.GetEnumerator().ToEnumerable<XmlNode>();
            if (dataType == PacketDataType.Structure)
            {
                memberState = ProcessStructure(childNodes.ToList());
            }
            else if (dataType == PacketDataType.Condition)
            {
                var peek = node.Attributes[PeekName];
                memberState = ProcessCondition(peek, childNodes.ToList());
            }
            else if(dataType == PacketDataType.Group)
            {
                var countType = node.Attributes[CountTypeName]?.Value;
                var breakOn = node.Attributes[BreakOnName]?.Value;
                var breakType = node.Attributes[BreakTypeName]?.Value;
                var peek = node.Attributes[PeekName]?.Value;
                memberState = ProcessGroup(countType, breakOn, breakType, peek, childNodes.ToList());
            }

            return new PacketDataElement(dataType, length, elementName, memberState);
        }

        private static IMemberState ProcessStructure(IReadOnlyList<XmlNode> childNodes)
        {
            var retList = new List<PacketDataElement>();

            foreach (var node in childNodes)
            {
                retList.Add(XmlNodeToDataElement(node));
            }

            return new StructureState(retList);
        }

        private static IMemberState ProcessCondition(XmlAttribute peekAttribute, List<XmlNode> childNodes)
        {
            var peekValue = bool.Parse(peekAttribute.Value);

            var testType = XmlNodeToDataElement(childNodes[0]);

            var casesList = new List<ConditionState.CaseState>();
            var cases = childNodes
                .Where(x => x.Name.Equals("Case", StringComparison.InvariantCultureIgnoreCase))
                .ToList();
            foreach (var @case in cases)
            {
                var testValue = @case.Attributes[ValueAttribute].Value;
                var members = @case.ChildNodes
                    .GetEnumerator().ToEnumerable<XmlNode>()
                    .Select(XmlNodeToDataElement).ToList();
                casesList.Add(new ConditionState.CaseState(testValue, members));
            }

            return new ConditionState(peekValue, testType, casesList);
        }

        private static IMemberState ProcessGroup(string countTypeStr,
                                                 string breakOnStr,
                                                 string breakTypeStr,
                                                 string peekStr,
                                                 IReadOnlyList<XmlNode> childNodes)
        {
            var countTypeIsSet = Enum.TryParse(countTypeStr, result: out PacketDataType countType, ignoreCase: true);
            var breakOnIsSet = int.TryParse(breakOnStr, out var breakOn);
            var breakTypeIsSet = Enum.TryParse(breakTypeStr, result: out PacketDataType breakType, ignoreCase: true);
            var peekIsSet = bool.TryParse(peekStr, out var peek);

            var preLoopNode = childNodes
                .SingleOrDefault(x => x.Name.Equals("preLoop", StringComparison.CurrentCultureIgnoreCase))
                ?.FirstChild;
            var preLoopDataElement = preLoopNode == null ? null : XmlNodeToDataElement(preLoopNode);


            var postLoopNode = childNodes
                .SingleOrDefault(x => x.Name.Equals("postLoop", StringComparison.CurrentCultureIgnoreCase))
                ?.FirstChild;
            var postLoopDataElement = postLoopNode == null ? null : XmlNodeToDataElement(postLoopNode);

            var structureDataElement = XmlNodeToDataElement(childNodes
                .Single(x => x.Name.Equals("structure", StringComparison.CurrentCultureIgnoreCase)));

            var groupState = new GroupState(preLoopDataElement, postLoopDataElement, structureDataElement);

            if (countTypeIsSet)
                groupState.CountType = countType;
            if (breakOnIsSet)
                groupState.BreakOn = breakOn;
            if (breakTypeIsSet)
                groupState.BreakType = breakType;
            if (peekIsSet)
                groupState.Peek = peek;

            return groupState;
        }

        private static ValidationState ValidatePacketStructure(XmlDocument specXml, ParseOptions options)
        {
            var resultState = new ValidationState(ValidationResult.Ok);

            var validationMessage = string.Empty;
            var validationLineNumber = 0;
            XmlSeverityType? validationSeverity = null;
            var validationErrorFound = false;

            if (!options.HasFlag(ParseOptions.SkipSchemaValidation))
            {
                specXml.Schemas.Add(string.Empty, SchemaUri);
                specXml.Validate(ValidationHandler);

                if (validationErrorFound)
                {
                    resultState = new ValidationState(ValidationResult.SchemaError,
                        validationSeverity,
                        validationMessage,
                        validationLineNumber);
                }
            }
            else
            {
                if (specXml.DocumentElement == null ||
                    !string.Equals(specXml.DocumentElement.Name, PacketElement, StringComparison.OrdinalIgnoreCase))
                {
                    resultState = new ValidationState(ValidationResult.InvalidRootElement);
                }
            }

            if (resultState.Status == ValidationResult.Ok)
            {
                var allNodes = specXml.SelectNodes("/packet//*").GetEnumerator().ToEnumerable<XmlNode>();
                var groupedByParent = allNodes.Where(x => x.Name != "case").GroupBy(x => x.ParentNode).ToList();
                foreach (var grouping in groupedByParent)
                {
                    var groupedByInnerXml = grouping.GroupBy(x => x.InnerXml);
                    if (groupedByInnerXml.Any(x => x.Count() > 1))
                    {
                        resultState = new ValidationState(ValidationResult.ElementRedefinition);
                    }
                }
            }

            return resultState;

            void ValidationHandler(object sender, ValidationEventArgs e)
            {
                validationSeverity = e.Severity;
                validationLineNumber = e.Exception.LineNumber;
                validationMessage = e.Message;
                validationErrorFound = true;
            }
        }

        private static string GetPacketName(XmlDocument specXml)
        {
            var packetElement = specXml.DocumentElement;
            return packetElement.Attributes[NameAttribute].Value ?? string.Empty;
        }

        private static string GetBasePacket(XmlDocument specXml)
        {
            var packetElement = specXml.DocumentElement;
            return packetElement.Attributes[BaseAttribute]?.Value ?? string.Empty;
        }

        private static void RemoveComments(XmlDocument specXml)
        {
            var commentNodes = specXml.SelectNodes("/descendant-or-self::node()")
                .OfType<XmlNode>()
                .Where(x => x.NodeType == XmlNodeType.Comment)
                .Select(x => (Parent: x.ParentNode, Node: x))
                .ToList();

            foreach (var node in commentNodes)
            {
                node.Parent.RemoveChild(node.Node);
            }
        }
    }
}
