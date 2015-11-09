using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core;
using Leda.Core.Game_Objects.Behaviours;

namespace Bopscotch.Interface.Content
{
    public class TextContent : ContentBase
    {
        private string _text;

        public TextWriter.Alignment Alignment { private get; set; }

        public TextContent(string text, Vector2 position)
            : base(position)
        {
            _text = text;

            Alignment = TextWriter.Alignment.Center;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            TextWriter.Write(_text, spriteBatch, _position + Offset, FadedTint, Color.Lerp(Color.Transparent, Color.Black, FadeFraction),
                Outline_Thickness, Scale, RenderDepth, Alignment);
        }

        private const float Outline_Thickness = 3;
    }
}
