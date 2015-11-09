using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core;
using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Asset_Management;

namespace Bopscotch.Effects
{
    public class FullScreenColourOverlay : IGameObject, ISimpleRenderable
    {
        public bool Visible { get { return true; } set { } }
        public int RenderLayer { get { return Render_Layer; } set { } }
        public Color Tint { private get; set; }
        public float TintFraction { private get; set; }

        public FullScreenColourOverlay()
        {
            Tint = Color.White;
        }

        public void Initialize()
        {
        }

        public void Reset()
        {
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Vector2 topLeft = GameBase.ScreenPosition(0.0f, 0.0f);
            Rectangle targetArea = new Rectangle((int)topLeft.X, (int)topLeft.Y, 
                (int)GameBase.ScreenScale(Definitions.Back_Buffer_Width), (int)GameBase.ScreenScale(Definitions.Back_Buffer_Height));

            spriteBatch.Draw(TextureManager.Textures["pixel"], targetArea, null, Color.Lerp(Color.Transparent, Tint, TintFraction), 0.0f, Vector2.Zero,
                SpriteEffects.None, 0.05f);
        }

        private const int Render_Layer = 0;
    }
}
