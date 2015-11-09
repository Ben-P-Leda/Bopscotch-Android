using System.Xml.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core;
using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Motion;
using Leda.Core.Motion.Engines;
using Leda.Core.Asset_Management;

using Bopscotch.Interface;

namespace Bopscotch.Gameplay.Objects.Display.Race
{
    public class PowerUpHelper : ISimpleRenderable, ISerializable, IMobile
    {
        private BounceEntryMotionEngine _entryMotionEngine;
        private BounceExitMotionEngine _exitMotionEngine;
        private string _helpText;

        public string ID { get { return "power-up-helper"; } set { } }

        public bool Visible { get; set; }
        public int RenderLayer { get { return Render_Layer; } set { } }

        public Vector2 WorldPosition { get; set; }
        public bool WorldPositionIsFixed { get { return false; } }

        public IMotionEngine MotionEngine { get; protected set; }

        public float MotionLine
        {
            set
            {
                WorldPosition = new Vector2(value, WorldPosition.Y);
                _entryMotionEngine.TargetWorldPosition = new Vector2(value, Y_When_Active);
                _exitMotionEngine.TargetWorldPosition = new Vector2(value, Y_When_Inactive);
            }
        }

        public PowerUpHelper()
        {
            _entryMotionEngine = new BounceEntryMotionEngine();
            _entryMotionEngine.ObjectToTrack = this;
            _entryMotionEngine.RecoilMultiplier = Default_Recoil_Multiplier;
            _entryMotionEngine.CompletionCallback = HandleEntryCompletion;

            _exitMotionEngine = new BounceExitMotionEngine();
            _exitMotionEngine.ObjectToTrack = this;
            _exitMotionEngine.RecoilMultiplier = Default_Recoil_Multiplier;
            _exitMotionEngine.CompletionCallback = Reset;
        }

        protected virtual void HandleEntryCompletion()
        {
            Visible = true;
            MotionEngine = null;
        }

        public void Initialize()
        {
        }

        public void Reset()
        {
            WorldPosition = new Vector2(WorldPosition.X, Y_When_Inactive);
            Visible = false;
            MotionEngine = null;
        }

        public void SetHelpText(string powerUpTextureName)
        {
            _helpText = Translator.Translation(string.Concat(powerUpTextureName, "-helper"));
        }

        public void Activate()
        {
            if ((!Visible) && (MotionEngine == null))
            {
                Reset();

                _entryMotionEngine.Activate();
                MotionEngine = _entryMotionEngine;

                Visible = true;
            }
        }

        public void Dismiss()
        {
            if ((Visible) && (MotionEngine == null))
            {
                _exitMotionEngine.Activate();
                MotionEngine = _exitMotionEngine;
            }
        }

        public void Update(int millisecondsSinceLastUpdate)
        {
            if (MotionEngine != null) { MotionEngine.Update(millisecondsSinceLastUpdate); }
            if (MotionEngine != null) { WorldPosition += MotionEngine.Delta; }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            TextWriter.Write(_helpText, spriteBatch, WorldPosition, Color.White, Color.Black, 3.0f, 0.75f, Render_Depth, TextWriter.Alignment.Center);
        }

        public XElement Serialize()
        {
            return null;
        }

        public void Deserialize(XElement serializedData)
        {
        }

        private const int Default_Recoil_Multiplier = 5;
        private const float Render_Depth = 0.1f;
        private const int Render_Layer = 4;
        private const float Y_When_Active = 20.0f;
        private const float Y_When_Inactive = -150.0f;
    }
}
