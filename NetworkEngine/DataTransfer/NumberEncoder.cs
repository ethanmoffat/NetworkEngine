// Original Work Copyright (c) Ethan Moffat 2014-2019
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Linq;

namespace NetworkEngine.DataTransfer
{
    public class NumberEncoder : INumberEncoder
    {
        internal const int OneByteMax = 253;
        internal const int TwoByteMax = 64009;
        internal const int ThreeByteMax = 16194277;

        public byte[] EncodeNumber(int number, int size)
        {
            var unsigned = (uint) number;
            var numArray = new byte[] {254, 254, 254, 254};
            var original = unsigned;

            if (original >= ThreeByteMax)
            {
                numArray[3] = (byte)(unsigned / ThreeByteMax + 1);
                unsigned = unsigned % ThreeByteMax;
            }

            if (original >= TwoByteMax)
            {
                numArray[2] = (byte)(unsigned / TwoByteMax + 1);
                unsigned = unsigned % TwoByteMax;
            }

            if (original >= OneByteMax)
            {
                numArray[1] = (byte)(unsigned / OneByteMax + 1);
                unsigned = unsigned % OneByteMax;
            }

            numArray[0] = (byte)(unsigned + 1);

            return numArray.Take(size).ToArray();
        }

        public int DecodeNumber(params byte[] b)
        {
            for (int index = 0; index < b.Length; ++index)
            {
                if (b[index] == 254)
                    b[index] = 1;
                else if (b[index] == 0)
                    b[index] = 128;
                --b[index];
            }

            var retNum = 0;
            if (b.Length > 3)
                retNum += b[3]*ThreeByteMax;
            if (b.Length > 2)
                retNum += b[2]*TwoByteMax;
            if (b.Length > 1)
                retNum += b[1]*OneByteMax;

            return retNum + b[0];
        }
    }
}
