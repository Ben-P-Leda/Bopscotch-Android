using Microsoft.Xna.Framework;

using Leda.Core.Game_Objects.Base_Classes;
using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Asset_Management;
using Leda.Core.Animation;

namespace Bopscotch.Gameplay.Objects.Environment.Signposts
{
    public class RouteMarker : StorableSimpleDrawableObject, IAnimated
    {
        private SpriteSheetAnimationEngine _animationEngine;
        public IAnimationEngine AnimationEngine { get { return _animationEngine; } }

        public RouteMarker()
            : base()
        {
            _animationEngine = new SpriteSheetAnimationEngine(this);
            _animationEngine.ID = "animation-engine";
            _animationEngine.Sequence = AnimationDataManager.Sequences[Animation_Sequence];

            RenderLayer = Render_Layer;
            RenderDepth = Render_Depth;
            Visible = true;
        }

        public void SetTexture(string textureName)
        {
            TextureReference = textureName;
            Texture = TextureManager.Textures[textureName];
            Frame = new Rectangle(0, 0, TextureManager.Textures[textureName].Width, TextureManager.Textures[textureName].Height / Frame_Count);
            Origin = new Vector2(Frame.Width, Frame.Height) / 2.0f;
        }

        private const int Frame_Count = 4;
        private const int Render_Layer = 2;
        private const float Render_Depth = 0.6f;
        private const string Animation_Sequence = "route-marker-stripes";

        public const string Data_Node_Name = "marker";
    }
}
