// Copyright (c) 2025 tuke productions. All rights reserved.
namespace SimTuning.Core.Models
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using UnitsNet;
    using UnitsNet.Units;

    /// <summary>
    /// SpeedQuantity.
    /// </summary>
    /// <seealso cref="System.Collections.ObjectModel.ObservableCollection{SimTuning.Core.UnitListItem}" />
    public class SpeedQuantity : ObservableCollection<UnitListItem>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpeedQuantity" /> class.
        /// </summary>
        public SpeedQuantity()
            : base()
        {
            var customSpeedUnits = new List<Enum>()
            {
                SpeedUnit.MillimeterPerMinute,
                SpeedUnit.DecimeterPerMinute,
                SpeedUnit.CentimeterPerMinute,
                SpeedUnit.DecimeterPerMinute,
                SpeedUnit.MeterPerMinute,
                SpeedUnit.KilometerPerMinute,

                SpeedUnit.MillimeterPerSecond,
                SpeedUnit.DecimeterPerSecond,
                SpeedUnit.CentimeterPerSecond,
                SpeedUnit.DecimeterPerSecond,
                SpeedUnit.MeterPerSecond,
                SpeedUnit.KilometerPerSecond,

                SpeedUnit.MillimeterPerHour,
                SpeedUnit.CentimeterPerHour,
                SpeedUnit.MeterPerHour,
                SpeedUnit.KilometerPerHour,
            };

            foreach (Enum unitValue in customSpeedUnits)
            {
                this.Add(new UnitListItem(unitValue));
            }
        }
    }
}