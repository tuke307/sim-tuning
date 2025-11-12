// Copyright (c) 2025 tuke productions. All rights reserved.
namespace SimTuning.Data.Models
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;
    using UnitsNet.Units;

    /// <summary>
    /// AuspuffModel.
    /// </summary>
    /// <seealso cref="Data.Models.BaseEntityModel" />
    public class AuspuffModel : BaseEntityModel
    {
        [NotMapped]
        private TemperatureUnit? _AbgasTUnit;

        [NotMapped]
        private SpeedUnit? _AbgasVUnit;

        [NotMapped]
        private LengthUnit? _DiffusorD1Unit;

        [NotMapped]
        private LengthUnit? _DiffusorD2Unit;

        [NotMapped]
        private LengthUnit? _DiffusorD3Unit;

        [NotMapped]
        private LengthUnit? _DiffusorDUnit;

        [NotMapped]
        private LengthUnit? _DiffusorL1Unit;

        [NotMapped]
        private LengthUnit? _DiffusorL2Unit;

        [NotMapped]
        private LengthUnit? _DiffusorL3Unit;

        [NotMapped]
        private LengthUnit? _DiffusorLUnit;

        [NotMapped]
        private LengthUnit? _EndrohrDUnit;

        [NotMapped]
        private LengthUnit? _EndrohrLUnit;

        [NotMapped]
        private LengthUnit? _GegenkonusDUnit;

        [NotMapped]
        private LengthUnit? _GegenkonusLUnit;

        [NotMapped]
        private LengthUnit? _GesamtLUnit;

        [NotMapped]
        private LengthUnit? _KruemmerDUnit;

        [NotMapped]
        private LengthUnit? _KruemmerLUnit;

        [NotMapped]
        private LengthUnit? _MittelteilDUnit;

        [NotMapped]
        private LengthUnit? _MittelteilLUnit;

        [NotMapped]
        private LengthUnit? _ResonanzLUnit;

        /// <summary>
        /// Gets the abgas t base unit.
        /// </summary>
        /// <value>The abgas t base unit.</value>
        [NotMapped]
        public static TemperatureUnit AbgasTBaseUnit { get => UnitsNet.Units.TemperatureUnit.DegreeCelsius; }

        /// <summary>
        /// Gets the abgas v base unit.
        /// </summary>
        /// <value>The abgas v base unit.</value>
        [NotMapped]
        public static UnitsNet.Units.SpeedUnit AbgasVBaseUnit { get => UnitsNet.Units.SpeedUnit.MeterPerSecond; }

        /// <summary>
        /// Gets the diffusor d1 base unit.
        /// </summary>
        /// <value>The diffusor d1 base unit.</value>
        [NotMapped]
        public static UnitsNet.Units.LengthUnit DiffusorD1BaseUnit { get => UnitsNet.Units.LengthUnit.Millimeter; }

        /// <summary>
        /// Gets the diffusor d2 base unit.
        /// </summary>
        /// <value>The diffusor d2 base unit.</value>
        [NotMapped]
        public static UnitsNet.Units.LengthUnit DiffusorD2BaseUnit { get => UnitsNet.Units.LengthUnit.Millimeter; }

        /// <summary>
        /// Gets the diffusor d3 base unit.
        /// </summary>
        /// <value>The diffusor d3 base unit.</value>
        [NotMapped]
        public static UnitsNet.Units.LengthUnit DiffusorD3BaseUnit { get => UnitsNet.Units.LengthUnit.Millimeter; }

        /// <summary>
        /// Gets the diffusor d base unit.
        /// </summary>
        /// <value>The diffusor d base unit.</value>
        [NotMapped]
        public static UnitsNet.Units.LengthUnit DiffusorDBaseUnit { get => UnitsNet.Units.LengthUnit.Millimeter; }

        /// <summary>
        /// Gets the diffusor l1 base unit.
        /// </summary>
        /// <value>The diffusor l1 base unit.</value>
        [NotMapped]
        public static UnitsNet.Units.LengthUnit DiffusorL1BaseUnit { get => UnitsNet.Units.LengthUnit.Millimeter; }

        /// <summary>
        /// Gets the diffusor l2 base unit.
        /// </summary>
        /// <value>The diffusor l2 base unit.</value>
        [NotMapped]
        public static UnitsNet.Units.LengthUnit DiffusorL2BaseUnit { get => UnitsNet.Units.LengthUnit.Millimeter; }

        /// <summary>
        /// Gets the diffusor l3 base unit.
        /// </summary>
        /// <value>The diffusor l3 base unit.</value>
        [NotMapped]
        public static UnitsNet.Units.LengthUnit DiffusorL3BaseUnit { get => UnitsNet.Units.LengthUnit.Millimeter; }

        /// <summary>
        /// Gets the diffusor l base unit.
        /// </summary>
        /// <value>The diffusor l base unit.</value>
        [NotMapped]
        public static UnitsNet.Units.LengthUnit DiffusorLBaseUnit { get => UnitsNet.Units.LengthUnit.Millimeter; }

        /// <summary>
        /// Gets the endrohr d base unit.
        /// </summary>
        /// <value>The endrohr d base unit.</value>
        [NotMapped]
        public static UnitsNet.Units.LengthUnit EndrohrDBaseUnit { get => UnitsNet.Units.LengthUnit.Millimeter; }

        /// <summary>
        /// Gets the endrohr l base unit.
        /// </summary>
        /// <value>The endrohr l base unit.</value>
        [NotMapped]
        public static UnitsNet.Units.LengthUnit EndrohrLBaseUnit { get => UnitsNet.Units.LengthUnit.Millimeter; }

        /// <summary>
        /// Gets the gegenkonus d base unit.
        /// </summary>
        /// <value>The gegenkonus d base unit.</value>
        [NotMapped]
        public static UnitsNet.Units.LengthUnit GegenkonusDBaseUnit { get => UnitsNet.Units.LengthUnit.Millimeter; }

        /// <summary>
        /// Gets the gegenkonus l base unit.
        /// </summary>
        /// <value>The gegenkonus l base unit.</value>
        [NotMapped]
        public static UnitsNet.Units.LengthUnit GegenkonusLBaseUnit { get => UnitsNet.Units.LengthUnit.Millimeter; }

        /// <summary>
        /// Gets the gesamt l base unit.
        /// </summary>
        /// <value>The gesamt l base unit.</value>
        [NotMapped]
        public static UnitsNet.Units.LengthUnit GesamtLBaseUnit { get => UnitsNet.Units.LengthUnit.Centimeter; }

        /// <summary>
        /// Gets the kruemmer d base unit.
        /// </summary>
        /// <value>The kruemmer d base unit.</value>
        [NotMapped]
        public static UnitsNet.Units.LengthUnit KruemmerDBaseUnit { get => UnitsNet.Units.LengthUnit.Millimeter; }

        /// <summary>
        /// Gets the kruemmer l base unit.
        /// </summary>
        /// <value>The kruemmer l base unit.</value>
        [NotMapped]
        public static UnitsNet.Units.LengthUnit KruemmerLBaseUnit { get => UnitsNet.Units.LengthUnit.Millimeter; }

        /// <summary>
        /// Gets the mittelteil base unit.
        /// </summary>
        /// <value>The mittelteil base unit.</value>
        [NotMapped]
        public static UnitsNet.Units.LengthUnit MittelteilBaseUnit { get => UnitsNet.Units.LengthUnit.Millimeter; }

        /// <summary>
        /// Gets the mittelteil d base unit.
        /// </summary>
        /// <value>The mittelteil d base unit.</value>
        [NotMapped]
        public static UnitsNet.Units.LengthUnit MittelteilDBaseUnit { get => UnitsNet.Units.LengthUnit.Millimeter; }

        /// <summary>
        /// Gets the mittelteil l base unit.
        /// </summary>
        /// <value>The mittelteil l base unit.</value>
        [NotMapped]
        public static UnitsNet.Units.LengthUnit MittelteilLBaseUnit { get => UnitsNet.Units.LengthUnit.Millimeter; }

        /// <summary>
        /// Gets the resonanz l base unit.
        /// </summary>
        /// <value>The resonanz l base unit.</value>
        [NotMapped]
        public static UnitsNet.Units.LengthUnit ResonanzLBaseUnit { get => UnitsNet.Units.LengthUnit.Millimeter; }

        /// <summary>
        /// Gets or sets the abgas t.
        /// </summary>
        /// <value>The abgas t.</value>
        public double? AbgasT { get; set; }

        /// <summary>
        /// Gets or sets the abgas t unit.
        /// </summary>
        /// <value>The abgas t unit.</value>
        [NotMapped]
        public UnitsNet.Units.TemperatureUnit? AbgasTUnit
        {
            get => this._AbgasTUnit ?? AbgasTBaseUnit;
            set
            {
                if (this.AbgasT.HasValue)
                {
                    UnitsNet.UnitConverter.TryConvert(
               this.AbgasT.Value,
               this.AbgasTUnit,
               value,
               out double convertedValue);

                    if (UnitSettings.RoundOnUnitChange)
                    {
                        this.AbgasT = Math.Round(convertedValue, UnitSettings.RoundingAccuracy);
                    }
                    else
                    {
                        this.AbgasT = convertedValue;
                    }
                }

                this._AbgasTUnit = value;
            }
        }

        /// <summary>
        /// Gets or sets the abgas v.
        /// </summary>
        /// <value>The abgas v.</value>
        public double? AbgasV { get; set; }

        /// <summary>
        /// Gets or sets the abgas v unit.
        /// </summary>
        /// <value>The abgas v unit.</value>
        [NotMapped]
        public UnitsNet.Units.SpeedUnit? AbgasVUnit
        {
            get => this._AbgasVUnit ?? AbgasVBaseUnit;
            set
            {
                if (this.AbgasV.HasValue)
                {
                    UnitsNet.UnitConverter.TryConvert(
                                    this.AbgasV.Value,
                                    this.AbgasVUnit,
                                    value,
                                    out double convertedValue);

                    if (UnitSettings.RoundOnUnitChange)
                    {
                        this.AbgasV = Math.Round(convertedValue, UnitSettings.RoundingAccuracy);
                    }
                    else
                    {
                        this.AbgasV = convertedValue;
                    }
                }

                this._AbgasVUnit = value;
            }
        }

        /// <summary>
        /// Gets or sets the auslass.
        /// </summary>
        /// <value>The auslass.</value>
        [ForeignKey("AuslassId")]
        public virtual AuslassModel Auslass { get; set; }

        /// <summary>
        /// Gets or sets the auslass identifier.
        /// </summary>
        /// <value>The auslass identifier.</value>
        public int AuslassId { get; set; }

        /// <summary>
        /// Gets or sets the diffusor d.
        /// </summary>
        /// <value>The diffusor d.</value>
        public double? DiffusorD { get; set; }

        /// <summary>
        /// Gets or sets the diffusor d1.
        /// </summary>
        /// <value>The diffusor d1.</value>
        public double? DiffusorD1 { get; set; }

        /// <summary>
        /// Gets or sets the diffusor d1 unit.
        /// </summary>
        /// <value>The diffusor d1 unit.</value>
        [NotMapped]
        public UnitsNet.Units.LengthUnit? DiffusorD1Unit
        {
            get => this._DiffusorD1Unit ?? DiffusorD1BaseUnit;
            set
            {
                if (this.DiffusorD1.HasValue)
                {
                    UnitsNet.UnitConverter.TryConvert(
                this.DiffusorD1.Value,
                this.DiffusorD1Unit,
                value,
                out double convertedValue);

                    if (UnitSettings.RoundOnUnitChange)
                    {
                        this.DiffusorD1 = Math.Round(convertedValue, UnitSettings.RoundingAccuracy);
                    }
                    else
                    {
                        this.DiffusorD1 = convertedValue;
                    }
                }

                this._DiffusorD1Unit = value;
            }
        }

        /// <summary>
        /// Gets or sets the diffusor d2.
        /// </summary>
        /// <value>The diffusor d2.</value>
        public double? DiffusorD2 { get; set; }

        /// <summary>
        /// Gets or sets the diffusor d2 unit.
        /// </summary>
        /// <value>The diffusor d2 unit.</value>
        [NotMapped]
        public UnitsNet.Units.LengthUnit? DiffusorD2Unit
        {
            get => this._DiffusorD2Unit ?? DiffusorD2BaseUnit;
            set
            {
                if (this.DiffusorD2.HasValue)
                {
                    UnitsNet.UnitConverter.TryConvert(
               this.DiffusorD2.Value,
               this.DiffusorD2Unit,
               value,
               out double convertedValue);

                    if (UnitSettings.RoundOnUnitChange)
                    {
                        this.DiffusorD2 = Math.Round(convertedValue, UnitSettings.RoundingAccuracy);
                    }
                    else
                    {
                        this.DiffusorD2 = convertedValue;
                    }
                }

                this._DiffusorD2Unit = value;
            }
        }

        /// <summary>
        /// Gets or sets the diffusor d3.
        /// </summary>
        /// <value>The diffusor d3.</value>
        public double? DiffusorD3 { get; set; }

        /// <summary>
        /// Gets or sets the diffusor d3 unit.
        /// </summary>
        /// <value>The diffusor d3 unit.</value>
        [NotMapped]
        public UnitsNet.Units.LengthUnit? DiffusorD3Unit
        {
            get => this._DiffusorD3Unit ?? DiffusorD3BaseUnit;
            set
            {
                if (this.DiffusorD3.HasValue)
                {
                    UnitsNet.UnitConverter.TryConvert(
                this.DiffusorD3.Value,
                this.DiffusorD3Unit,
                value,
                out double convertedValue);

                    if (UnitSettings.RoundOnUnitChange)
                    {
                        this.DiffusorD3 = Math.Round(convertedValue, UnitSettings.RoundingAccuracy);
                    }
                    else
                    {
                        this.DiffusorD3 = convertedValue;
                    }
                }

                this._DiffusorD3Unit = value;
            }
        }

        /// <summary>
        /// Gets or sets the diffusor d unit.
        /// </summary>
        /// <value>The diffusor d unit.</value>
        [NotMapped]
        public UnitsNet.Units.LengthUnit? DiffusorDUnit
        {
            get => this._DiffusorDUnit ?? DiffusorDBaseUnit;
            set
            {
                if (this.DiffusorD.HasValue)
                {
                    UnitsNet.UnitConverter.TryConvert(
                                    this.DiffusorD.Value,
                                    this.DiffusorDUnit,
                                    value,
                                    out double convertedValue);

                    if (UnitSettings.RoundOnUnitChange)
                    {
                        this.DiffusorD = Math.Round(convertedValue, UnitSettings.RoundingAccuracy);
                    }
                    else
                    {
                        this.DiffusorD = convertedValue;
                    }
                }

                this._DiffusorDUnit = value;
            }
        }

        /// <summary>
        /// Gets or sets the diffusor l.
        /// </summary>
        /// <value>The diffusor l.</value>
        public double? DiffusorL { get; set; }

        /// <summary>
        /// Gets or sets the diffusor l1.
        /// </summary>
        /// <value>The diffusor l1.</value>
        public double? DiffusorL1 { get; set; }

        /// <summary>
        /// Gets or sets the diffusor l1 unit.
        /// </summary>
        /// <value>The diffusor l1 unit.</value>
        [NotMapped]
        public UnitsNet.Units.LengthUnit? DiffusorL1Unit
        {
            get => this._DiffusorL1Unit ?? DiffusorL1BaseUnit;
            set
            {
                if (this.DiffusorL1.HasValue)
                {
                    UnitsNet.UnitConverter.TryConvert(
                this.DiffusorL1.Value,
                this.DiffusorL1Unit,
                value,
                out double convertedValue);

                    if (UnitSettings.RoundOnUnitChange)
                    {
                        this.DiffusorL1 = Math.Round(convertedValue, UnitSettings.RoundingAccuracy);
                    }
                    else
                    {
                        this.DiffusorL1 = convertedValue;
                    }
                }

                this._DiffusorL1Unit = value;
            }
        }

        /// <summary>
        /// Gets or sets the diffusor l2.
        /// </summary>
        /// <value>The diffusor l2.</value>
        public double? DiffusorL2 { get; set; }

        /// <summary>
        /// Gets or sets the diffusor l2 unit.
        /// </summary>
        /// <value>The diffusor l2 unit.</value>
        [NotMapped]
        public UnitsNet.Units.LengthUnit? DiffusorL2Unit
        {
            get => this._DiffusorL2Unit ?? DiffusorL2BaseUnit;
            set
            {
                if (this.DiffusorL2.HasValue)
                {
                    UnitsNet.UnitConverter.TryConvert(
                this.DiffusorL2.Value,
                this.DiffusorL2Unit,
                value,
                out double convertedValue);

                    if (UnitSettings.RoundOnUnitChange)
                    {
                        this.DiffusorL2 = Math.Round(convertedValue, UnitSettings.RoundingAccuracy);
                    }
                    else
                    {
                        this.DiffusorL2 = convertedValue;
                    }
                }

                this._DiffusorL2Unit = value;
            }
        }

        /// <summary>
        /// Gets or sets the diffusor l3.
        /// </summary>
        /// <value>The diffusor l3.</value>
        public double? DiffusorL3 { get; set; }

        /// <summary>
        /// Gets or sets the diffusor l3 unit.
        /// </summary>
        /// <value>The diffusor l3 unit.</value>
        [NotMapped]
        public UnitsNet.Units.LengthUnit? DiffusorL3Unit
        {
            get => this._DiffusorL3Unit ?? DiffusorL3BaseUnit;
            set
            {
                if (this.DiffusorL3.HasValue)
                {
                    UnitsNet.UnitConverter.TryConvert(
               this.DiffusorL3.Value,
               this.DiffusorL3Unit,
               value,
               out double convertedValue);

                    if (UnitSettings.RoundOnUnitChange)
                    {
                        this.DiffusorL3 = Math.Round(convertedValue, UnitSettings.RoundingAccuracy);
                    }
                    else
                    {
                        this.DiffusorL3 = convertedValue;
                    }
                }

                this._DiffusorL3Unit = value;
            }
        }

        /// <summary>
        /// Gets or sets the diffusor l unit.
        /// </summary>
        /// <value>The diffusor l unit.</value>
        [NotMapped]
        public UnitsNet.Units.LengthUnit? DiffusorLUnit
        {
            get => this._DiffusorLUnit ?? DiffusorLBaseUnit;
            set
            {
                if (this.DiffusorL.HasValue)
                {
                    UnitsNet.UnitConverter.TryConvert(
               this.DiffusorL.Value,
               this.DiffusorLUnit,
               value,
               out double convertedValue);

                    if (UnitSettings.RoundOnUnitChange)
                    {
                        this.DiffusorL = Math.Round(convertedValue, UnitSettings.RoundingAccuracy);
                    }
                    else
                    {
                        this.DiffusorL = convertedValue;
                    }
                }

                this._DiffusorLUnit = value;
            }
        }

        /// <summary>
        /// Gets or sets the diffusor stage.
        /// </summary>
        /// <value>The diffusor stage.</value>
        public int DiffusorStage { get; set; }

        /// <summary>
        /// Gets or sets the diffusor w.
        /// </summary>
        /// <value>The diffusor w.</value>
        public double? DiffusorW { get; set; }

        /// <summary>
        /// Gets or sets the diffusor w1.
        /// </summary>
        /// <value>The diffusor w1.</value>
        public double? DiffusorW1 { get; set; }

        /// <summary>
        /// Gets or sets the diffusor w2.
        /// </summary>
        /// <value>The diffusor w2.</value>
        public double? DiffusorW2 { get; set; }

        /// <summary>
        /// Gets or sets the diffusor w3.
        /// </summary>
        /// <value>The diffusor w3.</value>
        public double? DiffusorW3 { get; set; }

        /// <summary>
        /// Gets or sets the endrohr d.
        /// </summary>
        /// <value>The endrohr d.</value>
        public double? EndrohrD { get; set; }

        /// <summary>
        /// Gets or sets the endrohr d unit.
        /// </summary>
        /// <value>The endrohr d unit.</value>
        [NotMapped]
        public UnitsNet.Units.LengthUnit? EndrohrDUnit
        {
            get => this._EndrohrDUnit ?? EndrohrDBaseUnit;
            set
            {
                if (this.EndrohrD.HasValue)
                {
                    UnitsNet.UnitConverter.TryConvert(
                this.EndrohrD.Value,
                this.EndrohrDUnit,
                value,
                out double convertedValue);

                    if (UnitSettings.RoundOnUnitChange)
                    {
                        this.EndrohrD = Math.Round(convertedValue, UnitSettings.RoundingAccuracy);
                    }
                    else
                    {
                        this.EndrohrD = convertedValue;
                    }
                }

                this._EndrohrDUnit = value;
            }
        }

        /// <summary>
        /// Gets or sets the endrohr l.
        /// </summary>
        /// <value>The endrohr l.</value>
        public double? EndrohrL { get; set; }

        /// <summary>
        /// Gets or sets the endrohr l unit.
        /// </summary>
        /// <value>The endrohr l unit.</value>
        [NotMapped]
        public UnitsNet.Units.LengthUnit? EndrohrLUnit
        {
            get => this._EndrohrLUnit ?? EndrohrLBaseUnit;
            set
            {
                if (this.EndrohrL.HasValue)
                {
                    UnitsNet.UnitConverter.TryConvert(
                                    this.EndrohrL.Value,
                                    this.EndrohrLUnit,
                                    value,
                                    out double convertedValue);

                    if (UnitSettings.RoundOnUnitChange)
                    {
                        this.EndrohrL = Math.Round(convertedValue, UnitSettings.RoundingAccuracy);
                    }
                    else
                    {
                        this.EndrohrL = convertedValue;
                    }
                }

                this._EndrohrLUnit = value;
            }
        }

        /// <summary>
        /// Gets or sets the gegenkonus d.
        /// </summary>
        /// <value>The gegenkonus d.</value>
        public double? GegenkonusD { get; set; }

        /// <summary>
        /// Gets or sets the gegenkonus d unit.
        /// </summary>
        /// <value>The gegenkonus d unit.</value>
        [NotMapped]
        public UnitsNet.Units.LengthUnit? GegenkonusDUnit
        {
            get => this._GegenkonusDUnit ?? GegenkonusDBaseUnit;
            set
            {
                if (this.GegenkonusD.HasValue)
                {
                    UnitsNet.UnitConverter.TryConvert(
                this.GegenkonusD.Value,
                this.GegenkonusDUnit,
                value,
                out double convertedValue);

                    if (UnitSettings.RoundOnUnitChange)
                    {
                        this.GegenkonusD = Math.Round(convertedValue, UnitSettings.RoundingAccuracy);
                    }
                    else
                    {
                        this.GegenkonusD = convertedValue;
                    }
                }

                this._GegenkonusDUnit = value;
            }
        }

        /// <summary>
        /// Gets or sets the gegenkonus l.
        /// </summary>
        /// <value>The gegenkonus l.</value>
        public double? GegenkonusL { get; set; }

        /// <summary>
        /// Gets or sets the gegenkonus l unit.
        /// </summary>
        /// <value>The gegenkonus l unit.</value>
        [NotMapped]
        public UnitsNet.Units.LengthUnit? GegenkonusLUnit
        {
            get => this._GegenkonusLUnit ?? GegenkonusLBaseUnit;
            set
            {
                if (this.GegenkonusL.HasValue)
                {
                    UnitsNet.UnitConverter.TryConvert(
                                    this.GegenkonusL.Value,
                                    this.GegenkonusLUnit,
                                    value,
                                    out double convertedValue);

                    if (UnitSettings.RoundOnUnitChange)
                    {
                        this.GegenkonusL = Math.Round(convertedValue, UnitSettings.RoundingAccuracy);
                    }
                    else
                    {
                        this.GegenkonusL = convertedValue;
                    }
                }

                this._GegenkonusLUnit = value;
            }
        }

        /// <summary>
        /// Gets or sets the gegen konus w.
        /// </summary>
        /// <value>The gegen konus w.</value>
        public double? GegenKonusW { get; set; }

        /// <summary>
        /// Gets or sets the gesamt l.
        /// </summary>
        /// <value>The gesamt l.</value>
        public double? GesamtL { get; set; }

        /// <summary>
        /// Gets or sets the gesamt l unit.
        /// </summary>
        /// <value>The gesamt l unit.</value>
        [NotMapped]
        public UnitsNet.Units.LengthUnit? GesamtLUnit
        {
            get => this._GesamtLUnit ?? GesamtLBaseUnit;
            set
            {
                if (this.GesamtL.HasValue)
                {
                    UnitsNet.UnitConverter.TryConvert(
               this.GesamtL.Value,
               this.GesamtLUnit,
               value,
               out double convertedValue);

                    if (UnitSettings.RoundOnUnitChange)
                    {
                        this.GesamtL = Math.Round(convertedValue, UnitSettings.RoundingAccuracy);
                    }
                    else
                    {
                        this.GesamtL = convertedValue;
                    }
                }

                this._GesamtLUnit = value;
            }
        }

        /// <summary>
        /// Gets or sets the kruemmer d.
        /// </summary>
        /// <value>The kruemmer d.</value>
        public double? KruemmerD { get; set; }

        /// <summary>
        /// Gets or sets the kruemmer d unit.
        /// </summary>
        /// <value>The kruemmer d unit.</value>
        [NotMapped]
        public UnitsNet.Units.LengthUnit? KruemmerDUnit
        {
            get => this._KruemmerDUnit ?? KruemmerDBaseUnit;
            set
            {
                if (this.KruemmerD.HasValue)
                {
                    UnitsNet.UnitConverter.TryConvert(
                    this.KruemmerD.Value,
                    this.KruemmerDUnit,
                    value,
                    out double convertedValue);

                    if (UnitSettings.RoundOnUnitChange)
                    {
                        this.KruemmerD = Math.Round(convertedValue, UnitSettings.RoundingAccuracy);
                    }
                    else
                    {
                        this.KruemmerD = convertedValue;
                    }
                }

                this._KruemmerDUnit = value;
            }
        }

        /// <summary>
        /// Gets or sets the kruemmer f.
        /// </summary>
        /// <value>The kruemmer f.</value>
        public double? KruemmerF { get; set; }

        /// <summary>
        /// Gets or sets the kruemmer l.
        /// </summary>
        /// <value>The kruemmer l.</value>
        public double? KruemmerL { get; set; }

        /// <summary>
        /// Gets or sets the kruemmer l unit.
        /// </summary>
        /// <value>The kruemmer l unit.</value>
        [NotMapped]
        public UnitsNet.Units.LengthUnit? KruemmerLUnit
        {
            get => this._KruemmerLUnit ?? KruemmerLBaseUnit;
            set
            {
                if (this.KruemmerL.HasValue)
                {
                    UnitsNet.UnitConverter.TryConvert(
               this.KruemmerL.Value,
               this.KruemmerLUnit,
               value,
               out double convertedValue);

                    if (UnitSettings.RoundOnUnitChange)
                    {
                        this.KruemmerL = Math.Round(convertedValue, UnitSettings.RoundingAccuracy);
                    }
                    else
                    {
                        this.KruemmerL = convertedValue;
                    }
                }

                this._KruemmerLUnit = value;
            }
        }

        /// <summary>
        /// Gets or sets the kruemmer w.
        /// </summary>
        /// <value>The kruemmer w.</value>
        public double? KruemmerW { get; set; }

        /// <summary>
        /// Gets or sets the mittelteil d.
        /// </summary>
        /// <value>The mittelteil d.</value>
        public double? MittelteilD { get; set; }

        /// <summary>
        /// Gets or sets the mittelteil d unit.
        /// </summary>
        /// <value>The mittelteil d unit.</value>
        [NotMapped]
        public UnitsNet.Units.LengthUnit? MittelteilDUnit
        {
            get => this._MittelteilDUnit ?? MittelteilDBaseUnit;
            set
            {
                if (this.MittelteilD.HasValue)
                {
                    UnitsNet.UnitConverter.TryConvert(
               this.MittelteilD.Value,
               this.MittelteilDUnit,
               value,
               out double convertedValue);

                    if (UnitSettings.RoundOnUnitChange)
                    {
                        this.MittelteilD = Math.Round(convertedValue, UnitSettings.RoundingAccuracy);
                    }
                    else
                    {
                        this.MittelteilD = convertedValue;
                    }
                }

                this._MittelteilDUnit = value;
            }
        }

        /// <summary>
        /// Gets or sets the mittelteil f.
        /// </summary>
        /// <value>The mittelteil f.</value>
        public double? MittelteilF { get; set; }

        /// <summary>
        /// Gets or sets the mittelteil l.
        /// </summary>
        /// <value>The mittelteil l.</value>
        public double? MittelteilL { get; set; }

        /// <summary>
        /// Gets or sets the mittelteil l unit.
        /// </summary>
        /// <value>The mittelteil l unit.</value>
        [NotMapped]
        public UnitsNet.Units.LengthUnit? MittelteilLUnit
        {
            get => this._MittelteilLUnit ?? MittelteilLBaseUnit;
            set
            {
                if (this.MittelteilL.HasValue)
                {
                    UnitsNet.UnitConverter.TryConvert(
                this.MittelteilL.Value,
                this.MittelteilLUnit,
                value,
                out double convertedValue);

                    if (UnitSettings.RoundOnUnitChange)
                    {
                        this.MittelteilL = Math.Round(convertedValue, UnitSettings.RoundingAccuracy);
                    }
                    else
                    {
                        this.MittelteilL = convertedValue;
                    }
                }

                this._MittelteilLUnit = value;
            }
        }

        /// <summary>
        /// Gets or sets the resonanz l.
        /// </summary>
        /// <value>The resonanz l.</value>
        public double? ResonanzL { get; set; }

        /// <summary>
        /// Gets or sets the resonanz l unit.
        /// </summary>
        /// <value>The resonanz l unit.</value>
        [NotMapped]
        public UnitsNet.Units.LengthUnit? ResonanzLUnit
        {
            get => this._ResonanzLUnit ?? ResonanzLBaseUnit;
            set
            {
                if (this.ResonanzL.HasValue)
                {
                    UnitsNet.UnitConverter.TryConvert(
                this.ResonanzL.Value,
                this.ResonanzLUnit,
                value,
                out double convertedValue);

                    if (UnitSettings.RoundOnUnitChange)
                    {
                        this.ResonanzL = Math.Round(convertedValue, UnitSettings.RoundingAccuracy);
                    }
                    else
                    {
                        this.ResonanzL = convertedValue;
                    }
                }

                this._ResonanzLUnit = value;
            }
        }
    }
}