// Copyright (c) 2025 tuke productions. All rights reserved.
namespace SimTuning.Data.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// DynoModel.
    /// </summary>
    /// <seealso cref="Data.Models.BaseEntityModel" />
    public class DynoModel : BaseEntityModel
    {
        /// <summary>
        /// Gets or sets the active.
        /// </summary>
        /// <value>The active.</value>
        public bool? Active { get; set; }

        /// <summary>
        /// Gets or sets the ausrollen location.
        /// </summary>
        /// <value>The ausrollen location.</value>
        public virtual IList<AusrollenModel> Ausrollen { get; set; }

        /// <summary>
        /// Gets or sets the beschreibung.
        /// </summary>
        /// <value>The beschreibung.</value>
        public string Beschreibung { get; set; }

        /// <summary>
        /// Gets or sets the audio.
        /// </summary>
        /// <value>The audio.</value>
        public virtual IList<DrehzahlModel> Drehzahl { get; set; }

        /// <summary>
        /// Gets or sets the dyno nm.
        /// </summary>
        /// <value>The dyno nm.</value>
        [Obsolete("obsolete", true)]
        public virtual IList<DynoNmModel> DynoNm { get; set; }

        /// <summary>
        /// Gets or sets the dyno ps.
        /// </summary>
        /// <value>The dyno ps.</value>
        public virtual IList<DynoPsModel> DynoPS { get; set; }

        /// <summary>
        /// Gets or sets the environment.
        /// </summary>
        /// <value>The environment.</value>
        public virtual EnvironmentModel Environment { get; set; }

        /// <summary>
        /// Gets or sets the Geschwindigkeit locations.
        /// </summary>
        /// <value>Die Geschwindigkeit location.</value>
        public virtual IList<GeschwindigkeitModel> Geschwindigkeit { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the vehicle.
        /// </summary>
        /// <value>The vehicle.</value>
        [ForeignKey("VehicleId")]
        public virtual VehiclesModel Vehicle { get; set; }

        /// <summary>
        /// Gets or sets the vehicle identifier.
        /// </summary>
        /// <value>The vehicle identifier.</value>
        public int VehicleId { get; set; }
    }
}