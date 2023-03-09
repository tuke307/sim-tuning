// Copyright (c) 2021 tuke productions. All rights reserved.
using Microsoft.Extensions.Logging;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
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
                Uri uri = new Uri(url);
                BrowserLaunchOptions options = new BrowserLaunchOptions()
                {
                    LaunchMode = BrowserLaunchMode.SystemPreferred,
                    TitleMode = BrowserTitleMode.Show,
                    //PreferredToolbarColor = Colors.Violet,
                    //PreferredControlColor = Colors.SandyBrown
                };
                
                await Browser.Default.OpenAsync(uri, options);
            }
            catch (Exception ex)
            {
                // An unexpected error occured. No browser may be installed on the device.
                this._logger.LogError(ex, ex.Message, null);
            }
        }

        /// <inheritdoc />
        public async Task DownloadDocumentAsync(string fileToDownload, string fileSave)
        {
            using (var client = new HttpClient())
            {
                using (var s = await client.GetStreamAsync(fileToDownload))
                {
                    using (var fs = new FileStream(fileSave, FileMode.OpenOrCreate))
                    {
                        await s.CopyToAsync(fs);
                    }
                }
            }
        }
    }
}