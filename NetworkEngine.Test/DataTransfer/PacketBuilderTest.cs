using NetworkEngine.DataTransfer;
using NUnit.Framework;
using System.Diagnostics.CodeAnalysis;

namespace NetworkEngine.Test.DataTransfer
{
    [TestFixture]
    [ExcludeFromCodeCoverage]
    public class PacketBuilderTest
    {
        [Test]
        public void LengthMatchesNumberOfBytes()
        {
            var packetLength = new PacketBuilder()
                .AddByte(1)
                .AddByte(2)
                .AddByte(3)
                .AddShort(5)
                .AddThree(8)
                .AddInt(12)
                .Length;
            Assert.That(packetLength, Is.EqualTo(12));
        }

        [Test]
        public void BreakAddsMaxValueByte()
        {
            var packet = new PacketBuilder()
                .AddBreak()
                .Build();
            Assert.That(packet.PeekByte(), Is.EqualTo(byte.MaxValue));
        }

        [Test]
        public void AddByteAddsByte()
        {
            const byte TestValue = 123;
            var packet = new PacketBuilder()
                .AddByte(TestValue)
                .Build();
            Assert.That(packet.PeekByte(), Is.EqualTo(TestValue));
        }

        [Test]
        public void AddCharAddsEncodedChar()
        {
            const byte CharByte = 222;
            var packet = new PacketBuilder()
                .AddChar(CharByte)
                .Build();
            Assert.That(packet.PeekChar(), Is.EqualTo(CharByte));
        }

        [Test]
        public void AddShortAddsEncodedShort()
        {
            const short TestShort = 32123;
            var packet = new PacketBuilder()
                .AddShort(TestShort)
                .Build();
            Assert.That(packet.PeekShort(), Is.EqualTo(TestShort));
        }

        [Test]
        public void AddThreeAddsEncodedThree()
        {
            const int TestThree = 123456;
            var packet = new PacketBuilder()
                .AddThree(TestThree)
                .Build();
            Assert.That(packet.PeekThree(), Is.EqualTo(TestThree));
        }

        [Test]
        public void AddThreeAddsEncodedThreeWithOnlyThreeBytes()
        {
            const int TestThree = 33322211;
            var packet = new PacketBuilder()
                .AddThree(TestThree)
                .Build();
            
            var encoder = new EndlessOnlineNumberEncoder();
            var expectedValue = encoder.DecodeNumber(encoder.EncodeNumber(TestThree, 3));
            Assert.That(packet.PeekThree(), Is.EqualTo(expectedValue));
        }

        [Test]
        public void AddIntAddsEncodedInt()
        {
            const int TestInt = 463726193;
            var packet = new PacketBuilder()
                .AddInt(TestInt)
                .Build();
            Assert.That(packet.PeekInt(), Is.EqualTo(TestInt));
        }

        [Test]
        public void AddStringAddsAsciiEncodedString()
        {
            const string TestString = "I'm Encoded! ... Hi encoded, I'm dad";
            var packet = new PacketBuilder()
                .AddString(TestString)
                .Build();
            Assert.That(packet.PeekString(TestString.Length), Is.EqualTo(TestString));
        }

        [Test]
        public void AddBreakStringAddsStringAndMaxValueByte()
        {
            const string TestString = "I hate it when he does that";
            const int AppendedInt = 1234;
            var packet = new PacketBuilder()
                .AddBreakString(TestString)
                .AddInt(AppendedInt)
                .Build();
            Assert.That(packet.ReadBreakString(), Is.EqualTo(TestString));
            Assert.That(packet.ReadInt(), Is.EqualTo(AppendedInt));
        }

        [Test]
        public void AddBytesAddsExpectedBytes()
        {
            var bytes = new byte[] { 1, 2, 4, 8, 16 };
            var packet = new PacketBuilder()
                .AddBytes(bytes)
                .Build();
            Assert.That(packet.PeekBytes(packet.Length), Is.EquivalentTo(bytes));
        }
    }
}
