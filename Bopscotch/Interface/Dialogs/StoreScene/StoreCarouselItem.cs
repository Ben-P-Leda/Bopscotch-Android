using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core;
using Leda.Core.Asset_Management;

using Bopscotch.Interface.Dialogs.Carousel;

namespace Bopscotch.Interface.Dialogs.StoreScene
{
    public class StoreCarouselItem : CarouselFlatImage
    {
        private bool _locked;
        private float _overlayRenderDepth;

        public override float RenderDepth { set { base.RenderDepth = value; _overlayRenderDepth = value - Overlay_Render_Depth_Offset; } }
        public string Annotation { private get; set; }

        public StoreCarouselItem(string selectionName, string textureReference)
            : base(selectionName, textureReference)
        {
            MasterScale = 0.2f;
            Annotation = "";
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            if (!string.IsNullOrEmpty(Annotation))
            {
                TextWriter.Write(Annotation, spriteBatch, _position + new Vector2(0.0f, Annotation_Vertical_Offset),
                    Color.Lerp(Color.Black, Color.White, DistanceFadeModifier), Color.Black, 2.0f, _overlayRenderDepth, TextWriter.Alignment.Center);

            }
        }

        private const string Lock_Texture = "icon-locked";
        private const float Overlay_Render_Depth_Offset = 0.00001f;
        private const float Annotation_Vertical_Offset = -130.0f;
    }
}
