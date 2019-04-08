// Original Work Copyright (c) Ethan Moffat 2014-2019

using System;
using System.Xml;

namespace NetworkEngine.PacketCompiler
{
    public class PacketSpecParser
    {
        private const string PacketElement = "Packet";
        private const string NameAttribute = "name";
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

            string FixCasing(string name) => char.ToUpper(name[0]) + name.Substring(1);
        }

        private static ValidationState ValidatePacketStructure(XmlDocument specXml)
        {
            var result = ValidationResult.Ok;

            if (!string.Equals(specXml.DocumentElement?.Name, PacketElement, StringComparison.OrdinalIgnoreCase))
            {
                result = ValidationResult.InvalidPacketNode;
            }

            return new ValidationState(result);
        }

        private static string GetPacketName(XmlDocument specXml)
        {
            var packetElement = specXml.DocumentElement;
            var nameAttribute = packetElement?.Attributes[NameAttribute]?.Value ?? string.Empty;
            return nameAttribute;
        }
    }
}
