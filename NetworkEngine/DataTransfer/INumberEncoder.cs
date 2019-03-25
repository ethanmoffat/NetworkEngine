// Original Work Copyright (c) Ethan Moffat 2014-2019
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace NetworkEngine.DataTransfer
{
    public interface INumberEncoder
    {
        byte[] EncodeNumber(int number, int size);

        int DecodeNumber(params byte[] b);
    }
}
