using Microsoft.Xna.Framework;

namespace Bopscotch.Communication
{
    public interface ICommunicator
    {
        Data.RacePlayerCommunicationData OwnPlayerData { get; set; }
        Data.RacePlayerCommunicationData OtherPlayerData { get; }
        bool ConnectionLost { get; }

        void Update();
    }
}
