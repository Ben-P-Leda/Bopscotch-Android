using Microsoft.Xna.Framework;

namespace Bopscotch.Data
{
    public class LevelData
    {
        public PlayState CurrentPlayState { get; set; }

        public LevelData()
        {
            CurrentPlayState = PlayState.Beginning;
        }

        public enum PlayState
        {
            Beginning,
            InPlay,
            Exiting,
            Complete
        }
    }
}
