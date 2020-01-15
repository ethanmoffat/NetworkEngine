using System.Diagnostics.CodeAnalysis;
using NetworkEngine.DataTransfer;
using NUnit.Framework;

namespace NetworkEngine.Test.DataTransfer
{
    [TestFixture]
    [ExcludeFromCodeCoverage]
    public class EndlessOnlineNumberEncoderTest
    {
        private readonly INumberEncoder _encoder = new EndlessOnlineNumberEncoder();

        [Test]
        public void NumberEncoder_OneByte_EncodesAndDecodes()
        {
            const int Expected = EndlessOnlineNumberEncoder.OneByteMax - 1;
            var encoded = _encoder.EncodeNumber(Expected, 1);
            var decoded = _encoder.DecodeNumber(encoded);
            Assert.That(decoded, Is.EqualTo(Expected));
        }

        [Test]
        public void NumberEncoder_TwoBytes_EncodesAndDecodes()
        {
            const int Expected = EndlessOnlineNumberEncoder.TwoByteMax - 1;
            var encoded = _encoder.EncodeNumber(Expected, 2);
            var decoded = _encoder.DecodeNumber(encoded);
            Assert.That(decoded, Is.EqualTo(Expected));
        }

        [Test]
        public void NumberEncoder_ThreeBytes_EncodesAndDecodes()
        {
            const int Expected = EndlessOnlineNumberEncoder.ThreeByteMax - 1;
            var encoded = _encoder.EncodeNumber(Expected, 3);
            var decoded = _encoder.DecodeNumber(encoded);
            Assert.That(decoded, Is.EqualTo(Expected));
        }

        [Test]
        public void NumberEncoder_FourBytes_EncodesAndDecodes()
        {
            const int Expected = EndlessOnlineNumberEncoder.ThreeByteMax + 1;
            var encoded = _encoder.EncodeNumber(Expected, 4);
            var decoded = _encoder.DecodeNumber(encoded);
            Assert.That(decoded, Is.EqualTo(Expected));
        }
    }
}
