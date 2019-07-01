// Original Work Copyright (c) Ethan Moffat 2014-2019

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using Moq;
using NetworkEngine.DataTransfer;
using NUnit.Framework;

namespace NetworkEngine.Test.DataTransfer
{
    [TestFixture]
    [ExcludeFromCodeCoverage]
    public class PacketTest
    {
        [Test]
        public void Packet_HasLength_EqualToDataLength()
        {
            var data = new byte[] {0, 1, 2};
            var packet = new Packet(data);
            Assert.That(packet.Length, Is.EqualTo(data.Length));
        }

        [Test]
        public void Packet_ReadPosition_PointsAtNextByteToRead()
        {
            var data = new byte[] {0, 1, 2, 3};
            var packet = new Packet(data);
            Assert.That(packet.ReadPosition, Is.EqualTo(0));

            packet.ReadByte();
            Assert.That(packet.ReadPosition, Is.EqualTo(1));
        }

        [Test]
        public void Packet_RawData_MatchesInputData()
        {
            var data = new byte[] {0, 1};
            var packet = new Packet(data);
            CollectionAssert.AreEqual(data, packet.RawData);
        }

        [Test]
        public void PacketSeeksFromBeginning()
        {
            var data = Enumerable.Repeat<byte>(5, 10).ToArray();
            var packet = new Packet(data);
            packet.Seek(5, SeekOrigin.Begin);
            Assert.That(packet.ReadPosition, Is.EqualTo(5));
        }

        [Test]
        public void PacketSeeksFromCurrent()
        {
            var data = Enumerable.Repeat<byte>(5, 10).ToArray();
            var packet = new Packet(data);
            packet.ReadByte();
            packet.Seek(5, SeekOrigin.Current);
            Assert.That(packet.ReadPosition, Is.EqualTo(6));
        }

        [Test]
        public void PacketSeeksFromEnd()
        {
            var data = Enumerable.Repeat<byte>(5, 10).ToArray();
            var packet = new Packet(data);
            packet.Seek(-1, SeekOrigin.End);
            Assert.That(packet.ReadPosition, Is.EqualTo(packet.Length - 2));
        }

        [Test]
        public void PacketThrowsInvalidSeekOrigin()
        {
            var packet = new Packet(new byte[] { 1, 2, 3 });
            Assert.That(() => packet.Seek(0, (SeekOrigin)200), Throws.InstanceOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void Packet_SeekPastEnd_ThrowsArgumentOutOfRangeException()
        {
            var data = Enumerable.Repeat<byte>(5, 10).ToArray();
            var packet = new Packet(data);

            Assert.That(() => packet.Seek(2, SeekOrigin.End), Throws.InstanceOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void Packet_ReadPastEnd_ThrowsInvalidOperationException()
        {
            var packet = new Packet(new byte[] { 1, 2, 3 });
            Assert.That(() => packet.ReadBytes(4), Throws.InvalidOperationException);
        }

        [Test]
        public void Packet_GettingNumbers_UsesDecoder()
        {
            var numberEncoderMock = Mock.Of<INumberEncoder>();

            var data = Enumerable.Repeat<byte>(10, 100);
            var packet = new Packet(data.ToList(), numberEncoderMock);

            packet.PeekChar();
            packet.PeekShort();
            packet.PeekThree();
            packet.PeekInt();
            packet.ReadChar();
            packet.ReadShort();
            packet.ReadThree();
            packet.ReadInt();

            Mock.Get(numberEncoderMock).Verify(x => x.DecodeNumber(It.IsAny<byte[]>()), Times.Exactly(8));
        }

        [Test]
        public void Packet_PeekStringFunctions_PeeksAsciiEncodedString()
        {
            const string TestString = "This is a test :)";
            var data = Encoding.ASCII.GetBytes(TestString);
            var packet = new Packet(data);
            var originalReadPos = packet.ReadPosition;

            var peekStringResult = packet.PeekString(TestString.Length);
            var peekEndStringResult = packet.PeekEndString();
            var peekBreakStringResult = packet.PeekBreakString();

            Assert.That(peekStringResult, Is.EqualTo(TestString));
            Assert.That(peekEndStringResult, Is.EqualTo(TestString));
            Assert.That(peekBreakStringResult, Is.EqualTo(TestString));
            Assert.That(packet.ReadPosition, Is.EqualTo(originalReadPos));
        }

        [Test]
        public void Packet_ReadStringFunctions_PeeksAsciiEncodedString()
        {
            const string TestString = "This is a test :)";
            var data = Encoding.ASCII.GetBytes(TestString);
            var packet = new Packet(data);
            var originalReadPos = packet.ReadPosition;

            var readStringResult = packet.ReadString(TestString.Length);
            Assert.That(packet.ReadPosition, Is.EqualTo(packet.Length));
            Assert.That(readStringResult, Is.EqualTo(TestString));
            packet.Seek(-readStringResult.Length, SeekOrigin.Current);

            var readEndStringResult = packet.ReadEndString();
            Assert.That(packet.ReadPosition, Is.EqualTo(packet.Length));
            Assert.That(readEndStringResult, Is.EqualTo(TestString));
            packet.Seek(-readEndStringResult.Length, SeekOrigin.Current);

            var readBreakStringResult = packet.ReadBreakString();
            Assert.That(packet.ReadPosition, Is.EqualTo(packet.Length));
            Assert.That(readBreakStringResult, Is.EqualTo(TestString));
            packet.Seek(-readBreakStringResult.Length, SeekOrigin.Current);
        }

        [Test]
        public void Packet_BreakString_DoesNotReadPastBreakByte()
        {
            const string TestString = "Shrek 2 electric boogaloo";
            var stringData = Encoding.ASCII.GetBytes(TestString);
            var packetData = new List<byte>();
            packetData.AddRange(stringData);
            packetData.Add(byte.MaxValue);
            packetData.Add((byte)'A');
            packetData.Add((byte)'B');
            packetData.Add((byte)'C');

            var packet = new Packet(packetData);

            var peekString = packet.PeekBreakString();
            Assert.That(peekString, Is.EqualTo(TestString));

            var readString = packet.ReadBreakString();
            Assert.That(readString, Is.EqualTo(TestString));
            Assert.That(packet.ReadPosition, Is.EqualTo(packet.Length - 3));

            var remainingBytes = packet.ReadBytes(3);
            Assert.That(remainingBytes, Is.EquivalentTo(new[] { (byte)'A', (byte)'B', (byte)'C' }));
        }

        [Test]
        public void Packet_DecodesNumbersWithZeroByteProperly()
        {
            // expected: 3*MAX_THREE+127*MAX_TWO+2*MAX_1+1
            const int ExpectedResult = 56712481;
            var bytes = new byte[] { 2, 3, 0, 4 };
            var packet = new Packet(bytes);
            Assert.That(packet.ReadInt(), Is.EqualTo(ExpectedResult));
        }
    }
}
