// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Linq;

namespace NetworkEngine.DataTransfer
{
    public class NumberEncoderService : INumberEncoder
    {
        private const int ONE_BYTE_MAX = 253;
        private const int TWO_BYTE_MAX = 64009;
        private const int THREE_BYTE_MAX = 16194277;

        public byte[] EncodeNumber(int number, int size)
        {
            var unsigned = (uint) number;
            var numArray = Enumerable.Repeat((uint)254, 4).ToArray();
            var original = unsigned;

            if (original >= THREE_BYTE_MAX)
            {
                numArray[3] = unsigned / THREE_BYTE_MAX + 1;
                unsigned = unsigned % THREE_BYTE_MAX;
            }

            if (original >= TWO_BYTE_MAX)
            {
                numArray[2] = unsigned / TWO_BYTE_MAX + 1;
                unsigned = unsigned % TWO_BYTE_MAX;
            }

            if (original >= ONE_BYTE_MAX)
            {
                numArray[1] = unsigned / ONE_BYTE_MAX + 1;
                unsigned = unsigned % ONE_BYTE_MAX;
            }

            numArray[0] = unsigned + 1;

            return numArray.Select(x => (byte)x)
                           .Take(size)
                           .ToArray();
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
                retNum += b[3]*THREE_BYTE_MAX;
            if (b.Length > 2)
                retNum += b[2]*TWO_BYTE_MAX;
            if (b.Length > 1)
                retNum += b[1]*ONE_BYTE_MAX;

            return retNum + b[0];
        }
    }
}
