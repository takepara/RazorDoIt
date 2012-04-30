using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace RazorDoIt
{
    public class KeyGenerator
    {
        public static string Generate(int length)
        {
            StringBuilder result = new StringBuilder(length);

            char[] chars = new char[36];
            string a;
            a = "abcdefghijklmnopqrstuvwxyz1234567890";
            chars = a.ToCharArray();
            int size = length;
            byte[] data = new byte[1];
            RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider();
            crypto.GetNonZeroBytes(data);
            size = length;
            data = new byte[size];
            crypto.GetNonZeroBytes(data);
            foreach (byte b in data)
            {
                result.Append(chars[b % (chars.Length - 1)]);
            }

            return result.ToString();
        }
    }
}
