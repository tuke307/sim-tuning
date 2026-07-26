// Copyright (c) 2025 tuke productions. All rights reserved.
using SimTuning.Core;
using SimTuning.Core.Models;

namespace SimTuning.Maui.UI.ViewModels
{
    /// <content>
    /// Spiegel-Eigenschaften für die Auslass-Felder (Vehicle.Motor.Auslass.*).
    /// </content>
    public partial class VehiclesViewModelBase
    {
        /// <summary>
        /// Gets or sets the vehicle motor auslass breite b.
        /// </summary>
        /// <value>The vehicle motor auslass breite b.</value>
        public double? VehicleMotorAuslassBreiteB
        {
            get => Vehicle?.Motor?.Auslass?.BreiteB;
            set => SetMirror(Vehicle?.Motor?.Auslass, a => a.BreiteB = value);
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass breite b unit.
        /// </summary>
        /// <value>The vehicle motor auslass breite b unit.</value>
        public UnitListItem? VehicleMotorAuslassBreiteBUnit
        {
            get => LengthQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(Vehicle?.Motor?.Auslass?.BreiteBUnit));
            set => SetMirror(Vehicle?.Motor?.Auslass, a => a.BreiteBUnit = (UnitsNet.Units.LengthUnit?)value?.UnitEnumValue, nameof(VehicleMotorAuslassBreiteB));
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass flaeche a.
        /// </summary>
        /// <value>The vehicle motor auslass flaeche a.</value>
        public double? VehicleMotorAuslassFlaecheA
        {
            get => Vehicle?.Motor?.Auslass?.FlaecheA;
            set => SetMirror(Vehicle?.Motor?.Auslass, a => a.FlaecheA = value);
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass flaeche a unit.
        /// </summary>
        /// <value>The vehicle motor auslass flaeche a unit.</value>
        public UnitListItem? VehicleMotorAuslassFlaecheAUnit
        {
            get => AreaQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(Vehicle?.Motor?.Auslass?.FlaecheAUnit));
            set => SetMirror(Vehicle?.Motor?.Auslass, a => a.FlaecheAUnit = ((UnitsNet.Units.AreaUnit?)value?.UnitEnumValue).GetValueOrDefault(), nameof(VehicleMotorAuslassFlaecheA));
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass hoehe h.
        /// </summary>
        /// <value>The vehicle motor auslass hoehe h.</value>
        public double? VehicleMotorAuslassHoeheH
        {
            get => Vehicle?.Motor?.Auslass?.HoeheH;
            set => SetMirror(Vehicle?.Motor?.Auslass, a => a.HoeheH = value);
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass hoehe h unit.
        /// </summary>
        /// <value>The vehicle motor auslass hoehe h unit.</value>
        public UnitListItem? VehicleMotorAuslassHoeheHUnit
        {
            get => LengthQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(Vehicle?.Motor?.Auslass?.HoeheHUnit));
            set => SetMirror(Vehicle?.Motor?.Auslass, a => a.HoeheHUnit = ((UnitsNet.Units.LengthUnit?)value?.UnitEnumValue).GetValueOrDefault(), nameof(VehicleMotorAuslassHoeheH));
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass laenge l.
        /// </summary>
        /// <value>The vehicle motor auslass laenge l.</value>
        public double? VehicleMotorAuslassLaengeL
        {
            get => Vehicle?.Motor?.Auslass?.LaengeL;
            set => SetMirror(Vehicle?.Motor?.Auslass, a => a.LaengeL = value);
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass laenge l unit.
        /// </summary>
        /// <value>The vehicle motor auslass laenge l unit.</value>
        public UnitListItem? VehicleMotorAuslassLaengeLUnit
        {
            get => LengthQuantityUnits.SingleOrDefault(x => x.UnitEnumValue.Equals(Vehicle?.Motor?.Auslass?.LaengeLUnit));
            set => SetMirror(Vehicle?.Motor?.Auslass, a => a.LaengeLUnit = ((UnitsNet.Units.LengthUnit?)value?.UnitEnumValue).GetValueOrDefault(), nameof(VehicleMotorAuslassLaengeL));
        }

        /// <summary>
        /// Gets or sets the vehicle motor auslass steuerzeit sz.
        /// </summary>
        /// <value>The vehicle motor auslass steuerzeit sz.</value>
        public double? VehicleMotorAuslassSteuerzeitSZ
        {
            get => Vehicle?.Motor?.Auslass?.SteuerzeitSZ;
            set => SetMirror(Vehicle?.Motor?.Auslass, a => a.SteuerzeitSZ = value);
        }
    }
}
