using System.Xml.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core;
using Leda.Core.Game_Objects.Base_Classes;
using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Animation;
using Leda.Core.Asset_Management;

namespace Bopscotch.Interface.Dialogs.RaceFinishScene
{
    public class ResultsAvatar : DialogAvatar
    {
        private Vector2 _baseWorldPosition;

        public float OffsetFromCenter { set { _baseWorldPosition.X = Definitions.Back_Buffer_Center.X + value; } }
        public float ParentDialogY { set { _baseWorldPosition.Y = value + Offset_From_Dialog_Top; } }

        public ResultsAvatar()
            : base()
        {
            _baseWorldPosition = new Vector2(Definitions.Back_Buffer_Center.X, 0.0f);

            RenderLayer = Render_Layer;
            RenderDepth = Render_Depth;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            WorldPosition = _baseWorldPosition;

            base.Draw(spriteBatch);

            spriteBatch.Draw(
                TextureManager.Textures[Podium_Texture_Name], 
                GameBase.ScreenPosition(_baseWorldPosition + new Vector2(0.0f, Podium_Y_Offset_From_Center)), 
                null,
                Color.White, 
                0.0f, 
                new Vector2(TextureManager.Textures[Podium_Texture_Name].Width / 2.0f, 0.0f), 
                GameBase.ScreenScale(), 
                SpriteEffects.None, 
                Podium_Render_Depth);
        }

        private const float Offset_From_Dialog_Top = 315.0f;
        private const int Render_Layer = 4;
        private const float Render_Depth = 0.05f;

        private const string Podium_Texture_Name = "in-dialog-podium";
        private const float Podium_Y_Offset_From_Center = 40.0f;
        private const float Podium_Render_Depth = 0.09f;
    }
}
