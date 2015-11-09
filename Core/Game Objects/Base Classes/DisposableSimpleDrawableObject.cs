using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Gamestate_Management;

namespace Leda.Core.Game_Objects.Base_Classes
{
    public class DisposableSimpleDrawableObject : ISimpleRenderable, ICameraRelative,
        ITransformationAnimatable, ISpriteSheetAnimatable, IColourAnimatable
    {
        private float _rotation;
        private SpriteEffects _mirrorEffect;
        private bool _worldPositionNeverChanges;

        public virtual Vector2 WorldPosition { get; set; }
        public virtual Vector2 CameraPosition { get; set; }
        public int RenderLayer { get; set; }
        public virtual bool Visible { get; set; }

        public virtual Texture2D Texture { set; private get; }
        public Rectangle Frame { set; protected get; }
        public Vector2 Origin { set; protected get; }
        public virtual float Scale { set; get; }
        public virtual float Rotation 
        { 
            get 
            { 
                return _rotation; 
            } 
            set 
            {
                _rotation = value;
                if (_rotation < 0.0f) { _rotation += MathHelper.TwoPi; }
                if (_rotation > MathHelper.TwoPi) { _rotation %= MathHelper.TwoPi; }
            } 
        }

        public virtual bool Mirror
        { 
            get { return (_mirrorEffect == SpriteEffects.FlipHorizontally); }
            set { if (value) { _mirrorEffect = SpriteEffects.FlipHorizontally; } else { _mirrorEffect = SpriteEffects.None; } } 
        }
        public virtual Color Tint { set; get; }
        public float RenderDepth { set; get; }
        public bool WorldPositionIsFixed { get { return _worldPositionNeverChanges; } set { _worldPositionNeverChanges = value; } }

        public DisposableSimpleDrawableObject()
        {
            _worldPositionNeverChanges = (!(this is IMobile));

            Texture = null;
            Frame = Rectangle.Empty;
            Origin = Vector2.Zero;
            Scale = 1.0f;
            Rotation = 0.0f;
            Mirror = false;
            RenderDepth = 0.5f;
            Tint = Color.White;

            WorldPosition = Vector2.Zero;
            CameraPosition = Vector2.Zero;
            RenderLayer = -1;
            Visible = false;
        }

        public virtual void Initialize()
        {
        }

        public virtual void Reset()
        {
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if ((Texture != null) && (Frame.Width > 0) && (Frame.Height > 0))
            {
                spriteBatch.Draw(Texture, GameBase.ScreenPosition(WorldPosition - CameraPosition), Frame, Tint, Rotation, Origin, 
                    GameBase.ScreenScale(Scale), _mirrorEffect, RenderDepth);
            }
        }
    }
}
