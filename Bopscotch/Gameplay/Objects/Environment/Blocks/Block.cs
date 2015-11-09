using System.Xml.Linq;

using Microsoft.Xna.Framework;

using Leda.Core.Game_Objects.Base_Classes;
using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Game_Objects.Tile_Map;
using Leda.Core.Serialization;
using Leda.Core.Asset_Management;

using Bopscotch.Gameplay.Objects.Behaviours;

namespace Bopscotch.Gameplay.Objects.Environment.Blocks
{
	public class Block : DisposableSimpleDrawableObject, IBoxCollidable, ITile
    {
        protected Rectangle _collisionBoundingBox;

        public bool Collidable { get; set; }
        public virtual Rectangle CollisionBoundingBox { get { return _collisionBoundingBox; } }
        public Rectangle PositionedCollisionBoundingBox { get; set; }

        public float TopSurfaceY { get { return WorldPosition.Y + _collisionBoundingBox.Y; } }
        public float LeftSurfaceX { get { return WorldPosition.X + _collisionBoundingBox.X; } }
        public float RightSurfaceX { get { return WorldPosition.X + _collisionBoundingBox.X + _collisionBoundingBox.Width; } }

        public Block()
            : base()
        {
            Frame = new Rectangle(0, 0, Definitions.Grid_Cell_Pixel_Size, Definitions.Grid_Cell_Pixel_Size);
            RenderLayer = Render_Layer;
            RenderDepth = Render_Depth;
            Visible = true;
            Collidable = true;

            _collisionBoundingBox = new Rectangle(0, 0, Definitions.Grid_Cell_Pixel_Size, Definitions.Grid_Cell_Pixel_Size);
        }

        public void HandleCollision(ICollidable collider)
        {
            if (collider is Characters.Player.Player) { HandlePlayerCollision(collider.WorldPosition); }
        }

        protected virtual void HandlePlayerCollision(Vector2 playerWorldPosition)
        {
        }

        public virtual bool HasBeenLandedOnSquarely(Vector2 colliderWorldPosition)
        {
            return HasBeenLandedOnSquarely(colliderWorldPosition, Player_Impact_Tolerance);
        }

        protected bool HasBeenLandedOnSquarely(Vector2 colliderWorldPosition, float tolerance)
        {
            return ((colliderWorldPosition.Y <= TopSurfaceY) &&
                (colliderWorldPosition.X >= (WorldPosition.X + _collisionBoundingBox.X) - tolerance) &&
                (colliderWorldPosition.X <= WorldPosition.X + _collisionBoundingBox.X + _collisionBoundingBox.Width + tolerance));
        }

        private const float Player_Impact_Tolerance = 15.0f;

        protected const int Render_Layer = 2;
        protected const float Render_Depth = 0.5f;

        public const string Data_Node_Name = "block";
    }
}
