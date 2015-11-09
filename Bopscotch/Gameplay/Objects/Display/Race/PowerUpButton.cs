using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core;
using Leda.Core.Asset_Management;
using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Animation;

namespace Bopscotch.Gameplay.Objects.Display.Race
{
    public class PowerUpButton : InGameButtonBase, IAnimated, ITransformationAnimatable, IColourAnimatable
    {
        private Texture2D _iconTexture;
        private Vector2 _iconOrigin;
        private IAnimationEngine _activeAnimationEngine;
        private TransformationAnimationEngine _transformAnimationEngine;
        private ColourAnimationEngine _colourAnimationEngine;
        private string _queuedIconTexture;

        public new float Scale { get { return base.Scale; } set { base.Scale = value; } }
        public float Rotation { get { return 0.0f; } set { } }
        public IAnimationEngine AnimationEngine { get { return _activeAnimationEngine; } }
        public Color Tint { get { return _tint; } set { _tint = value; } }

        public string IconTexture
        {
            set
            {
                if (Visible)
                {
                    _queuedIconTexture = value;
                    Deactivate();
                }
                else
                {
                    _iconTexture = TextureManager.Textures[value];
                    _iconOrigin = new Vector2(_iconTexture.Width, _iconTexture.Height) / 2.0f;
                    _queuedIconTexture = "";

                    Visible = true;
                    Scale = 0.0f;

                    _tint = Color.DodgerBlue;
                    _transformAnimationEngine.Sequence = AnimationDataManager.Sequences[Entry_Sequence_Name];
                    _activeAnimationEngine = _transformAnimationEngine;
                }
            }
        }

        public PowerUpButton()
            : base()
        {
            _queuedIconTexture = "";
            _iconTexture = null;
            _activeAnimationEngine = null;

            _transformAnimationEngine = new TransformationAnimationEngine(this);
            _transformAnimationEngine.SequenceCompletionHandler = HandleAnimationComplete;

            _colourAnimationEngine = new ColourAnimationEngine(this);
            _colourAnimationEngine.SequenceCompletionHandler = HandleAnimationComplete;

            Visible = false;
        }

        public override void Reset()
        {
            Visible = false;
        }

        private void HandleAnimationComplete()
        {
            if (_activeAnimationEngine == _colourAnimationEngine)
            {
                Visible = false;
                if (!string.IsNullOrEmpty(_queuedIconTexture)) { IconTexture = _queuedIconTexture; }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            if (_iconTexture != null)
            {
                spriteBatch.Draw(_iconTexture, GameBase.ScreenPosition(Center), _iconTexture.Bounds, IconTint, 0.0f, _iconOrigin, GameBase.ScreenScale(Scale), 
                    SpriteEffects.None, Icon_Render_Depth);
            }
        }

        private Color IconTint { get { return new Color(_tint.B, _tint.B, _tint.B, _tint.A); } }

        public void Deactivate()
        {
            _colourAnimationEngine.Sequence = AnimationDataManager.Sequences[Exit_Sequence_Name];
            _activeAnimationEngine = _colourAnimationEngine;
        }

        private const string Entry_Sequence_Name = "image-popup-entry-with-bounce";
        private const string Exit_Sequence_Name = "image-popup-exit";

        public const string In_Game_Button_Name = "power-up";
    }
}
