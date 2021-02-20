using System;
using System.Collections.Generic;
using System.Text;

namespace btag
{
    public static class Converter
    {
        public static byte[] GetString(string str)
        {
            return Encoding.Default.GetBytes(str);
        }

        public static byte[] GetOptimized(int num)
        {
            if (num < byte.MaxValue)
            {
                return GetInt8(num);
            }
            else if (num < Int16.MaxValue)
            {
                return GetInt16(num);
            }
            else
            {
                return GetInt32(num);
            }
        }

        public static byte[] GetInt32(int num)
        {
            return BitConverter.GetBytes(num);
        }

        public static byte[] GetInt16(int num)
        {
            return BitConverter.GetBytes((Int16)num);
        }

        public static byte[] GetInt8(int num)
        {
            return new byte[] { (byte)num };
        }

        public static int ToOptimized(byte[] num)
        {
            if (num.Length == 4)
            {
                return ToInt32(num);
            }
            else if (num.Length == 2)
            {
                return ToInt16(num);
            }
            else
            {
                return 0;
            }
        }

        public static int ToInt32(byte[] num)
        {
            return BitConverter.ToInt32(num);
        }
        public static int ToInt16(byte[] num)
        {
            return BitConverter.ToInt16(num);
        }

        public static string ToString(byte[] str)
        {
            return Encoding.Default.GetString(str);
        }
    }
}
