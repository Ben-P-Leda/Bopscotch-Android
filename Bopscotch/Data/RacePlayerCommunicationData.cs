using Microsoft.Xna.Framework;

using Bopscotch.Gameplay.Objects.Characters.Player;

namespace Bopscotch.Data
{
    public abstract class RacePlayerCommunicationData
    {
        public bool ReadyToRace { get; protected set; }
        public int TotalRaceTimeElapsedInMilliseconds { get; protected set; }
        public int LapsCompleted { get; protected set; }
        public int LastCheckpointTimeInMilliseconds { get; protected set; }
        public int LastCheckpointIndex { get; protected set; }

        public Definitions.PowerUp LastAttackPowerUp { get; set; }
        public int LastAttackPowerUpTimeInMilliseconds { get; set; }

        public abstract Vector2 PlayerWorldPosition { get; }

        public RacePlayerCommunicationData()
            : base()
        {
            ReadyToRace = false;
            TotalRaceTimeElapsedInMilliseconds = 0;
            LastCheckpointIndex = -1;
        }
    }
}
