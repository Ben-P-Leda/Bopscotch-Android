using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core;
using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Asset_Management;

namespace Bopscotch.Scenes.Objects
{
    public class AnimatedBackgroundSegment : IAnimatedBackgroundComponent
    {
        private Texture2D _texture;
        private Rectangle _sourceArea;
        private Vector2 _initialPosition;
        private Vector2 _position;

        private float _heightRatio;

        public AnimatedBackgroundSegment(string texture, int segmentIndex, int frameValue, int heightValue)
        {
            _texture = TextureManager.Textures[texture];

            int frameCount = _texture.Height / Unscaled_Height;
            _sourceArea = new Rectangle(0, (((segmentIndex + frameValue) % frameCount) * Unscaled_Height) + 1, _texture.Width, Unscaled_Height - 2);

            _heightRatio = (float)GameBase.Instance.GraphicsDevice.Viewport.Height / (float)Definitions.Back_Buffer_Height;
            _initialPosition = new Vector2(
                segmentIndex * TextureManager.Textures[texture].Width * Base_Scale, 
                ((Definitions.Back_Buffer_Height - (Unscaled_Height * Base_Scale)) - 2) + (Height_Step * heightValue));
        }

        public void SetOffset(Vector2 offset)
        {
            _position = _initialPosition + offset;
        }

        public void UpdateAbsolutePosition(Vector2 step, float wrapEdge)
        {
            _initialPosition += step;
            if (_initialPosition.X < -Definitions.Back_Buffer_Width)
            {
                _initialPosition.X += wrapEdge;
            }

            _position = _initialPosition;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, _position * _heightRatio, _sourceArea, Color.White, 0.0f, Vector2.Zero, _heightRatio * Base_Scale, 
                SpriteEffects.None, Render_Depth);
        }

        public static float SegmentWidth(string textureName)
        {
            return TextureManager.Textures[textureName].Width * Base_Scale;
        }

        private const int Render_Layer = 0;
        private const float Render_Depth = 0.898f;
        private const int Unscaled_Height = 225;
        private const float Height_Step = 15.0f;
        private const float Base_Scale = 2.0f;
    }
}
