// Original Work Copyright (c) Ethan Moffat 2014-2019

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Schema;

namespace NetworkEngine.PacketCompiler
{
    public class PacketSpecParser
    {
        private const string SchemaUri = "NetworkPacket.xsd";
        private const string PacketElement = "packet";
        private const string NameAttribute = "name";
        private const string BaseAttribute = "base";
        private const string LengthAttribute = "length";

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

            var elementName = dataType == PacketDataType.Structure
                              ? node.Attributes?[NameAttribute]?.Value 
                              : node.InnerText;

            var childElements = new List<PacketDataElement>();
            if (dataType == PacketDataType.Structure)
            {
                childElements.AddRange(ProcessStructure(node.ChildNodes));
            }

            return new PacketDataElement(dataType, length, elementName, childElements);
        }

        private static IReadOnlyList<PacketDataElement> ProcessStructure(XmlNodeList childNodes)
        {
            var retList = new List<PacketDataElement>();

            foreach (XmlNode node in childNodes)
            {
                retList.Add(XmlNodeToDataElement(node));
            }

            return retList;
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
                var allNodes = ToEnumerable<XmlNode>(specXml.SelectNodes("//*").GetEnumerator());
                var groupedByParent = allNodes.GroupBy(x => new {x.ParentNode, x.InnerText});
                foreach (var grouping in groupedByParent)
                {
                    var groupedByInnerText = grouping.GroupBy(x => x.InnerText);
                    if (groupedByInnerText.Any(x => x.Count() > 1))
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

            IEnumerable<T> ToEnumerable<T>(IEnumerator enumerator)
            {
                while (enumerator.MoveNext())
                    yield return (T)enumerator.Current;
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
