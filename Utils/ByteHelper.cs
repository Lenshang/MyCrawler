using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyCrawler.Utils
{
    class ByteHelper
    {
        public static string Get16(IEnumerable<byte> buffer)
        {
            return BitConverter.ToString(buffer.ToArray(), 0).Replace("-", string.Empty).ToLower();
        }
        public static string Hash32(IEnumerable<byte> buffer)
        {
            return Get16(buffer.Reverse());
        }
    }
}
