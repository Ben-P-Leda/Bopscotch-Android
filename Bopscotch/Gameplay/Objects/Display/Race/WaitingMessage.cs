using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Animation;
using Leda.Core;
using Leda.Core.Asset_Management;

using Bopscotch.Interface;

namespace Bopscotch.Gameplay.Objects.Display.Race
{
    public class WaitingMessage : IAnimated, IColourAnimatable
    {
        private Vector2 _position;
        private ColourAnimationEngine _animationEngine;
        private bool _hideAfterCurrentAnimationCycleComplete;

        public Vector2 Position { set { _position.X = value.X + Left_Margin; _position.Y = value.Y - Line_Height; } }
        public bool Visible { get { return true; } set { } }
        public Color Tint { get; set; }
        public int RenderLayer { get { return Render_Layer; } set { } }

        public IAnimationEngine AnimationEngine { get { return _animationEngine; } }

        private Color OutlineTint { get { return new Color(0, 0, 0, Tint.A); } }

        public WaitingMessage()
        {
            _hideAfterCurrentAnimationCycleComplete = false;
            _position = Vector2.Zero;

            _animationEngine = new ColourAnimationEngine(this);
            _animationEngine.KeyframeCompletionHandler = HandleAnimationFrameCompletion;
        }

        private void HandleAnimationFrameCompletion(int frameIndex)
        {
            if ((_hideAfterCurrentAnimationCycleComplete) && (frameIndex == AnimationDataManager.Sequences[Pulse_Fade_Sequence].FrameCount - 1))
            {
                Reset();
            }
        }

        public void Initialize()
        {
        }

        public void Reset()
        {
            _animationEngine.Sequence = null;

            Visible = false;
        }

        public void Activate()
        {
            Visible = true;
            _hideAfterCurrentAnimationCycleComplete = false;

            AnimationEngine.Sequence = AnimationDataManager.Sequences[Pulse_Fade_Sequence];
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            TextWriter.Write(Translator.Translation("Waiting for opponent..."), spriteBatch, _position, Tint, OutlineTint, 3.0f, 0.7f, Render_Depth, 
                TextWriter.Alignment.Left);
        }

        public void HideAfterCurrentAnimationCycleComplete()
        {
            _hideAfterCurrentAnimationCycleComplete = true;
        }

        private const int Render_Layer = 4;
        private const float Left_Margin = 10.0f;
        private const float Line_Height = 80.0f;
        private const float Render_Depth = 0.1f;
        private const string Pulse_Fade_Sequence = "pulse-fade";
    }
}
