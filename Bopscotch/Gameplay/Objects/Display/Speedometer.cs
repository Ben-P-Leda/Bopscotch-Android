using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core;
using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Asset_Management;
using Leda.Core.Animation;

using Bopscotch.Gameplay.Objects.Characters.Player;

namespace Bopscotch.Gameplay.Objects.Display
{
    public class Speedometer : ISimpleRenderable, IAnimated, ITransformationAnimatable
    {
        private float _displayedSpeed;
        private bool _displayingLock;

        private Color[] _segmentTint;
        private bool[] _segmentIsActive;

        private Vector2 _segmentOrigin;

        public int RenderLayer { get { return Render_Layer; } set { } }
        public bool Visible { get; set; }
        public Vector2 CenterPosition { private get; set; }
        public Vector2 Origin { get; private set; }
        public float Scale { get; set; }
        public float Rotation { get; set; }

        public PlayerMotionEngine PlayerMotionEngine { private get; set; }
        public IAnimationEngine AnimationEngine { get; private set; }

        public Speedometer()
            : base()
        {
            _segmentTint = new Color[] { Color.ForestGreen, Color.Yellow, Color.Orange, Color.Red };
            _segmentIsActive = new bool[_segmentTint.Length];

            RenderLayer = Render_Layer;
            Visible = true;

            AnimationEngine = new TransformationAnimationEngine(this);
        }

        public void Initialize()
        {
            Origin = new Vector2(TextureManager.Textures[Base_Texture_Name].Width, TextureManager.Textures[Base_Texture_Name].Height) / 2.0f;
            _segmentOrigin = new Vector2(TextureManager.Textures[Segment_Texture_Name].Width, TextureManager.Textures[Segment_Texture_Name].Height / 2.0f);
        }

        public void Reset()
        {
            _displayedSpeed = PlayerMotionEngine.Minimum_Movement_Speed;
            _displayingLock = false;

            Scale = 0.0f;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            SetActiveSegments();
            spriteBatch.Draw(TextureManager.Textures[Base_Texture_Name], GameBase.ScreenPosition(CenterPosition), null, Color.White, 0.0f, 
                Origin, GameBase.ScreenScale(), SpriteEffects.None, Base_Render_Depth);

            for (int i = 0; i < _segmentTint.Length; i++)
            {
                spriteBatch.Draw(TextureManager.Textures[Segment_Texture_Name], GameBase.ScreenPosition(CenterPosition), null, 
                    (_segmentIsActive[i] ? _segmentTint[i] : Color.Gray), MathHelper.ToRadians(Segment_Sweep_In_Degrees) * i , _segmentOrigin, 
                    GameBase.ScreenScale(), SpriteEffects.None, Segment_Render_Depth);
            }

            spriteBatch.Draw(TextureManager.Textures[Needle_Texture_Name], GameBase.ScreenPosition(CenterPosition), null, Color.White, 
                NeedleAngle, Origin, GameBase.ScreenScale(), SpriteEffects.None, Needle_Render_Depth);

            spriteBatch.Draw(TextureManager.Textures[Center_Texture_Name], GameBase.ScreenPosition(CenterPosition), null, Color.White, 0.0f,
                new Vector2(TextureManager.Textures[Center_Texture_Name].Width, TextureManager.Textures[Center_Texture_Name].Height) / 2.0f, 
                GameBase.ScreenScale(), SpriteEffects.None, Center_Render_Depth);

            DrawFreezeLockIfRequired(spriteBatch);
        }

        private float NeedleAngle
        {
            get
            {
                CalculateDisplayedSpeed();
                float fraction = MathHelper.ToRadians(Dial_Sweep_Angle_In_Degrees) /
                    (PlayerMotionEngine.Speed_Limit_Range_Step * PlayerMotionEngine.Maximum_Speed_Range_Steps);

                return fraction * (_displayedSpeed - PlayerMotionEngine.Minimum_Movement_Speed);
            }
        }

        private void CalculateDisplayedSpeed()
        {
            float delta = 0.0f;
            float targetSpeed = Math.Max(PlayerMotionEngine.Speed, PlayerMotionEngine.Minimum_Movement_Speed);

            if (_displayedSpeed < targetSpeed) { delta = Math.Min(targetSpeed - _displayedSpeed, Displayed_Speed_Delta); }
            else if (_displayedSpeed > targetSpeed) { delta = Math.Max(targetSpeed - _displayedSpeed, -Displayed_Speed_Delta); }

            _displayedSpeed += delta;
        }

        public void SetActiveSegments()
        {
            for (int i = 0; i < _segmentIsActive.Length; i++) 
            {
                _segmentIsActive[i] = Utility.Between(i, 
                    PlayerMotionEngine.AvailableSpeedRangeSteps.Minimum, PlayerMotionEngine.AvailableSpeedRangeSteps.Maximum + 1);
            }
        }

        private void DrawFreezeLockIfRequired(SpriteBatch spriteBatch)
        {
            if ((PlayerMotionEngine.SpeedChangesAreLockedOut) && (!_displayingLock))
            {
                Scale = 0.0f;
                AnimationEngine.Sequence = AnimationDataManager.Sequences["image-popup-entry-with-bounce"];
                SoundEffectManager.PlayEffect("freeze-speedo");
                _displayingLock = true;
            }
            else if ((!PlayerMotionEngine.SpeedChangesAreLockedOut) && (_displayingLock))
            {
                _displayingLock = false;
                AnimationEngine.Sequence = AnimationDataManager.Sequences["exit-with-bounce"];
            }

            spriteBatch.Draw(
                TextureManager.Textures["speedo-snowflake"],
                GameBase.ScreenPosition(CenterPosition),
                null,
                Color.White,
                0.0f,
                new Vector2(TextureManager.Textures["speedo-snowflake"].Bounds.Width, TextureManager.Textures["speedo-snowflake"].Bounds.Height) / 2.0f,
                GameBase.ScreenScale(Scale),
                SpriteEffects.None,
                0.4f);
        }

        private const int Render_Layer = 4;
        private const float Base_Render_Depth = 0.51f;
        private const string Base_Texture_Name = "speedo-base";
        private const float Segment_Render_Depth = 0.5f;
        private const string Segment_Texture_Name = "speedo-segment";
        private const float Needle_Render_Depth = 0.49f;
        private const string Needle_Texture_Name = "speedo-needle";
        private const float Center_Render_Depth = 0.48f;
        private const string Center_Texture_Name = "speedo-center";

        private const float Segment_Sweep_In_Degrees = 60.0f;
        private const float Dial_Sweep_Angle_In_Degrees = 240.0f;

        private const float Displayed_Speed_Delta = 0.02f;
    }
}
