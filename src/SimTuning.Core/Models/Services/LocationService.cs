// Copyright (c) 2025 tuke productions. All rights reserved.
using Microsoft.Extensions.Logging;

namespace SimTuning.Core.Models
{
    /// <summary>
    /// Service for managing location operations.
    /// </summary>
    public class LocationService : ILocationService
    {
        private readonly ILogger<LocationService> _log;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocationService"/> class.
        /// </summary>
        /// <param name="log">The logger instance.</param>
        public LocationService(ILogger<LocationService> log)
        {
            _log = log;
        }

        // TODO: Implement location tracking functionality when required
        // This was previously using MvvmCross plugin which has been removed
    }
}