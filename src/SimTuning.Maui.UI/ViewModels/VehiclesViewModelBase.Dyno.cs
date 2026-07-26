// Copyright (c) 2025 tuke productions. All rights reserved.
using System.Collections.ObjectModel;

namespace SimTuning.Maui.UI.ViewModels
{
    /// <content>
    /// Spiegel-Eigenschaften für die Dyno-Felder (Vehicle.Dyno.*).
    /// </content>
    public partial class VehiclesViewModelBase
    {
        /// <summary>
        /// Gets the vehicle dyno audio.
        /// </summary>
        /// <value>The vehicle dyno audio.</value>
        public ObservableCollection<Data.Models.DrehzahlModel> VehicleDynoAudio
        {
            get => Vehicle?.Dyno?.Drehzahl is ObservableCollection<Data.Models.DrehzahlModel> drehzahl ? new ObservableCollection<Data.Models.DrehzahlModel>(drehzahl) : new ObservableCollection<Data.Models.DrehzahlModel>();
        }

        /// <summary>
        /// Gets or sets the vehicle dyno beschreibung.
        /// </summary>
        /// <value>The vehicle dyno beschreibung.</value>
        public string? VehicleDynoBeschreibung
        {
            get => Vehicle?.Dyno?.Beschreibung;
            set => SetMirror(Vehicle?.Dyno, d => d.Beschreibung = value);
        }

        /// <summary>
        /// Gets the vehicle dyno dyno ps.
        /// </summary>
        /// <value>The vehicle dyno dyno ps.</value>
        public ObservableCollection<Data.Models.DynoPsModel> VehicleDynoDynoPS
        {
            get => Vehicle?.Dyno?.DynoPS is ObservableCollection<Data.Models.DynoPsModel> dynoPS ? new ObservableCollection<Data.Models.DynoPsModel>(dynoPS) : new ObservableCollection<Data.Models.DynoPsModel>();
        }

        /// <summary>
        /// Gets or sets the name of the vehicle dyno.
        /// </summary>
        /// <value>The name of the vehicle dyno.</value>
        public string? VehicleDynoName
        {
            get => Vehicle?.Dyno?.Name;
            set => SetMirror(Vehicle?.Dyno, d => d.Name = value);
        }
    }
}
