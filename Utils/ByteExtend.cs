using System;
using System.Collections.Generic;
using System.Text;

namespace MyCrawler.Utils
{
    public static class ByteExtend
    {
        public static byte ToByte(this sbyte sbt)
        {
            return Convert.ToByte(sbt);
        }

        public static sbyte ToSbyte(this byte bt)
        {
            return Convert.ToSByte(bt);
        }
    }
}
