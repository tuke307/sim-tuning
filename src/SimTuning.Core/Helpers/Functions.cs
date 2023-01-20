// Copyright (c) 2021 tuke productions. All rights reserved.
using SimTuning.Core.Converters;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Resources;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.Maui.ApplicationModel.Permissions;
using CommunityToolkit.Maui.Alerts;
using Microsoft.Maui.ApplicationModel;
using System.Collections;

namespace SimTuning.Core.Helpers
{
    /// <summary>
    /// allgemeine Funktionen.
    /// </summary>
    public static class Functions
    {
        #region variables

        // This constant determines the number of iterations for the password bytes
        // generation function.
        private const int DerivationIterations = 1000;

        private const int Keysize = 128;

        #endregion variables

        /// <summary>
        /// Gets the permission.
        /// </summary>
        /// <typeparam name="TPermission">The type of the permission.</typeparam>
        /// <returns></returns>
        public static async Task<bool> GetPermission<TPermission>()
            where TPermission : BasePermission, new()
        {
            var hasPermission = await CheckStatusAsync<TPermission>().ConfigureAwait(true);

            if (hasPermission == PermissionStatus.Granted)
            {
                return true;
            }
            else if (hasPermission == PermissionStatus.Disabled)
            {
                return false;
            }

            var result = await RequestAsync<TPermission>().ConfigureAwait(true);
            if (result != PermissionStatus.Granted)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Shows the snackbar dialog.
        /// </summary>
        /// <param name="content">The content.</param>
        public static async void ShowSnackbarDialog(object content)
        {
            if (content == null)
            {
                return;
            }

            if (content.GetType().IsGenericType && content is IEnumerable)
            {
                List<string> messages = (List<string>)content;

                foreach (var message in messages)
                {
                    await Snackbar
                        .Make(
                        message: message,
                        duration: TimeSpan.FromSeconds(3))
                        .Show();
                }
            }
            else
            {
                await Snackbar
                    .Make(
                    message: content.ToString(),
                    duration: TimeSpan.FromSeconds(3))
                    .Show();
            }
        }

        /// <summary>
        /// Create a ZIP file of the files provided.
        /// </summary>
        /// <param name="fileName">The full path and name to store the ZIP file at.</param>
        /// <param name="files">The list of files to be added.</param>
        public static void CreateZipFile(string fileName, IEnumerable<string> files)
        {
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            // Create and open a new ZIP file
            var zip = ZipFile.Open(fileName, ZipArchiveMode.Create);
            foreach (var file in files)
            {
                // Add the entry for each file
                zip.CreateEntryFromFile(file, Path.GetFileName(file), CompressionLevel.Optimal);
            }
            // Dispose of the object when we are done
            zip.Dispose();
        }

        /// <summary>
        /// Decrypts the specified cipher text.
        /// </summary>
        /// <param name="cipherText">The cipher text.</param>
        /// <param name="passPhrase">The pass phrase.</param>
        /// <returns></returns>
        public static string Decrypt(string cipherText, string passPhrase)
        {
            // Get the complete stream of bytes that represent: [32 bytes of Salt] + [16
            // bytes of IV] + [n bytes of CipherText]
            var cipherTextBytesWithSaltAndIv = Convert.FromBase64String(cipherText);
            // Get the saltbytes by extracting the first 16 bytes from the supplied
            // cipherText bytes.
            var saltStringBytes = cipherTextBytesWithSaltAndIv.Take(Keysize / 8).ToArray();
            // Get the IV bytes by extracting the next 16 bytes from the supplied
            // cipherText bytes.
            var ivStringBytes = cipherTextBytesWithSaltAndIv.Skip(Keysize / 8).Take(Keysize / 8).ToArray();
            // Get the actual cipher text bytes by removing the first 64 bytes from the
            // cipherText string.
            var cipherTextBytes = cipherTextBytesWithSaltAndIv.Skip((Keysize / 8) * 2).Take(cipherTextBytesWithSaltAndIv.Length - ((Keysize / 8) * 2)).ToArray();

            using (var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations))
            {
                var keyBytes = password.GetBytes(Keysize / 8);
                using (var symmetricKey = new RijndaelManaged())
                {
                    symmetricKey.BlockSize = 128;
                    symmetricKey.Mode = CipherMode.CBC;
                    symmetricKey.Padding = PaddingMode.PKCS7;
                    using (var decryptor = symmetricKey.CreateDecryptor(keyBytes, ivStringBytes))
                    {
                        using (var memoryStream = new MemoryStream(cipherTextBytes))
                        {
                            using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                            {
                                var plainTextBytes = new byte[cipherTextBytes.Length];
                                var decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                                memoryStream.Close();
                                cryptoStream.Close();
                                return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Encrypts the specified plain text.
        /// </summary>
        /// <param name="plainText">The plain text.</param>
        /// <param name="passPhrase">The pass phrase.</param>
        /// <returns></returns>
        public static string Encrypt(string plainText, string passPhrase)
        {
            // Salt and IV is randomly generated each time, but is preprended to encrypted
            // cipher text so that the same Salt and IV values can be used when
            // decrypting.
            var saltStringBytes = Generate128BitsOfRandomEntropy();
            var ivStringBytes = Generate128BitsOfRandomEntropy();
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            using (var password = new Rfc2898DeriveBytes(passPhrase.ToString(), saltStringBytes, DerivationIterations))
            {
                var keyBytes = password.GetBytes(Keysize / 8);
                using (var symmetricKey = new RijndaelManaged())
                {
                    symmetricKey.BlockSize = 128;
                    symmetricKey.Mode = CipherMode.CBC;
                    symmetricKey.Padding = PaddingMode.PKCS7;
                    using (var encryptor = symmetricKey.CreateEncryptor(keyBytes, ivStringBytes))
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                            {
                                cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                                cryptoStream.FlushFinalBlock();
                                // Create the final bytes as a concatenation of the random
                                // salt bytes, the random iv bytes and the cipher bytes.
                                var cipherTextBytes = saltStringBytes;
                                cipherTextBytes = cipherTextBytes.Concat(ivStringBytes).ToArray();
                                cipherTextBytes = cipherTextBytes.Concat(memoryStream.ToArray()).ToArray();
                                memoryStream.Close();
                                cryptoStream.Close();
                                return Convert.ToBase64String(cipherTextBytes);
                            }
                        }
                    }
                }
            }
        }

        public static string GetLocalisedRes(Type resType, string resourceNameKey)
        {
            string translate = string.Empty;
            // string baseName = "SimTuning.Core." + resourceName;

            try
            {
                // Type resType = Type.GetType(baseName);
                ResourceManager rm = new ResourceManager(resType);
                translate = rm.GetString(resourceNameKey, CultureInfo.CurrentCulture);
            }
            catch
            {
                translate = string.Empty;
            }
            return translate;
        }

        /// <summary>
        /// Berechnet den Punkt auf einem beliebigen Kreis.
        /// </summary>
        /// <param name="angle">The angle.</param>
        /// <param name="radius">The radius.</param>
        /// <returns>Punkt auf dem Kreis.</returns>
        public static SKPoint GetPointOnCircle(double angle, double radius)
        {
            SKPoint coordinates = new SKPoint();

            double x_richtung = radius * Math.Cos(angle * Math.PI / 180);
            coordinates.X = Convert.ToInt32(radius + x_richtung);

            double y_richtung = radius * Math.Sin(angle * Math.PI / 180);
            if (y_richtung >= 0)
            {
                coordinates.Y = Convert.ToInt32(radius - y_richtung);
            }
            else if (y_richtung <= 0)
            {
                coordinates.Y = Convert.ToInt32(radius + Math.Abs(y_richtung));
            }

            return coordinates;
        }

        /// <summary>
        /// Rotiert die SKBitmap.
        /// </summary>
        /// <param name="bitmap">The bitmap.</param>
        /// <param name="degrees">The degrees.</param>
        /// <returns>Das gedrehte Bild.</returns>
        public static SKBitmap RotateBitmap(SKBitmap bitmap, int degrees)
        {
            var rotated = new SKBitmap(bitmap.Width, bitmap.Height);

            var surface = new SKCanvas(rotated);

            surface.Translate(rotated.Width / 2, rotated.Height / 2);
            surface.RotateDegrees(degrees);
            surface.Translate(-rotated.Width / 2, -rotated.Height / 2);
            surface.DrawBitmap(bitmap, 0, 0);

            return rotated;
        }

        /// <summary>
        /// Updates the value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="selectedFromUnit">The selected from unit.</param>
        /// <param name="selectedToUnit">The selected to unit.</param>
        /// <returns></returns>
        public static double? UpdateValue(double? value, UnitListItem selectedFromUnit, UnitListItem selectedToUnit)
        {
            if (value == null || selectedFromUnit == null || selectedToUnit == null)
            {
                return null;
            }

            return Math.Round(
                UnitsNet.UnitConverter.Convert(
                    value.Value,
                    selectedFromUnit.UnitEnumValue,
                    selectedToUnit.UnitEnumValue), 2);
        }

        /// <summary>
        /// Generate128s the bits of random entropy.
        /// </summary>
        /// <returns></returns>
        private static byte[] Generate128BitsOfRandomEntropy()
        {
            var randomBytes = new byte[16]; // 16 Bytes will give us 128 bits.
            using (var rngCsp = new RNGCryptoServiceProvider())
            {
                // Fill the array with cryptographically secure random bytes.
                rngCsp.GetBytes(randomBytes);
            }
            return randomBytes;
        }
    }
}