using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core;
using Leda.Core.Game_Objects.Base_Classes;
using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Motion;
using Leda.Core.Timing;

namespace Bopscotch.Effects.SmashBlockItems
{
    public class SmashBlockItem : DisposableSimpleDrawableObject, ITemporary, IMobile
    {
        private SmashBlockItemMotionEngine _motionEngine;
        private Timer _lifeTimer;

        public bool ReadyForDisposal { get; set; }

        public IMotionEngine MotionEngine { get { return _motionEngine; } }
        public TimerController.TickCallbackRegistrationHandler TimerTickCallback { set { value(_lifeTimer.Tick); } }

        public SmashBlockItem()
            : base()
        {
            RenderLayer = Render_Layer;
            RenderDepth = Render_Depth;

            Visible = true;
            ReadyForDisposal = false;

            _motionEngine = new SmashBlockItemMotionEngine();

            _lifeTimer = new Timer("", HandleLifeTimeComplete);
            _lifeTimer.NextActionDuration = Random.Generator.Next(Minimum_Duration_In_Milliseconds, Maximum_Duration_In_Milliseconds);
        }

        private void HandleLifeTimeComplete()
        {
            Visible = false;
            ReadyForDisposal = true;
        }

        public virtual void Update(int millisecondsSinceLastUpdate)
        {
            Tint = Color.Lerp(Color.White, Color.Transparent, _lifeTimer.CurrentActionProgress);
            _motionEngine.Update(millisecondsSinceLastUpdate);
            WorldPosition += _motionEngine.Delta;
        }

        public void PrepareForDisposal()
        {
        }

        private const int Render_Layer = 4;
        private const float Render_Depth = 0.8f;

        private const int Minimum_Duration_In_Milliseconds = 1000;
        private const int Maximum_Duration_In_Milliseconds = 2500;
    }
}
