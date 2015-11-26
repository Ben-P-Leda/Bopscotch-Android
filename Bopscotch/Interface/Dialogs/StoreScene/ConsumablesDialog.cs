using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core;
using Leda.Core.Asset_Management;

using Bopscotch.Data;

namespace Bopscotch.Interface.Dialogs.StoreScene
{
    public class ConsumablesDialog : ButtonDialog
    {
        public ConsumablesDialog()
            : base()
        {
            Height = Dialog_Height;
            TopYWhenActive = Bopscotch.Scenes.NonGame.StoreScene.Dialog_Margin;
            _boxCaption = Translator.Translation("Bopscotch Store");
        }

        public override void Reset()
        {
            base.Reset();
            WorldPosition = new Vector2(0.0f, -Height);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            RenderConsumableInfo(spriteBatch, Lives_Icon_Texture, 1.0f, Profile.Lives, Definitions.Left_Button_Column_X);
            RenderConsumableInfo(spriteBatch, Golden_Ticket_Texture, 1.5f, Profile.GoldenTickets, Definitions.Right_Button_Column_X);

            base.Draw(spriteBatch);
        }

        private void RenderConsumableInfo(SpriteBatch spriteBatch, string iconName, float iconScale, int totalUnits, float centerX)
        {
            string text = Translator.Translation("unit-count").Replace("[QUANTITY]", totalUnits.ToString());
            float width = (TextureManager.Textures[iconName].Width * iconScale) + TextWriter.Dimensions(text).X;
            Vector2 position = new Vector2(centerX - (width / 2.0f), WorldPosition.Y + Top_Margin);

            spriteBatch.Draw(TextureManager.Textures[iconName], GameBase.ScreenPosition(position), null, Color.White, 0.0f, Vector2.Zero, 
                GameBase.ScreenScale(iconScale), SpriteEffects.None, Render_Depth);

            TextWriter.Write(text, spriteBatch, position + new Vector2(TextureManager.Textures[iconName].Width * iconScale, 0.0f), Color.White, 
                Color.Black, Outline_Thickness, Text_Scale, Render_Depth, TextWriter.Alignment.Left);
        }

        private const int Dialog_Height = 220;
        private const string Golden_Ticket_Texture = "golden-ticket";
        private const string Lives_Icon_Texture = "icon-lives";
        protected const float Render_Depth = 0.1f;
        protected const float Outline_Thickness = 3.0f;
        protected const float Text_Scale = 0.75f;
        protected const float Left_Margin = 100.0f;
        protected const float Top_Margin = 105.0f;
    }
}
