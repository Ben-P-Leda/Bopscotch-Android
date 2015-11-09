using Leda.Core.Asset_Management;

namespace Bopscotch.Gameplay.Objects.Environment.Flags
{
    public class GoalFlag : Flag
    {
        public GoalFlag()
            : base()
        {
            TextureReference = Texture_Name;
            Texture = TextureManager.Textures[Texture_Name];

            SetFrameAndAnimation();
        }

        private const string Texture_Name = "flag-goal";

        public const string Data_Node_Name = "goal-flag";
    }
}
