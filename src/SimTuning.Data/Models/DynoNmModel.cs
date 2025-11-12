// Copyright (c) 2025 tuke productions. All rights reserved.
namespace SimTuning.Data.Models
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// DynoNM Model.
    /// </summary>
    /// <seealso cref="Data.Models.BaseEntityModel" />
    public class DynoNmModel : BaseEntityModel
    {
        /// <summary>
        /// Gets or sets the x.
        /// </summary>
        /// <value>The x.</value>
        [Required]
        public double Drehzahl { get; set; }

        /// <summary>
        /// Gets or sets the dyno.
        /// </summary>
        /// <value>The dyno.</value>
        public virtual DynoModel Dyno { get; set; }

        /// <summary>
        /// Gets or sets the y.
        /// </summary>
        /// <value>The y.</value>
        [Required]
        public double Nm { get; set; }
    }
}