// Original Work Copyright (c) Ethan Moffat 2014-2019

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using NetworkEngine.Extensions;
using NetworkEngine.PacketCompiler.State;

namespace NetworkEngine.PacketCompiler
{
    public class PacketSpecParser
    {
        private const string SchemaUri = "NetworkPacket.xsd";
        private const string PacketElement = "packet";
        private const string NameAttribute = "name";
        private const string BaseAttribute = "base";
        private const string LengthAttribute = "length";
        private const string ValueAttribute = "value";
        private const string PeekName = "peek";

        private readonly XmlDocument _specXml;
        private readonly string _filePath;

        public PacketSpecParser(string filePath)
        {
            _filePath = filePath;

            _specXml = new XmlDocument();
            _specXml.Load(filePath);
        }

        internal PacketSpecParser(XmlDocument specXml)
        {
            _specXml = specXml;
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
                var dataElement = XmlNodeToDataElement(node);
                packetState = packetState.WithData(dataElement);
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

            var attribute = node.Attributes?[LengthAttribute];
            var length = int.Parse(attribute?.Value ?? "0");

            var elementName = node.InnerText;
            switch (dataType)
            {
                case PacketDataType.Structure:
                    elementName = node.Attributes?[NameAttribute]?.Value;
                    break;
                case PacketDataType.Condition:
                    elementName = string.Empty;
                    break;
            }

            IMemberState memberState = null;
            if (dataType == PacketDataType.Structure)
            {
                memberState = ProcessStructure(node.ChildNodes);
            }
            else if (dataType == PacketDataType.Condition)
            {
                memberState = ProcessCondition(node.Attributes[PeekName],
                                               node.ChildNodes.GetEnumerator().ToEnumerable<XmlNode>());
            }

            return new PacketDataElement(dataType, length, elementName, memberState);
        }

        private static IMemberState ProcessStructure(XmlNodeList childNodes)
        {
            var retList = new List<PacketDataElement>();

            foreach (XmlNode node in childNodes)
            {
                retList.Add(XmlNodeToDataElement(node));
            }

            return new StructureState(retList);
        }

        private static IMemberState ProcessCondition(XmlAttribute peekAttribute, IEnumerable<XmlNode> childNodes)
        {
            var peekValue = bool.Parse(peekAttribute.Value);

            var childNodesList = childNodes.ToList();
            var testType = XmlNodeToDataElement(childNodesList[0]);

            var casesList = new List<ConditionState.CaseState>();
            var cases = childNodesList
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
    }
}
