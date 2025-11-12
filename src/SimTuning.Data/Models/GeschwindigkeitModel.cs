// Copyright (c) 2025 tuke productions. All rights reserved.
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SimTuning.Data.Models
{
    /// <summary>
    /// LocationModel.
    /// </summary>
    /// <seealso cref="Data.Models.BaseEntityModel" />
    public class GeschwindigkeitModel : BaseEntityModel
    {
        /// <summary>
        /// Gets or sets the accuracy.
        /// </summary>
        /// <value>The accuracy.</value>
        public double? Accuracy { get; set; }

        /// <summary>
        /// Gets or sets the altitude.
        /// </summary>
        /// <value>The altitude.</value>
        public double? Altitude { get; set; }

        /// <summary>
        /// Gets or sets the altitude accuracy.
        /// </summary>
        /// <value>The altitude accuracy.</value>
        public double? AltitudeAccuracy { get; set; }

        /// <summary>
        /// Gets or sets the dyno.
        /// </summary>
        /// <value>The dyno.</value>
        [ForeignKey(nameof(DynoId))]
        public virtual DynoModel Dyno { get; set; }

        /// <summary>
        /// Gets or sets the dyno identifier.
        /// </summary>
        /// <value>The dyno identifier.</value>
        public int DynoId { get; set; }

        /// <summary>
        /// Gets or sets the heading.
        /// </summary>
        /// <value>The heading.</value>
        public double? Heading { get; set; }

        /// <summary>
        /// Gets or sets the heading accuracy.
        /// </summary>
        /// <value>The heading accuracy.</value>
        public double? HeadingAccuracy { get; set; }

        /// <summary>
        /// Gets or sets the latitude.
        /// </summary>
        /// <value>The latitude.</value>
        public double Latitude { get; set; }

        /// <summary>
        /// Gets or sets the longitude.
        /// </summary>
        /// <value>The longitude.</value>
        public double Longitude { get; set; }

        /// <summary>
        /// Gets or sets the speed.
        /// </summary>
        /// <value>The speed.</value>
        public double? Speed { get; set; }
    }
}