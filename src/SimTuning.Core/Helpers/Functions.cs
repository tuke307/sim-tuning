// Copyright (c) 2025 tuke productions. All rights reserved.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Resources;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Maui.Alerts;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices;
using SimTuning.Core.Converters;
using SkiaSharp;
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
            try
            {
                ResourceManager rm = new ResourceManager(resType);
                return rm.GetString(resourceNameKey, CultureInfo.CurrentCulture) ?? string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Gets the permission status and requests it if needed.
        /// </summary>
        /// <typeparam name="TPermission">The type of the permission.</typeparam>
        /// <returns>The permission status.</returns>
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
                // On iOS, once a permission has been denied it may not be requested again from the application
                ShowPermissionDeniedMessage<TPermission>();
                return status;
            }

            if (Permissions.ShouldShowRationale<TPermission>())
            {
                // Show rationale before requesting permission
                ShowPermissionDeniedMessage<TPermission>();
            }

            // Request the permission
            status = await RequestAsync<TPermission>();
            return status;
        }

        /// <summary>
        /// Shows the appropriate error message for denied permissions.
        /// </summary>
        /// <typeparam name="TPermission">The type of the permission.</typeparam>
        private static void ShowPermissionDeniedMessage<TPermission>()
            where TPermission : BasePermission
        {
            string? messageKey = typeof(TPermission).Name switch
            {
                nameof(Permissions.StorageRead) => "ERR_STORAGEREAD",
                nameof(Permissions.StorageWrite) => "ERR_STORAGEWRITE",
                nameof(Permissions.LocationWhenInUse) => "ERR_LOCATION",
                nameof(Permissions.Microphone) => "ERR_MICROPHONE",
                _ => null,
            };

            if (messageKey != null)
            {
                _ = ShowSnackbarDialogAsync(GetLocalisedRes(typeof(resources), messageKey));
            }
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
        public static async Task ShowSnackbarDialogAsync(object content)
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
                    await Snackbar.Make(message: message, duration: TimeSpan.FromSeconds(3)).Show();
                }
            }
            else
            {
                await Snackbar
                    .Make(message: content.ToString() ?? string.Empty, duration: TimeSpan.FromSeconds(3))
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
        public static double? UpdateValue(double? value, UnitListItem? selectedFromUnit, UnitListItem? selectedToUnit)
        {
            if (value == null || selectedFromUnit == null || selectedToUnit == null)
            {
                return null;
            }

            return Math.Round(
                UnitsNet.UnitConverter.Convert(
                    value.Value,
                    selectedFromUnit.UnitEnumValue,
                    selectedToUnit.UnitEnumValue
                ),
                2
            );
        }
    }
}
