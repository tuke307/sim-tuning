// Copyright (c) 2025 tuke productions. All rights reserved.
namespace SimTuning.Data.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// BaseModel.
    /// </summary>
    public class BaseEntityModel
    {
        /// <summary>
        /// Gets or sets the created date.
        /// </summary>
        /// <value>The created date.</value>
        public DateTime? CreatedDate { get; set; }

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        [Key]
        public int? Id { get; set; }

        /// <summary>
        /// Gets or sets the updated date.
        /// </summary>
        /// <value>The updated date.</value>
        public DateTime? UpdatedDate { get; set; }

        /// <summary>
        /// Converts a quantity value from <paramref name="fromUnit" /> to
        /// <paramref name="toUnit" /> for a <c>*Unit</c> setter, applying the global
        /// rounding policy (<see cref="UnitSettings.RoundOnUnitChange" /> /
        /// <see cref="UnitSettings.RoundingAccuracy" />). Returns <paramref name="value" />
        /// unchanged when it has no value. Centralizes the conversion logic that was
        /// previously inlined (and duplicated ~46×) in every <c>*Unit</c> setter.
        /// </summary>
        /// <param name="value">The current quantity value (in <paramref name="fromUnit" />).</param>
        /// <param name="fromUnit">The unit <paramref name="value" /> is currently in.</param>
        /// <param name="toUnit">The unit to convert to.</param>
        /// <returns>The converted (and optionally rounded) value, or <paramref name="value" /> if null.</returns>
        /// <remarks>
        /// <paramref name="fromUnit" /> / <paramref name="toUnit" /> are <see cref="Enum" /> so the
        /// nullable UnitsNet unit types (<c>XUnit?</c>) box exactly as they did in the inlined
        /// setters — the underlying <c>UnitConverter.TryConvert(double, Enum, Enum, out double)</c>
        /// call is identical.
        /// </remarks>
        protected static double? ConvertValueForUnit(double? value, Enum fromUnit, Enum toUnit)
        {
            if (!value.HasValue)
            {
                return value;
            }

            // Same call the inlined setters made: TryConvert ignores its bool result and
            // uses convertedValue regardless (preserving the prior behavior exactly).
            UnitsNet.UnitConverter.TryConvert(value.Value, fromUnit, toUnit, out double convertedValue);

            return UnitSettings.RoundOnUnitChange
                ? Math.Round(convertedValue, UnitSettings.RoundingAccuracy)
                : convertedValue;
        }
    }
}