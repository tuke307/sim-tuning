// Copyright (c) 2025 tuke productions. All rights reserved.
using SimTuning.Core;
using SimTuning.Core.Models;

namespace SimTuning.Maui.UI.ViewModels
{
    /// <content>
    /// Spiegel-Eigenschaften für die direkten Motor-Felder (Vehicle.Motor.*).
    /// </content>
    public partial class VehiclesViewModelBase
    {
        /// <summary>
        /// Gets or sets the vehicle motor bohrung d.
        /// </summary>
        /// <value>The vehicle motor bohrung d.</value>
        public double? VehicleMotorBohrungD
        {
            get => Vehicle?.Motor?.BohrungD;
            set => SetMirror(Vehicle?.Motor, m => m.BohrungD = value);
        }

        /// <summary>
        /// Gets or sets the vehicle motor bohrung d unit.
        /// </summary>
        /// <value>The vehicle motor bohrung d unit.</value>
        public UnitListItem? VehicleMotorBohrungDUnit
        {
            get => LengthQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(Vehicle?.Motor?.BohrungDUnit));
            set => SetMirror(Vehicle?.Motor, m => m.BohrungDUnit = (UnitsNet.Units.LengthUnit?)value?.UnitEnumValue, nameof(VehicleMotorBohrungD));
        }

        /// <summary>
        /// Gets or sets the vehicle motor brennraum v.
        /// </summary>
        /// <value>The vehicle motor brennraum v.</value>
        public double? VehicleMotorBrennraumV
        {
            get => Vehicle?.Motor?.BrennraumV;
            set => SetMirror(Vehicle?.Motor, m => m.BrennraumV = value);
        }

        /// <summary>
        /// Gets or sets the vehicle motor brennraum v unit.
        /// </summary>
        /// <value>The vehicle motor brennraum v unit.</value>
        public UnitListItem? VehicleMotorBrennraumVUnit
        {
            get => VolumeQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(Vehicle?.Motor?.BrennraumVUnit));
            set => SetMirror(Vehicle?.Motor, m => m.BrennraumVUnit = (UnitsNet.Units.VolumeUnit?)value?.UnitEnumValue, nameof(VehicleMotorBrennraumV));
        }

        /// <summary>
        /// Gets or sets the vehicle motor deachsierung l.
        /// </summary>
        /// <value>The vehicle motor deachsierung l.</value>
        public double? VehicleMotorDeachsierungL
        {
            get => Vehicle?.Motor?.DeachsierungL;
            set => SetMirror(Vehicle?.Motor, m => m.DeachsierungL = value);
        }

        /// <summary>
        /// Gets or sets the vehicle motor deachsierung l unit.
        /// </summary>
        /// <value>The vehicle motor deachsierung l unit.</value>
        public UnitListItem? VehicleMotorDeachsierungLUnit
        {
            get => LengthQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(Vehicle?.Motor?.DeachsierungLUnit));
            set => SetMirror(Vehicle?.Motor, m => m.DeachsierungLUnit = (UnitsNet.Units.LengthUnit?)value?.UnitEnumValue, nameof(VehicleMotorDeachsierungL));
        }

        /// <summary>
        /// Gets or sets the vehicle motor heizwert u.
        /// </summary>
        /// <value>The vehicle motor heizwert u.</value>
        public double? VehicleMotorHeizwertU
        {
            get => Vehicle?.Motor?.HeizwertU;
            set => SetMirror(Vehicle?.Motor, m => m.HeizwertU = value);
        }

        /// <summary>
        /// Gets or sets the vehicle motor hub l.
        /// </summary>
        /// <value>The vehicle motor hub l.</value>
        public double? VehicleMotorHubL
        {
            get => Vehicle?.Motor?.HubL;
            set => SetMirror(Vehicle?.Motor, m => m.HubL = value);
        }

        /// <summary>
        /// Gets or sets the vehicle motor hub l unit.
        /// </summary>
        /// <value>The vehicle motor hub l unit.</value>
        public UnitListItem? VehicleMotorHubLUnit
        {
            get => LengthQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(Vehicle?.Motor?.HubLUnit));
            set => SetMirror(Vehicle?.Motor, m => m.HubLUnit = (UnitsNet.Units.LengthUnit?)value?.UnitEnumValue, nameof(VehicleMotorHubL));
        }

        /// <summary>
        /// Gets or sets the vehicle motor hubraum v.
        /// </summary>
        /// <value>The vehicle motor hubraum v.</value>
        public double? VehicleMotorHubraumV
        {
            get => Vehicle?.Motor?.HubraumV;
            set => SetMirror(Vehicle?.Motor, m => m.HubraumV = value);
        }

        /// <summary>
        /// Gets or sets the vehicle motor hubraum v unit.
        /// </summary>
        /// <value>The vehicle motor hubraum v unit.</value>
        public UnitListItem? VehicleMotorHubraumVUnit
        {
            get => VolumeQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(Vehicle?.Motor?.HubraumVUnit));
            set => SetMirror(Vehicle?.Motor, m => m.HubraumVUnit = (UnitsNet.Units.VolumeUnit?)value?.UnitEnumValue, nameof(VehicleMotorHubraumV));
        }

        /// <summary>
        /// Gets or sets the vehicle motor kolben g.
        /// </summary>
        /// <value>The vehicle motor kolben g.</value>
        public double? VehicleMotorKolbenG
        {
            get => Vehicle?.Motor?.KolbenG;
            set => SetMirror(Vehicle?.Motor, m => m.KolbenG = value);
        }

        /// <summary>
        /// Gets or sets the vehicle motor kolben g unit.
        /// </summary>
        /// <value>The vehicle motor kolben g unit.</value>
        public UnitListItem? VehicleMotorKolbenGUnit
        {
            get => SpeedQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(Vehicle?.Motor?.KolbenGUnit));
            set => SetMirror(Vehicle?.Motor, m => m.KolbenGUnit = (UnitsNet.Units.SpeedUnit?)value?.UnitEnumValue, nameof(VehicleMotorKolbenG));
        }

        /// <summary>
        /// Gets or sets the vehicle motor kurbelgehaeuse v.
        /// </summary>
        /// <value>The vehicle motor kurbelgehaeuse v.</value>
        public double? VehicleMotorKurbelgehaeuseV
        {
            get => Vehicle?.Motor?.KurbelgehaeuseV;
            set => SetMirror(Vehicle?.Motor, m => m.KurbelgehaeuseV = value);
        }

        /// <summary>
        /// Gets or sets the vehicle motor kurbelgehaeuse v unit.
        /// </summary>
        /// <value>The vehicle motor kurbelgehaeuse v unit.</value>
        public UnitListItem? VehicleMotorKurbelgehaeuseVUnit
        {
            get => VolumeQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(Vehicle?.Motor?.KurbelgehaeuseVUnit));
            set => SetMirror(Vehicle?.Motor, m => m.KurbelgehaeuseVUnit = (UnitsNet.Units.VolumeUnit?)value?.UnitEnumValue, nameof(VehicleMotorKurbelgehaeuseV));
        }

        /// <summary>
        /// Gets or sets the name of the vehicle motor.
        /// </summary>
        /// <value>The name of the vehicle motor.</value>
        public string? VehicleMotorName
        {
            get => Vehicle?.Motor?.Name;
            set => SetMirror(Vehicle?.Motor, m => m.Name = value);
        }

        /// <summary>
        /// Gets or sets the vehicle motor pleul l.
        /// </summary>
        /// <value>The vehicle motor pleul l.</value>
        public double? VehicleMotorPleulL
        {
            get => Vehicle?.Motor?.PleulL;
            set => SetMirror(Vehicle?.Motor, m => m.PleulL = value);
        }

        /// <summary>
        /// Gets or sets the vehicle motor pleul l unit.
        /// </summary>
        /// <value>The vehicle motor pleul l unit.</value>
        public UnitListItem? VehicleMotorPleulLUnit
        {
            get => LengthQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(Vehicle?.Motor?.PleulLUnit));
            set => SetMirror(Vehicle?.Motor, m => m.PleulLUnit = (UnitsNet.Units.LengthUnit?)value?.UnitEnumValue, nameof(VehicleMotorPleulL));
        }

        /// <summary>
        /// Gets or sets the vehicle motor resonanz u.
        /// </summary>
        /// <value>The vehicle motor resonanz u.</value>
        public double? VehicleMotorResonanzU
        {
            get => Vehicle?.Motor?.ResonanzU;
            set => SetMirror(Vehicle?.Motor, m => m.ResonanzU = value);
        }

        /// <summary>
        /// Gets or sets the vehicle motor verdichtung v.
        /// </summary>
        /// <value>The vehicle motor verdichtung v.</value>
        public double? VehicleMotorVerdichtungV
        {
            get => Vehicle?.Motor?.VerdichtungV;
            set => SetMirror(Vehicle?.Motor, m => m.VerdichtungV = value);
        }

        /// <summary>
        /// Gets or sets the vehicle motor zuendzeitpunkt.
        /// </summary>
        /// <value>The vehicle motor zuendzeitpunkt.</value>
        public double? VehicleMotorZuendzeitpunkt
        {
            get => Vehicle?.Motor?.Zuendzeitpunkt;
            set => SetMirror(Vehicle?.Motor, m => m.Zuendzeitpunkt = value);
        }

        /// <summary>
        /// Gets or sets the vehicle motor zylinder anz.
        /// </summary>
        /// <value>The vehicle motor zylinder anz.</value>
        public double? VehicleMotorZylinderAnz
        {
            get => Vehicle?.Motor?.ZylinderAnz;
            set => SetMirror(Vehicle?.Motor, m => m.ZylinderAnz = value);
        }
    }
}
