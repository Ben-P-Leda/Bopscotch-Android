using Microsoft.Xna.Framework;

namespace Bopscotch.Communication
{
    public class OpponentRaceProgressCoordinator : Data.RacePlayerCommunicationData
    {
        private Vector2 _playerWorldPosition;

        public override Vector2 PlayerWorldPosition { get { return _playerWorldPosition; } }

        public OpponentRaceProgressCoordinator()
        {
            _playerWorldPosition = Vector2.Zero;
        }

        public void Populate(int raceTimeElapsed, int lapsComplete, int lastCheckpointIndex, int lastCheckpointTime, float worldXPosition, float worldYPosition)
        {
			ReadyToRace = true;

            TotalRaceTimeElapsedInMilliseconds = raceTimeElapsed;
            LapsCompleted = lapsComplete;
            LastCheckpointTimeInMilliseconds = lastCheckpointTime;
            LastCheckpointIndex = lastCheckpointIndex;
            _playerWorldPosition.X = worldXPosition;
            _playerWorldPosition.Y = worldYPosition;
        }
    }
}
