//// Copyright (c) 2021 tuke productions. All rights reserved.
//using MvvmCross.Plugin.Messenger;
//using SimTuning.Data.Models;

//namespace SimTuning.Core.Models
//{
//    /// <summary>
//    /// MvxReloaderMessage.
//    /// </summary>
//    /// <seealso cref="MvvmCross.Plugin.Messenger.MvxMessage" />
//    public class MvxReloaderMessage
//      : MvxMessage
//    {
//        public DynoModel Dyno
//        {
//            get;
//            private set;
//        }

//        /// <summary>
//        /// Initializes a new instance of the <see cref="MvxReloaderMessage" /> class.
//        /// </summary>
//        /// <param name="sender">The sender.</param>
//        /// <param name="dyno">The dyno.</param>
//        public MvxReloaderMessage(object sender, DynoModel dyno)
//            : base(sender)
//        {
//            this.Dyno = dyno;
//        }
//    }
//}