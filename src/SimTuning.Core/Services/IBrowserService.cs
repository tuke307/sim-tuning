// Copyright (c) 2021 tuke productions. All rights reserved.
using System;
using System.Threading.Tasks;

namespace SimTuning.Core.Services
{
    public interface IBrowserService
    {
        /// <summary>
        /// Opens the browser.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns></returns>
        Task OpenBrowser(string url);

        /// <summary>
        /// Downloads the document asynchronous.
        /// </summary>
        /// <param name="fileToDownload">The file to download.</param>
        /// <param name="fileSave">The file save.</param>
        /// <returns></returns>
        Task DownloadDocumentAsync(string fileToDownload, string fileSave);
    }
}