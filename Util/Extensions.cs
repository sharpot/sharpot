using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpOT.Util
{
    public static class Extensions
    {
        /// <summary>
        /// Repeats the specified string n times.
        /// </summary>
        /// <param name="instr">The input string.</param>
        /// <param name="n">The number of times input string 
        /// should be repeated.</param>
        /// <returns></returns>
        // http://weblogs.asp.net/gunnarpeipman/archive/2009/05/13/string-repeat-smaller-and-faster-version.aspx
        public static string Repeat(this string instr, int n)
        {
            if (string.IsNullOrEmpty(instr))
                return instr;

            var result = new StringBuilder(instr.Length * n);
            return result.Insert(0, instr, n).ToString();
        }

        #region Packets

        public static byte[] ToByteArray(this uint[] unsignedIntegers)
        {
            byte[] temp = new byte[unsignedIntegers.Length * 4];

            for (int i = 0; i < unsignedIntegers.Length; i++)
                Array.Copy(BitConverter.GetBytes(unsignedIntegers[i]), 0, temp, i * 4, 4);

            return temp;
        }

        public static uint[] ToUInt32Array(this byte[] bytes)
        {
            if (bytes.Length % 4 > 0)
                throw new Exception();

            uint[] temp = new uint[bytes.Length / 4];

            for (int i = 0; i < temp.Length; i++)
                temp[i] = BitConverter.ToUInt32(bytes, i * 4);

            return temp;
        }

        /// <summary>
        /// Convert an array of byte to a IP String (exemple: 127.0.0.1)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToIPString(this byte[] value)
        {
            string ret = "";

            for (int i = 0; i < value.Length; i++)
                ret += value[i] + ".";

            return ret.TrimEnd('.');
        }

        /// <summary>
        /// Convert an array of byte to a printable string.
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="start"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string ToPrintableString(this byte[] bytes, int start, int length)
        {
            string text = string.Empty;
            for (int i = start; i < start + length; i++)
            {
                text += bytes[i].ToPrintableChar();
            }
            return text;
        }

        /// <summary>
        /// Convert a byte to a printable
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static char ToPrintableChar(this byte value)
        {
            if (value < 32 || value > 126)
            {
                return (char)'.';
            }
            else
            {
                return (char)value;
            }
        }

        /// <summary>
        /// Converts a char to a byte
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte ToByte(this char value)
        {
            return (byte)value;
        }

        /// <summary>
        /// Converts a string to a byte array
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] ToByteArray(this string s)
        {
            List<byte> value = new List<byte>();
            foreach (char c in s.ToCharArray())
                value.Add(c.ToByte());
            return value.ToArray();
        }

        /// <summary>Convert a string of hex digits (ex: E4 CA B2) to a byte array.</summary>
        /// <param name="s">The string containing the hex digits (with or without spaces).</param>
        /// <returns>Returns an array of bytes.</returns>
        public static byte[] ToBytesAsHex(this string s)
        {
            s = s.Replace(" ", string.Empty);
            byte[] buffer = new byte[s.Length / 2];
            for (int i = 0; i < s.Length; i += 2)
                buffer[i / 2] = (byte)Convert.ToByte(s.Substring(i, 2), 16);
            return buffer;
        }

        /// <summary>
        /// Convert a string of hex digits to a printable string of characters.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToPrintableStringAsHex(this string value)
        {
            byte[] temp = value.ToBytesAsHex();
            string loc = "";
            for (int i = 0; i < temp.Length; i++)
            {
                loc += temp[i].ToPrintableChar();
            }
            return loc;
        }

        /// <summary>
        /// Converts a integer to a hex string
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToHexString(this int value)
        {
            byte[] temp = BitConverter.GetBytes(value);
            return temp.ToHexString(0, temp.Length);
        }

        /// <summary>
        /// Converts a hex string to a integer
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int ToIntAsHex(this string value)
        {
            byte[] bytes = value.ToBytesAsHex();
            if (bytes.Length >= 2)
                return BitConverter.ToInt16(bytes, 0);
            else
                return int.MinValue;
        }

        /// <summary>Converts an array of bytes into a formatted string of hex digits (ex: E4 CA B2)</summary>
        /// <param name="data">The array of bytes to be translated into a string of hex digits.</param>
        /// <param name="length">The length of data to convert</param>
        /// <returns>Returns a well formatted string of hex digits with spacing.</returns>
        public static string ToHexString(this byte[] data, int start, int length)
        {
            StringBuilder sb = new StringBuilder(data.Length * 3);
            for (int i = start; i < start + length; i++)
                sb.Append(Convert.ToString(data[i], 16).PadLeft(2, '0').PadRight(3, ' '));
            return sb.ToString().ToUpper();
        }

        /// <summary>Converts an array of bytes into a formatted string of hex digits (ex: E4 CA B2)</summary>
        /// <param name="data">The array of bytes to be translated into a string of hex digits.</param>
        /// <returns>Returns a well formatted string of hex digits with spacing.</returns>
        public static string ToHexString(this byte[] data)
        {
            return data.ToHexString(0, data.Length);
        }
        #endregion
    }
}
