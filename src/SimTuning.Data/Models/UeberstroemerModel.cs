// Copyright (c) 2025 tuke productions. All rights reserved.
namespace SimTuning.Data.Models
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;
    using UnitsNet.Units;

    /// <summary>
    /// ÜberströmerModel.
    /// </summary>
    /// <seealso cref="Data.Models.BaseEntityModel" />
    public class UeberstroemerModel : BaseEntityModel
    {
        /// <summary>
        /// Gets the breite b base unit.
        /// </summary>
        /// <value>The breite b base unit.</value>
        [NotMapped]
        public static UnitsNet.Units.LengthUnit BreiteBBaseUnit { get => UnitsNet.Units.LengthUnit.Millimeter; }

        /// <summary>
        /// Gets the flaeche a base unit.
        /// </summary>
        /// <value>The flaeche a base unit.</value>
        [NotMapped]
        public static UnitsNet.Units.AreaUnit FlaecheABaseUnit { get => UnitsNet.Units.AreaUnit.SquareMillimeter; }

        /// <summary>
        /// Gets the hoehe h base unit.
        /// </summary>
        /// <value>The hoehe h base unit.</value>
        [NotMapped]
        public static UnitsNet.Units.LengthUnit HoeheHBaseUnit { get => UnitsNet.Units.LengthUnit.Millimeter; }

        /// <summary>
        /// Gets or sets the anzahl.
        /// </summary>
        /// <value>The anzahl.</value>
        public double? Anzahl { get; set; }

        /// <summary>
        /// Gets or sets the breite.
        /// </summary>
        /// <value>The breite.</value>
        public double? BreiteB { get; set; }

        /// <summary>
        /// Gets or sets the breite b unit.
        /// </summary>
        /// <value>The breite b unit.</value>
        [NotMapped]
        public UnitsNet.Units.LengthUnit? BreiteBUnit
        {
            get => this._BreiteBUnit ?? BreiteBBaseUnit;
            set
            {
                if (this.BreiteB.HasValue)
                {
                    UnitsNet.UnitConverter.TryConvert(
                this.BreiteB.Value,
                this.BreiteBUnit,
                value,
                out double convertedValue);

                    if (UnitSettings.RoundOnUnitChange)
                    {
                        this.BreiteB = Math.Round(convertedValue, UnitSettings.RoundingAccuracy);
                    }
                    else
                    {
                        this.BreiteB = convertedValue;
                    }
                }

                this._BreiteBUnit = value;
            }
        }

        /// <summary>
        /// Gets or sets the flaeche.
        /// </summary>
        /// <value>The flaeche.</value>
        public double? FlaecheA { get; set; }

        /// <summary>
        /// Gets or sets the flaeche a unit.
        /// </summary>
        /// <value>The flaeche a unit.</value>
        [NotMapped]
        public UnitsNet.Units.AreaUnit? FlaecheAUnit
        {
            get => this._FlaecheAUnit ?? FlaecheABaseUnit;
            set
            {
                if (this.FlaecheA.HasValue)
                {
                    UnitsNet.UnitConverter.TryConvert(
                this.FlaecheA.Value,
                this.FlaecheAUnit,
                value,
                out double convertedValue);

                    if (UnitSettings.RoundOnUnitChange)
                    {
                        this.FlaecheA = Math.Round(convertedValue, UnitSettings.RoundingAccuracy);
                    }
                    else
                    {
                        this.FlaecheA = convertedValue;
                    }
                }

                this._FlaecheAUnit = value;
            }
        }

        /// <summary>
        /// Gets or sets the hoehe.
        /// </summary>
        /// <value>The hoehe.</value>
        public double? HoeheH { get; set; }

        /// <summary>
        /// Gets or sets the hoehe h unit.
        /// </summary>
        /// <value>The hoehe h unit.</value>
        [NotMapped]
        public UnitsNet.Units.LengthUnit? HoeheHUnit
        {
            get => this._HoeheHUnit ?? HoeheHBaseUnit;
            set
            {
                if (this.HoeheH.HasValue)
                {
                    UnitsNet.UnitConverter.TryConvert(
               this.HoeheH.Value,
               this.HoeheHUnit,
               value,
               out double convertedValue);

                    if (UnitSettings.RoundOnUnitChange)
                    {
                        this.HoeheH = Math.Round(convertedValue, UnitSettings.RoundingAccuracy);
                    }
                    else
                    {
                        this.HoeheH = convertedValue;
                    }
                }

                this._HoeheHUnit = value;
            }
        }

        /// <summary>
        /// Gets or sets the motor.
        /// </summary>
        /// <value>The motor.</value>
        [ForeignKey("MotorId")]
        public virtual MotorModel Motor { get; set; }

        /// <summary>
        /// Gets or sets the motor identifier.
        /// </summary>
        /// <value>The motor identifier.</value>
        public int MotorId { get; set; }

        /// <summary>
        /// Gets or sets the steuerzeit sz.
        /// </summary>
        /// <value>The steuerzeit sz.</value>
        public double? SteuerzeitSZ { get; set; }

        /// <summary>
        /// Gets or sets the breite b unit.
        /// </summary>
        /// <value>The breite b unit.</value>
        [NotMapped]
        private LengthUnit? _BreiteBUnit { get; set; }

        /// <summary>
        /// Gets or sets the flaeche a unit.
        /// </summary>
        /// <value>The flaeche a unit.</value>
        [NotMapped]
        private AreaUnit? _FlaecheAUnit { get; set; }

        /// <summary>
        /// Gets or sets the hoehe h unit.
        /// </summary>
        /// <value>The hoehe h unit.</value>
        [NotMapped]
        private LengthUnit? _HoeheHUnit { get; set; }
    }
}