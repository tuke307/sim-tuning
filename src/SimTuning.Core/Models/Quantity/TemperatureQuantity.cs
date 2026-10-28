// Copyright (c) 2025 tuke productions. All rights reserved.
namespace SimTuning.Core.Models.Quantity
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using UnitsNet.Units;

    /// <summary>
    /// TemperatureQuantity.
    /// </summary>
    /// <seealso cref="System.Collections.ObjectModel.ObservableCollection{SimTuning.Core.UnitListItem}" />
    public class TemperatureQuantity : ObservableCollection<UnitListItem>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TemperatureQuantity" /> class.
        /// </summary>
        public TemperatureQuantity()
            : base()
        {
            var customTemperatureUnits = new List<Enum>()
            {
                TemperatureUnit.DegreeCelsius,
                TemperatureUnit.DegreeFahrenheit,
                TemperatureUnit.Kelvin,
            };

            foreach (Enum unitValue in customTemperatureUnits)
            {
                this.Add(new UnitListItem(unitValue));
            }
        }
    }
}