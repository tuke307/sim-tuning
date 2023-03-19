// Copyright (c) 2021 tuke productions. All rights reserved.
namespace SimTuning.Data.Models
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// DynoAudio.
    /// </summary>
    /// <seealso cref="Data.Models.BaseEntityModel" />
    public class DrehzahlModel : BaseEntityModel
    {
        public DrehzahlModel()
        {
        }

        public DrehzahlModel(double zeit, double drehzahl)
        {
            this.Zeit = zeit;
            this.Drehzahl = drehzahl;
        }

        /// <summary>
        /// Gets or sets the drehzahl.
        /// </summary>
        /// <value>The drehzahl.</value>
        [Required]
        public double Drehzahl { get; set; }

        /// <summary>
        /// Gets or sets the dyno.
        /// </summary>
        /// <value>The dyno.</value>
        public virtual DynoModel Dyno { get; set; }

        /// <summary>
        /// Gets or sets the zeit.
        /// </summary>
        /// <value>The zeit.</value>
        [Required]
        public double Zeit { get; set; }
    }
}