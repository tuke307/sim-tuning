// Copyright (c) 2025 tuke productions. All rights reserved.
using SkiaSharp;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;

namespace SimTuning.Core.Converters
{
    public class Converts
    {
        /// <summary>
        /// Secures the string to string.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static string SecureStringToString(SecureString value)
        {
            IntPtr valuePtr = IntPtr.Zero;
            try
            {
                valuePtr = Marshal.SecureStringToGlobalAllocUnicode(value);
                return Marshal.PtrToStringUni(valuePtr) ?? string.Empty;
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(valuePtr);
            }
        }

        /// <summary>
        /// Sks the bitmap to stream.
        /// </summary>
        /// <param name="bitmap">The bitmap.</param>
        /// <returns></returns>
        public static Stream SKBitmapToStream(SKBitmap bitmap)
        {
            // create an image COPY
            SKImage image = SKImage.FromBitmap(bitmap);

            // encode the image (defaults to PNG)
            SKData encoded = image.Encode();

            // get a stream over the encoded data
            return encoded.AsStream();
        }

        /// <summary>
        /// Strings to secure string.
        /// </summary>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        public static SecureString? StringToSecureString(string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length == 0)
            {
                return null;
            }

            var securePassword = new SecureString();

            foreach (char c in password)
                securePassword.AppendChar(c);

            securePassword.MakeReadOnly();
            return securePassword;
        }
    }
}