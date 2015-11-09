using System.Xml.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core;
using Leda.Core.Game_Objects.Base_Classes;
using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Asset_Management;
using Leda.Core.Serialization;
using Leda.Core.Animation;

namespace Bopscotch.Gameplay.Objects.Environment.Blocks
{
    public class SpikeBlock : ObstructionBlock, IAnimated
    {
        TransformationAnimationEngine _animationEngine;
        public IAnimationEngine AnimationEngine { get { return _animationEngine; } }

        public SpikeBlock()
            : base()
        {
            Texture = TextureManager.Textures[Wheel_Texture_Name];
            Origin = new Vector2(Definitions.Grid_Cell_Pixel_Size) / 2.0f;

            _animationEngine = new TransformationAnimationEngine(this);
            _animationEngine.Sequence = AnimationDataManager.Sequences["looping-rotation"];

            _collisionBoundingBox.X = -(Definitions.Grid_Cell_Pixel_Size / 2);
            _collisionBoundingBox.Y = -(Definitions.Grid_Cell_Pixel_Size / 2);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            spriteBatch.Draw(TextureManager.Textures[Outer_Texture_Name], GameBase.ScreenPosition(WorldPosition - CameraPosition), null, Color.White, 0.0f,
                new Vector2(Definitions.Grid_Cell_Pixel_Size) / 2.0f, GameBase.ScreenScale(), SpriteEffects.None, RenderDepth + Outer_Render_Depth_Offset);
        }

        private const string Wheel_Texture_Name = "block-spike-wheel";
        private const string Outer_Texture_Name = "block-spike-outer";
        private const float Outer_Render_Depth_Offset = -0.01f;

        public new const string Data_Node_Name = "spike-block";
    }
}
