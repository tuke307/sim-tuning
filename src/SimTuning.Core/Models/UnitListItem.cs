// Copyright (c) 2025 tuke productions. All rights reserved.
namespace SimTuning.Core
{
    using System;
    using UnitsNet;

    /// <summary>
    /// Represents an item in the from/to unit listboxes. Provides a formatted <see
    /// cref="Text" /> property as well as holding on to the original unit enum value, in
    /// order to perform the unit conversion.
    /// </summary>
    public sealed class UnitListItem
    {
        /// <summary>
        /// Gets the abbreviation.
        /// </summary>
        /// <value>The abbreviation.</value>
        public string Abbreviation { get; }

        /// <summary>
        /// Gets the text.
        /// </summary>
        /// <value>The text.</value>
        public string Text { get; }

        /// <summary>
        /// Gets the type of the unit enum.
        /// </summary>
        /// <value>The type of the unit enum.</value>
        public Type UnitEnumType { get; }

        /// <summary>
        /// Gets the unit enum value.
        /// </summary>
        /// <value>The unit enum value.</value>
        public Enum UnitEnumValue { get; }

        /// <summary>
        /// Gets the unit enum value int.
        /// </summary>
        /// <value>The unit enum value int.</value>
        public int UnitEnumValueInt { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnitListItem" /> class.
        /// </summary>
        /// <param name="val">The value.</param>
        public UnitListItem(Enum val)
        {
            this.UnitEnumValue = val;
            this.UnitEnumValueInt = Convert.ToInt32(val);
            this.UnitEnumType = val.GetType();
            this.Abbreviation = UnitAbbreviationsCache.Default.GetDefaultAbbreviation(this.UnitEnumType, this.UnitEnumValueInt);

            this.Text = $"{val} [{this.Abbreviation}]";
        }
    }
}