// Copyright (c) 2021 tuke productions. All rights reserved.
using CommunityToolkit.Maui.Alerts;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices;
using SimTuning.Core.Converters;
using SkiaSharp;
using System;
using System.Collections;
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

namespace SimTuning.Core.Helpers
{
    /// <summary>
    /// allgemeine Funktionen.
    /// </summary>
    public static class Functions
    {
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
        /// Gets the permission.
        /// </summary>
        /// <typeparam name="TPermission">The type of the permission.</typeparam>
        /// <returns></returns>
        public static async Task<PermissionStatus> GetPermission<TPermission>()
            where TPermission : BasePermission, new()
        {
            PermissionStatus status = await CheckStatusAsync<TPermission>();

            if (status == PermissionStatus.Granted)
            {
                return status;
            }

            if (status == PermissionStatus.Denied && DeviceInfo.Platform == DevicePlatform.iOS)
            {
                // Prompt the user to turn on in settings On iOS once a permission has been denied it may not be requested again from the application
                if (typeof(TPermission) is Permissions.StorageRead)
                    Functions.ShowSnackbarDialog(SimTuning.Core.Helpers.Functions.GetLocalisedRes(typeof(SimTuning.Core.resources), "ERR_STORAGEREAD"));

                if (typeof(TPermission) is Permissions.StorageWrite)
                    Functions.ShowSnackbarDialog(SimTuning.Core.Helpers.Functions.GetLocalisedRes(typeof(SimTuning.Core.resources), "ERR_STORAGEWRITE"));

                if (typeof(TPermission) is Permissions.LocationWhenInUse)
                    Functions.ShowSnackbarDialog(SimTuning.Core.Helpers.Functions.GetLocalisedRes(typeof(SimTuning.Core.resources), "ERR_LOCATION"));

                if (typeof(TPermission) is Permissions.Microphone)
                    Functions.ShowSnackbarDialog(SimTuning.Core.Helpers.Functions.GetLocalisedRes(typeof(SimTuning.Core.resources), "ERR_MICROPHONE"));

                return status;
            }

            if (Permissions.ShouldShowRationale<TPermission>())
            {
                // Prompt the user with additional information as to why the permission is needed
                if (typeof(TPermission) is Permissions.StorageRead)
                    Functions.ShowSnackbarDialog(SimTuning.Core.Helpers.Functions.GetLocalisedRes(typeof(SimTuning.Core.resources), "ERR_STORAGEREAD"));

                if (typeof(TPermission) is Permissions.StorageWrite)
                    Functions.ShowSnackbarDialog(SimTuning.Core.Helpers.Functions.GetLocalisedRes(typeof(SimTuning.Core.resources), "ERR_STORAGEWRITE"));

                if (typeof(TPermission) is Permissions.LocationWhenInUse)
                    Functions.ShowSnackbarDialog(SimTuning.Core.Helpers.Functions.GetLocalisedRes(typeof(SimTuning.Core.resources), "ERR_LOCATION"));

                if (typeof(TPermission) is Permissions.Microphone)
                    Functions.ShowSnackbarDialog(SimTuning.Core.Helpers.Functions.GetLocalisedRes(typeof(SimTuning.Core.resources), "ERR_MICROPHONE"));
            }

            var result = await RequestAsync<Permissions.StorageWrite>();

            return status;
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

        public static List<int> GetPowersOf2(int startN, int maxN)
        {
            List<int> ints = new List<int>();

            for (int i = startN; i < maxN; i++)
            {
                ints.Add((int)Math.Pow(2, i));
            }

            return ints;
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