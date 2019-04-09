// Original Work Copyright (c) Ethan Moffat 2014-2019

using System;
using System.Xml;
using System.Xml.Schema;

namespace NetworkEngine.PacketCompiler
{
    public class PacketSpecParser
    {
        private const string SchemaUri = "NetworkPacket.xsd";
        private const string PacketElement = "Packet";
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
            var validationResult = ValidatePacketStructure(_specXml);
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
            while (node?.NextSibling != null)
            {
                var dataElement = GetNextDataElement(node);
                packetState = packetState.WithData(dataElement);

                node = node.NextSibling;
            }

            return packetState;
        }

        private static PacketDataElement GetNextDataElement(XmlNode node)
        {
            var dataType = (PacketDataType)Enum.Parse(typeof(PacketDataType), FixCasing(node.Name));

            var attribute = node.Attributes?[LengthAttribute];
            var length = int.Parse(attribute?.Value ?? "0");

            var elementName = node.InnerText;

            return new PacketDataElement(dataType, length, elementName);

            string FixCasing(string name) => char.ToUpper(name[0]) + name.Substring(1).ToLower();
        }

        private static ValidationState ValidatePacketStructure(XmlDocument specXml)
        {
            var resultState = new ValidationState(ValidationResult.Ok);

            var validationMessage = string.Empty;
            var validationLineNumber = 0;
            XmlSeverityType? validationSeverity = null;
            var validationErrorFound = false;

            specXml.Schemas.Add(string.Empty, SchemaUri);
            specXml.Validate(ValidationHandler);

            if (validationErrorFound)
            {
                resultState = new ValidationState(ValidationResult.SchemaError,
                    validationSeverity,
                    validationMessage,
                    validationLineNumber);
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
