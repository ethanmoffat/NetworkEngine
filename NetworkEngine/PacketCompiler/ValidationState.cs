﻿// Original Work Copyright (c) Ethan Moffat 2014-2019

using System.Xml.Schema;

namespace NetworkEngine.PacketCompiler
{
    internal class ValidationState
    {
        public ValidationResult Status { get; }
        public XmlSeverityType? ValidationSeverity { get; }
        public string ValidationMessage { get; }
        public int ValidationLineNumber { get; }

        public ValidationState(ValidationResult status)
            : this(status, null, string.Empty, 0) { }

        public ValidationState(ValidationResult status, XmlSeverityType? validationSeverity, string validationMessage, int validationLineNumber)
        {
            Status = status;
            ValidationSeverity = validationSeverity;
            ValidationMessage = validationMessage;
            ValidationLineNumber = validationLineNumber;
        }
    }
}