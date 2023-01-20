// Copyright (c) 2021 tuke productions. All rights reserved.
using Microsoft.Extensions.Logging;
using SimTuning.Core.Models;
using System;

namespace SimTuning.Core.Models
{
    public class LocationService
       : ILocationService
    {
        private readonly ILogger<LocationService> _log;

        //private readonly IMvxLocationWatcher _watcher;

        //public LocationService(IMvxLocationWatcher watcher, IMvxMessenger messenger, ILogger<LocationService> log)
        //{
        //    this._watcher = watcher;
        //    this._messenger = messenger;
        //    this._log = log;

        //    var options = new MvxLocationOptions
        //    {
        //        Accuracy = MvxLocationAccuracy.Fine,
        //        TrackingMode = MvxLocationTrackingMode.Foreground,
        //        TimeBetweenUpdates = TimeSpan.Zero,
        //        MovementThresholdInM = 0,
        //    };

        //    this._watcher.Start(options, this.OnLocation, this.OnError);
        //}

        //private void OnError(MvxLocationError error)
        //{
        //    this._log.LogWarning($"Location Error: {error.Code} {error.ToString()}");
        //}

        //private void OnLocation(MvxGeoLocation location)
        //{
        //    var message = new MvxLocationMessage(
        //        this,
        //        location.Coordinates.Latitude,
        //        location.Coordinates.Longitude,
        //        location.Coordinates.Altitude,
        //        location.Coordinates.Speed);

        //    this._messenger.Publish(message);
        //}

        //~LocationService()
        //{
        //    this._watcher.Stop();
        //}
    }
}