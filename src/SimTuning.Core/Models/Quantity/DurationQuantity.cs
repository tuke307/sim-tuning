// Copyright (c) 2025 tuke productions. All rights reserved.
namespace SimTuning.Core.Models
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using UnitsNet.Units;

    /// <summary>
    /// DurationQuantity.
    /// </summary>
    /// <seealso cref="System.Collections.ObjectModel.ObservableCollection{SimTuning.Core.UnitListItem}" />
    public class DurationQuantity : ObservableCollection<UnitListItem>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DurationQuantity" /> class.
        /// </summary>
        public DurationQuantity()
            : base()
        {
            var customDurationUnits = new List<Enum>()
            {
                DurationUnit.Millisecond,
                DurationUnit.Second,
                DurationUnit.Hour,
                DurationUnit.Day,
            };

            foreach (Enum unitValue in customDurationUnits)
            {
                this.Add(new UnitListItem(unitValue));
            }
        }
    }
}