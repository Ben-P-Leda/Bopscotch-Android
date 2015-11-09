using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core;
using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Asset_Management;
using Leda.Core.Motion;
using Leda.Core.Motion.Engines;
using Leda.Core.Timing;

using Bopscotch.Gameplay.Coordination;

namespace Bopscotch.Gameplay.Objects.Display.Race
{
    public class PowerUpTimer : ISimpleRenderable, IMobile
    {
        private Vector2 _textureOrigin;
        private Texture2D _iconTexture;
        private int _timerDurationInMilliseconds;
        private BounceEntryMotionEngine _entryMotionEngine;
        private BounceExitMotionEngine _exitMotionEngine;
        private Timer _durationTimer;

        public Vector2 WorldPosition { get; set; }
        public bool WorldPositionIsFixed { get { return false; } }

        public IMotionEngine MotionEngine { get; private set; }

        public int RenderLayer { get { return Render_Layer; } set { } }
        public bool Visible { get; set; }

        public Vector2 DisplayTopRight { private get; set; }
        public TimerController.TickCallbackRegistrationHandler TickCallback { set { value(_durationTimer.Tick); } }
        public Timer.ActionCompletionCallback CompletionCallback { set { _durationTimer.ActionCompletionHandler = value; } }

        public PowerUpTimer()
        {
            _entryMotionEngine = new BounceEntryMotionEngine();
            _entryMotionEngine.ObjectToTrack = this;
            _entryMotionEngine.RecoilMultiplier = Recoil_Multiplier;
            _entryMotionEngine.TargetWorldPosition = new Vector2(Center_X_Offset, Center_Y_When_Active);
            _entryMotionEngine.CompletionCallback = StartTimer;

            _exitMotionEngine = new BounceExitMotionEngine();
            _exitMotionEngine.ObjectToTrack = this;
            _exitMotionEngine.RecoilMultiplier = Recoil_Multiplier;
            _exitMotionEngine.TargetWorldPosition = new Vector2(Center_X_Offset, -Center_Y_When_Active);
            _exitMotionEngine.CompletionCallback = Reset;

            _durationTimer = new Timer("");
        }

        private void StartTimer()
        {
            _durationTimer.NextActionDuration = _timerDurationInMilliseconds;
        }

        public void Initialize()
        {
            _textureOrigin = new Vector2(TextureManager.Textures[Base_Texture_Name].Width, TextureManager.Textures[Base_Texture_Name].Height) / 2.0f;
        }

        public void Reset()
        {
            MotionEngine = null;
            WorldPosition = new Vector2(Center_X_Offset, -Center_Y_When_Active);
            Visible = false;
        }

        public void Activate(Definitions.PowerUp selectedPowerUp)
        {
            switch (selectedPowerUp)
            {
                case Definitions.PowerUp.Boots: _timerDurationInMilliseconds = Boots_Duration_In_Milliseconds; break;
                case Definitions.PowerUp.Chilli: _timerDurationInMilliseconds = Chilli_Duration_In_Milliseconds; break;
                default: _timerDurationInMilliseconds = 0; break;
            }

            if (_timerDurationInMilliseconds > 0)
            {
                _entryMotionEngine.Activate();
                MotionEngine = _entryMotionEngine;

                _iconTexture = TextureManager.Textures[string.Concat("power-", selectedPowerUp).ToLower()];

                Visible = true;
            }
        }

        public void Deactivate()
        {
            _durationTimer.Reset();
            _exitMotionEngine.Activate();
            MotionEngine = _exitMotionEngine;
        }

        public void Update(int millisecondsSinceLastUpdate)
        {
            if (MotionEngine != null) { MotionEngine.Update(millisecondsSinceLastUpdate); }
            if (MotionEngine != null) { WorldPosition += MotionEngine.Delta; }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(TextureManager.Textures[Base_Texture_Name], GameBase.ScreenPosition(DisplayTopRight + WorldPosition), 
                null, Color.White, 0.0f, _textureOrigin, GameBase.ScreenScale(), SpriteEffects.None, Base_Render_Depth);

            spriteBatch.Draw(_iconTexture, GameBase.ScreenPosition(DisplayTopRight + WorldPosition), null, Color.White, 0.0f,
                new Vector2(_iconTexture.Width, _iconTexture.Height) / 2.0f, GameBase.ScreenScale(), SpriteEffects.None, Icon_Render_Depth);

            spriteBatch.Draw(TextureManager.Textures[Needle_Texture_Name], GameBase.ScreenPosition(DisplayTopRight + WorldPosition), null, 
                Color.White, (MathHelper.TwoPi * _durationTimer.CurrentActionProgress), _textureOrigin, GameBase.ScreenScale(), SpriteEffects.None, Needle_Render_Depth);
        }

        private const int Render_Layer = 4;
        private const float Base_Render_Depth = 0.61f;
        private const string Base_Texture_Name = "clock-base";
        private const float Icon_Render_Depth = 0.5f;
        private const float Needle_Render_Depth = 0.49f;
        private const string Needle_Texture_Name = "clock-needle";

        private const int Recoil_Multiplier = 7;
        private const float Center_X_Offset = -200.0f;
        private const float Center_Y_When_Active = 51.0f;

        public const int Chilli_Duration_In_Milliseconds = 2500;
        public const int Boots_Duration_In_Milliseconds = 3500;
    }
}
