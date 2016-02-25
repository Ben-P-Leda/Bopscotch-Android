using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core;
using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Asset_Management;

namespace Bopscotch.Scenes.Objects
{
    public class AnimatedBackgroundCloud: IAnimatedBackgroundComponent
    {
        private Texture2D _texture;
        private Rectangle _sourceArea;
        private float _initialX;
        private Rectangle _targetArea;
        private Color _tint;
        private float _scale;

        public int Width { get { return _targetArea.Width; } }

        public AnimatedBackgroundCloud(Point initialPosition, Vector2 scaling, Color tint)
        {
            _texture = TextureManager.Textures[Texture_Name];
            _tint = tint;

            int frameCount = _texture.Height / Unscaled_Height;
            int frame = Random.Generator.NextInt(frameCount);
            float heightRatio = (float)GameBase.Instance.GraphicsDevice.Viewport.Height / (float)Definitions.Back_Buffer_Height;
            
            _sourceArea = new Rectangle(0, frame * Unscaled_Height, _texture.Width, Unscaled_Height);

            _scale = Random.Generator.Next(0.5f, 1.0f) * heightRatio;
            _initialX = initialPosition.X * heightRatio;
            _targetArea = new Rectangle(0, (int)(initialPosition.Y * heightRatio), 
                (int)(_texture.Width * scaling.X * _scale), (int)(Unscaled_Height * scaling.Y * _scale));

            SetOffset(Vector2.Zero);
        }

        public void SetOffset(Vector2 offset)
        {
            _targetArea.X = (int)(_initialX + (offset.X * Base_Speed_Modifier * _scale));
        }

        public void UpdateAbsolutePosition(Vector2 step, float wrapEdge)
        {
            float x = _initialX + (step.X * Base_Speed_Modifier * _scale);
            if (x + _targetArea.Width < 0)
            {
                x += wrapEdge;
            }
            _initialX = x;
            _targetArea.X = (int)x;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, _targetArea, _sourceArea, Color.Lerp(Color.Transparent, _tint, 0.5f + (_scale * 0.5f)), 0.0f, Vector2.Zero, 
                SpriteEffects.None, Render_Depth);
        }

        private const string Texture_Name = "anim-background-clouds";
        private const int Render_Layer = 0;
        private const float Render_Depth = 0.899f;
        private const int Unscaled_Height = 200;
        private const float Base_Speed_Modifier = 0.2f;
    }
}
