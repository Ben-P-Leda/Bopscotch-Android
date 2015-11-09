using System.Xml.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core;
using Leda.Core.Game_Objects.Base_Classes;
using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Asset_Management;
using Leda.Core.Serialization;

namespace Bopscotch.Gameplay.Objects.Environment.Signposts
{
    public abstract class SignpostBase : StorableSimpleDrawableObject, IBoxCollidable
    {
        private Rectangle _collisionBoundingBox;

        public bool Collidable { get; set; }
        public Rectangle CollisionBoundingBox { get { return _collisionBoundingBox; } }
        public Rectangle PositionedCollisionBoundingBox { get; set; }

        public SignpostBase()
            : base()
        {
            Frame = new Rectangle(0, 0, Definitions.Grid_Cell_Pixel_Size, Definitions.Grid_Cell_Pixel_Size);
            Origin = new Vector2(Frame.Width, Frame.Height) / 2.0f;
            RenderLayer = Render_Layer;
            RenderDepth = Plate_Render_Depth;
            Visible = true;
            Collidable = true;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
 	        base.Draw(spriteBatch);

            spriteBatch.Draw(
                TextureManager.Textures[Pole_Texture],
                GameBase.ScreenPosition(WorldPosition - CameraPosition),
                null,
                Color.White,
                0.0f,
                new Vector2(TextureManager.Textures[Pole_Texture].Width / 2.0f, -Pole_Vertical_Offset),
                GameBase.ScreenScale(),
                SpriteEffects.None,
                Pole_Render_Depth);
        }

        public void SetCollisionBoundingBox(float verticalOverflow)
        {
            _collisionBoundingBox = new Rectangle(
                -(Definitions.Grid_Cell_Pixel_Size / 2),
                (int)(verticalOverflow - Plate_Vertical_Offset),
                Definitions.Grid_Cell_Pixel_Size,
                (Definitions.Grid_Cell_Pixel_Size * 2) - (int)verticalOverflow);
        }

        public void HandleCollision(ICollidable collider)
        {
        }

        protected override XElement Serialize(Serializer serializer)
        {
            base.Serialize(serializer);

            serializer.AddDataItem("collidable", Collidable);
            serializer.AddDataItem("collision-box", _collisionBoundingBox);

            return serializer.SerializedData;
        }

        protected override Serializer Deserialize(Serializer serializer)
        {
            base.Deserialize(serializer);

            Collidable = serializer.GetDataItem<bool>("collidable");
            _collisionBoundingBox = serializer.GetDataItem<Rectangle>("collision-box");

            return serializer;
        }

        private const int Render_Layer = 2;
        private const float Plate_Render_Depth = 0.6f;
        private const float Pole_Render_Depth = 0.61f;
        private const string Pole_Texture = "signpost";
        private const float Pole_Vertical_Offset = 17.0f;

        public const float Plate_Vertical_Offset = 10;
    }
}
