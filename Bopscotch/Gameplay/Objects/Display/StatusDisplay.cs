using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core;
using Leda.Core.Game_Objects.Behaviours;

using Bopscotch.Data;

namespace Bopscotch.Gameplay.Objects.Display
{
    public class StatusDisplay : ISimpleRenderable
    {
        protected Vector2 _position;

        public int RenderLayer { get { return Render_Layer; } set { } }
        public bool Visible { get; set; }

        public Vector2 Position { set { _position = value + new Vector2(Left, Top); } }

        public StatusDisplay()
        {
            Visible = true;
        }

        public void Initialize()
        {
        }

        public void Reset()
        {
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
        }

        private const int Render_Layer = 4;
        private const float Left = 10.0f;
        private const float Top = -10.0f;

        protected const float Render_Depth = 0.3f;
        protected const float Outline_Thickness = 3.0f;
        protected const float Text_Scale = 0.75f;
        protected const float Line_Height = 50.0f;
    }
}
