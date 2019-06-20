﻿using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace MyCrawler.Utils
{
    public class MD5Helper
    {
        public static string getMd5Hash(string input)
        {
            if (String.IsNullOrEmpty(input))
            {
                return null;
            }

            var benchStr = input.Trim();
            // Create a new instance of the MD5CryptoServiceProvider object.
            MD5 md5Hasher = MD5.Create();

            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hasher.ComputeHash(Encoding.UTF8.GetBytes(benchStr));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }
    }
}
