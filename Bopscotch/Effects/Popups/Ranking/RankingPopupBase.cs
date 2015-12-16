using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core;
using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Animation;
using Leda.Core.Asset_Management;

using Bopscotch.Gameplay.Coordination;

namespace Bopscotch.Effects.Popups.Ranking
{
    public abstract class RankingPopupBase : ISimpleRenderable, IAnimated, ITransformationAnimatable, IColourAnimatable
    {
        public SurvivalRankingCoordinator.RankSequenceCallback NextAction { protected get; set; }

        private Texture2D _texture;
        private Vector2 _origin;
        private Rectangle _frame;

        protected string TextureName { private get; set; }

        public Vector2 DisplayPosition { private get; set; }
        public bool Visible { get; set; }
        public int RenderLayer { get { return Render_Layer; } set { } }
        public float Rotation { get; set; }
        public float Scale { get; set; }
        public Color Tint { get; set; }
        public int FrameOffset { set { _frame.X = value * _frame.Width; } }

        public IAnimationEngine AnimationEngine { get; private set; }

        public void Initialize()
        {
            _texture = TextureManager.Textures[TextureName];
            _origin = new Vector2(_texture.Bounds.Height) / 2.0f;
            _frame = new Rectangle(0, 0, _texture.Bounds.Width / Rank_Count, _texture.Bounds.Height);

            AnimationEngine = new TransformationAnimationEngine(this);
            AnimationEngine.SequenceCompletionHandler = HandleAnimationSequenceComplete;
        }

        public virtual void Activate()
        {
            Scale = 0.0f;
            Tint = Color.White;
            Rotation = 0.0f;
            Visible = true;
        }

        public void Reset()
        {
            Scale = 0.0f;
            Visible = false;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, GameBase.ScreenPosition(DisplayPosition), _frame, Tint, Rotation, _origin, GameBase.ScreenScale(Scale),
                SpriteEffects.None, Render_Depth);
        }

        protected abstract void HandleAnimationSequenceComplete();

        private const int Render_Layer = 4;
        private const float Render_Depth = 0.7f;
        private const int Rank_Count = 3;
    }
}
