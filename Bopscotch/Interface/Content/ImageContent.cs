using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core;
using Leda.Core.Asset_Management;
using Leda.Core.Game_Objects.Behaviours;

namespace Bopscotch.Interface.Content
{
    public class ImageContent : ContentBase
    {
        private string _textureReference;

        public Rectangle Frame { private get; set; }
        public Vector2 Origin { private get; set; }

        public ImageContent(string textureReference, Vector2 position)
            : base(position)
        {
            _textureReference = textureReference;

            Frame = Rectangle.Empty;
            Origin = Vector2.Zero;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Frame == Rectangle.Empty)
            {
                Frame = TextureManager.Textures[_textureReference].Bounds;
                Origin = new Vector2(Frame.Width, Frame.Height) / 2.0f;
            }

            spriteBatch.Draw(TextureManager.Textures[_textureReference], GameBase.ScreenPosition(_position + Offset), Frame, FadedTint, 0.0f, Origin, 
                GameBase.ScreenScale(Scale), SpriteEffects.None, RenderDepth);
        }
    }
}
