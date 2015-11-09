using Microsoft.Xna.Framework;

using Leda.Core.Game_Objects.Base_Classes;
using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Motion;
using Leda.Core.Animation;
using Leda.Core.Asset_Management;

using Bopscotch.Data;
using Bopscotch.Data.Avatar;
using Bopscotch.Gameplay.Objects.Characters.Player;

namespace Bopscotch.Interface.Objects.EditCharacter
{
    public class CustomisationDisplayAvatar : DisposableSkeleton, IMobile, IAnimated
    {
        private PlayerMotionEngine _motionEngine;
        public IMotionEngine MotionEngine { get { return _motionEngine; } }

        private SkeletalAnimationEngine _animationEngine;
        public IAnimationEngine AnimationEngine { get { return _animationEngine; } }

        public CustomisationDisplayAvatar()
            : base()
        {
            _motionEngine = new PlayerMotionEngine();
            _animationEngine = new SkeletalAnimationEngine(this);

            RenderLayer = Render_Layer;
            Visible = true;
        }

        public override void Initialize()
        {
            CreateBonesFromDataManager(Definitions.Avatar_Skeleton_Side);
            SkinBones(AvatarComponentManager.SideFacingAvatarSkin(Profile.Settings.SelectedAvatarSlot));
            base.Initialize();
        }

        public override void Reset()
        {
            WorldPosition = new Vector2(Definitions.Back_Buffer_Center.X, Starting_Y_Position);

            _motionEngine.VerticalMovementIsEnabled = true;
            _motionEngine.PlayerIsOnGround = true;

            AnimationEngine.Sequence = AnimationDataManager.Sequences["player-jump"];

            base.Reset();
        }

        public void Update(int milliSecondsSinceLastUpdate)
        {
            float lastDeltaY = _motionEngine.Delta.Y;

            _motionEngine.Update(milliSecondsSinceLastUpdate);
            WorldPosition += new Vector2(0.0f, _motionEngine.Delta.Y);

            if (WorldPosition.Y > Starting_Y_Position) { Reset(); }
            else if ((lastDeltaY < 0.0f) && (_motionEngine.Delta.Y > 0.0f)) { AnimationEngine.Sequence = AnimationDataManager.Sequences["player-fall"]; }
        }

        private const int Render_Layer = 2;
        private const float Starting_Y_Position = 300.0f;
    }
}
