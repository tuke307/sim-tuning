// Copyright (c) 2021 tuke productions. All rights reserved.
using Microsoft.Extensions.Logging;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SimTuning.Core.Services
{
    public class BrowserService : IBrowserService
    {
        private ILogger<BrowserService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="BrowserService" /> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public BrowserService(ILogger<BrowserService> logger)
        {
            this._logger = logger;
        }

        /// <inheritdoc />
        public async Task OpenBrowser(string url)
        {
            try
            {
                // mobile
                if (DeviceInfo.Platform != DevicePlatform.Unknown)
                {
                    await Browser.OpenAsync(url, BrowserLaunchMode.SystemPreferred);
                }
                // Windows
                else
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true,
                    };
                    Process.Start(psi);
                }
            }
            catch (Exception ex)
            {
                // An unexpected error occured. No browser may be installed on the device.
                this._logger.LogError(ex, ex.Message, null);
            }
        }
    }
}