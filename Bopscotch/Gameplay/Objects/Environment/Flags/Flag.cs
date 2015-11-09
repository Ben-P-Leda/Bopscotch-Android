using System.Xml.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core;
using Leda.Core.Game_Objects.Base_Classes;
using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Asset_Management;
using Leda.Core.Serialization;
using Leda.Core.Animation;
using Leda.Core.Motion;

namespace Bopscotch.Gameplay.Objects.Environment.Flags
{
    public abstract class Flag : StorableSimpleDrawableObject, IBoxCollidable, ICameraRelative, IAnimated
    {
        private Rectangle _collisionBoundingBox;
        private SpriteSheetAnimationEngine _animationEngine;
        private Vector2 _poleWorldPosition;

        public IAnimationEngine AnimationEngine { get { return _animationEngine; } }

        public bool Collidable { get; set; }
        public Rectangle CollisionBoundingBox { get { return _collisionBoundingBox; } }
        public Rectangle PositionedCollisionBoundingBox { get; set; }

        public bool ActivatedWhenMovingLeft { get; set; }

        public Flag()
            : base()
        {
            RenderLayer = Render_Layer;
            RenderDepth = Flag_Render_Depth;

            Visible = true;
            Collidable = true;

            ActivatedWhenMovingLeft = false;

            _animationEngine = new SpriteSheetAnimationEngine(this);
        }

        protected void SetFrameAndAnimation()
        {
            Rectangle frame = TextureManager.Textures[TextureReference].Bounds;
            frame.Width /= AnimationDataManager.Sequences[Flag_Animation_Sequence_Name].FrameCount;

            Frame = frame;

            _animationEngine.Sequence = AnimationDataManager.Sequences[Flag_Animation_Sequence_Name];
        }

        public void SetUpPole()
        {
            _poleWorldPosition = WorldPosition;
            WorldPosition += new Vector2(Flag_Horizontal_Offset, Flag_Vertical_Offset);
        }

        public void SetCollisionBoundingBox(float verticalOverflow)
        {
            _collisionBoundingBox = new Rectangle(
                (int)Flag_Horizontal_Offset - (Definitions.Grid_Cell_Pixel_Size / 2),
                (int)(verticalOverflow - (Definitions.Grid_Cell_Pixel_Size / 2)),
                Definitions.Grid_Cell_Pixel_Size,
                (Definitions.Grid_Cell_Pixel_Size * 3) - (int)verticalOverflow);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            spriteBatch.Draw(TextureManager.Textures[Pole_Texture], GameBase.ScreenPosition(_poleWorldPosition - CameraPosition), null, Color.White, 0.0f, 
                Vector2.Zero, GameBase.ScreenScale(), SpriteEffects.None, Pole_Render_Depth);
        }

        public void HandleCollision(ICollidable collider)
        {
        }

        protected override XElement Serialize(Serializer serializer)
        {
            base.Serialize(serializer);

            serializer.AddDataItem("collidable", Collidable);
            serializer.AddDataItem("collision-box", _collisionBoundingBox);
            serializer.AddDataItem("animation-engine", _animationEngine);
            serializer.AddDataItem("pole-position", _poleWorldPosition);

            return serializer.SerializedData;
        }

        protected override Serializer Deserialize(Serializer serializer)
        {
            serializer.KnownSerializedObjects.Add(_animationEngine);

            base.Deserialize(serializer);

            Collidable = serializer.GetDataItem<bool>("collidable");
            _collisionBoundingBox = serializer.GetDataItem<Rectangle>("collision-box");
            _animationEngine = serializer.GetDataItem<SpriteSheetAnimationEngine>("animation-engine");
            _poleWorldPosition = serializer.GetDataItem<Vector2>("pole-position");

            return serializer;
        }

        private const int Render_Layer = 2;

        private const float Pole_Render_Depth = 0.6f;
        public const string Pole_Texture = "flagpole";

        private const float Flag_Render_Depth = 0.61f;
        private const float Flag_Horizontal_Offset = 10.0f;
        private const float Flag_Vertical_Offset = 28.0f;
        private const string Flag_Animation_Sequence_Name = "flag";
    }
}