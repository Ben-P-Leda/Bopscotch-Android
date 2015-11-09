using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core.Timing;

namespace Bopscotch.Gameplay.Objects.Environment
{
    public class Blackout : Background
    {
        private Timer _timer;
        private State _state;

        public TimerController.TickCallbackRegistrationHandler TickCallback { set { value(_timer.Tick); } }

        public Blackout()
            : base()
        {
            _renderDepth = Render_Depth;
            _timer = new Timer("blackout-timer", HandleTimerActionComplete);

            TextureReference = Texture_Reference;
            RenderLayer = Render_Layer;

            Reset();
        }

        private void HandleTimerActionComplete()
        {
            switch (_state)
            {
                case State.FadingIn: _state = State.Sustaining; _timer.NextActionDuration = Sustain_Duration_In_Milliseconds; break;
                case State.Sustaining: _state = State.FadingOut; _timer.NextActionDuration = Fade_Duration_In_Milliseconds; break;
                case State.FadingOut: Reset(); break;
            }
        }

        public override void Reset()
        {
            _tint = Color.Transparent;
            _state = State.FadingIn;
            Visible = false;
        }

        public void Activate()
        {
            Reset();
            _timer.NextActionDuration = Fade_Duration_In_Milliseconds;
            Visible = true;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            switch (_state)
            {
                case State.FadingIn: _tint = Color.Lerp(Color.Transparent, Color.Black, _timer.CurrentActionProgress); break;
                case State.Sustaining: _tint = Color.Black; break;
                case State.FadingOut: _tint = Color.Lerp(Color.Black, Color.Transparent, _timer.CurrentActionProgress); break;
            }

            base.Draw(spriteBatch);
        }

        private enum State
        {
            FadingIn,
            Sustaining,
            FadingOut
        }

        private const string Texture_Reference = "pixel";
        private const int Render_Layer = 2;
        private const float Render_Depth = 0.425f;
        private const int Fade_Duration_In_Milliseconds = 250;
        private const int Sustain_Duration_In_Milliseconds = 2000;
    }
}