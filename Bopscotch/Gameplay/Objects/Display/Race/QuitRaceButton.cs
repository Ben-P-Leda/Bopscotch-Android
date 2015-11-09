using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core;
using Leda.Core.Asset_Management;

using Bopscotch.Interface.Dialogs;

namespace Bopscotch.Gameplay.Objects.Display.Race
{
	public class QuitRaceButton : InGameButtonBase
    {
        private Texture2D _iconTexture;
        private Rectangle _iconFrame;
        private Vector2 _iconOrigin;

        public Vector2 DisplayEdgePositions { set { CenterPosition = value + new Vector2(Offset_From_Right, Offset_From_Top); } }

		public QuitRaceButton()
            : base()
        {
        }

        public override void Initialize()
        {
            base.Initialize();

            _iconTexture = TextureManager.Textures[Icon_Texture_Name];
			_iconFrame = Button.IconFrame(Button.ButtonIcon.Back);
            _iconOrigin = new Vector2(Definitions.Button_Icon_Pixel_Size) / 2.0f;
			_tint = Color.Red;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            spriteBatch.Draw(_iconTexture, GameBase.ScreenPosition(Center), _iconFrame, Color.Black, 0.0f, _iconOrigin, GameBase.ScreenScale(Scale), 
                SpriteEffects.None, Icon_Render_Depth);
        }

        private const float Offset_From_Right = -40.0f;
        private const float Offset_From_Top = 170.0f;
        private const string Icon_Texture_Name = "button-icons";

		public const string In_Game_Button_Name = "quit-race";
    }
}
