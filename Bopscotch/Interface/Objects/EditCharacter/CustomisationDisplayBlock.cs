using Microsoft.Xna.Framework;

using Leda.Core.Game_Objects.Base_Classes;
using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Asset_Management;
using Leda.Core.Motion;

namespace Bopscotch.Interface.Objects.EditCharacter
{
    public class CustomisationDisplayBlock : DisposableSimpleDrawableObject, IMobile
    {
        private CustomisationDisplayBlockMotionEngine _motionEngine;
        public IMotionEngine MotionEngine { get { return _motionEngine; } }

        public CustomisationDisplayBlock(float initialXPosition)
            : base()
        {
            _motionEngine = new CustomisationDisplayBlockMotionEngine();

            WorldPosition = new Vector2(initialXPosition, Top_Y_Position);
            RenderLayer = Render_Layer;
            Visible = true;
        }

        public override void Initialize()
        {
            Texture = TextureManager.Textures[Texture_Name];
            Frame = TextureManager.Textures[Texture_Name].Bounds;
            Origin = Vector2.Zero;
        }

        public void Update(int millisecondsSinceLastUpdate)
        {
            _motionEngine.Update(millisecondsSinceLastUpdate);

            WorldPosition += _motionEngine.Delta;

            if (WorldPosition.X < -Definitions.Grid_Cell_Pixel_Size)
            {
                WorldPosition = new Vector2(WorldPosition.X + Definitions.Back_Buffer_Width + Definitions.Grid_Cell_Pixel_Size, WorldPosition.Y);
            }
        }

        private const float Top_Y_Position = 340.0f;
        private const int Render_Layer = 2;
        private const string Texture_Name = "block-1";
    }
}
