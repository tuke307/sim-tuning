using CommunityToolkit.Mvvm.Messaging.Messages;
using SimTuning.Data.Models;

namespace SimTuning.Core.Models.Messages
{
    public class DynoChangedMessage : ValueChangedMessage<DynoModel>
    {
        public DynoChangedMessage(DynoModel dyno)
            : base(dyno)
        {
        }
    }
}