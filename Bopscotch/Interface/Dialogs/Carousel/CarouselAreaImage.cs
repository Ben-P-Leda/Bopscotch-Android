using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core;
using Leda.Core.Asset_Management;

namespace Bopscotch.Interface.Dialogs.Carousel
{
    public class CarouselAreaImage : CarouselFlatImage
    {
        private bool _locked;
        private float _overlayRenderDepth;

        public override float RenderDepth { set { base.RenderDepth = value; _overlayRenderDepth = value - Overlay_Render_Depth_Offset; } }
        public string Annotation { private get; set; }

        public CarouselAreaImage(string selectionName, string textureReference, bool locked)
            : base(selectionName, textureReference)
        {
            _locked = locked;
            if (locked) { Tint = Color.Gray; }

            Annotation = "";
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            if (_locked)
            {
                spriteBatch.Draw(
                    TextureManager.Textures[Lock_Texture], 
                    GameBase.ScreenPosition(_position), 
                    null, 
                    Color.Lerp(Color.Black, Color.White, DistanceFadeModifier), 
                    0.0f, 
                    new Vector2(TextureManager.Textures[Lock_Texture].Width, TextureManager.Textures[Lock_Texture].Height) / 2.0f,
                    GameBase.ScreenScale(base.Scale / Definitions.Background_Texture_Thumbnail_Scale), 
                    SpriteEffects.None, 
                    _overlayRenderDepth);
            }

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
