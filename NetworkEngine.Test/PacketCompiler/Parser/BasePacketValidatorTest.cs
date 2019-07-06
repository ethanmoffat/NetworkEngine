using NetworkEngine.PacketCompiler.Parser;
using NetworkEngine.PacketCompiler.State;
using NUnit.Framework;

namespace NetworkEngine.Test.PacketCompiler.Parser
{
    [TestFixture]
    public class BasePacketValidatorTest
    {
        [Test]
        public void GivenSetOfPackets_ValidatePacketStates_NonExistentBasePacketReturnsError()
        {
            var packetA = PacketState.Create("PacketA");
            var packetB = PacketState.Create("PacketB").WithBasePacket("SomeOtherPacket");

            var validationResult = new BasePacketValidator().ValidatePacketStates(packetA, packetB);

            Assert.That(validationResult.Status, Is.EqualTo(ValidationResult.NonexistentBasePacket));
            Assert.That(validationResult.ValidationMessage, Is.Not.Null.Or.Empty);
        }

        [Test]
        public void GivenSetOfPackets_ValidatePacketStates_AllBasePacketsExist_OkResult()
        {
            var packetA = PacketState.Create("PacketA");
            var packetB = PacketState.Create("PacketB").WithBasePacket("PacketA");

            var validationResult = new BasePacketValidator().ValidatePacketStates(packetA, packetB);

            Assert.That(validationResult.Status, Is.EqualTo(ValidationResult.Ok));
            Assert.That(validationResult.ValidationMessage, Is.Empty);
        }

        [Test]
        public void GivenSetOfPackets_ValidatePacketStates_CircularBasePacketDependencyReturnsError()
        {
            var packetA = PacketState.Create("PacketA").WithBasePacket("PacketB");
            var packetB = PacketState.Create("PacketB").WithBasePacket("PacketA");

            var validationResult = new BasePacketValidator().ValidatePacketStates(packetA, packetB);

            Assert.That(validationResult.Status, Is.EqualTo(ValidationResult.CircularBasePacketDependency));
            Assert.That(validationResult.ValidationMessage, Is.Not.Null.Or.Empty);
        }

        [Test]
        public void GivenSetOfPackets_ValidatePacketStates_CircularBasePacketDependencyReturnsError_NLevelsDeep()
        {
            var packetA = PacketState.Create("PacketA").WithBasePacket("PacketB");
            var packetB = PacketState.Create("PacketB").WithBasePacket("PacketC");
            var packetC = PacketState.Create("PacketC").WithBasePacket("PacketD");
            var packetD = PacketState.Create("PacketD").WithBasePacket("PacketE");
            var packetE = PacketState.Create("PacketE").WithBasePacket("PacketA");

            var validationResult = new BasePacketValidator().ValidatePacketStates(packetA, packetB, packetC, packetD, packetE);

            Assert.That(validationResult.Status, Is.EqualTo(ValidationResult.CircularBasePacketDependency));
            Assert.That(validationResult.ValidationMessage, Is.Not.Null.Or.Empty);
        }

        [Test]
        public void GivenSetOfPackets_WithNonExistentPacketInDependencyChain_ValidatePacketStates_ReturnsError()
        {
            var packetA = PacketState.Create("PacketA").WithBasePacket("PacketB");
            var packetB = PacketState.Create("PacketB").WithBasePacket("PacketC");
            var packetC = PacketState.Create("PacketC").WithBasePacket("PacketD");

            var validationResult = new BasePacketValidator().ValidatePacketStates(packetA, packetB, packetC);

            Assert.That(validationResult.Status, Is.EqualTo(ValidationResult.NonexistentBasePacket));
            Assert.That(validationResult.ValidationMessage, Is.Not.Null.Or.Empty);
        }
    }
}