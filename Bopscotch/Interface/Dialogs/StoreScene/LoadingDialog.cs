using Microsoft.Xna.Framework;

using Leda.Core;
using Leda.Core.Asset_Management;

using Bopscotch;

namespace Bopscotch.Interface.Dialogs.StoreScene
{
    public class LoadingDialog : ButtonDialog
    {
        public delegate void EntryCompleteHandler();
        private EntryCompleteHandler _entryCompleteHandler;
        private float _spinnerRotation;

        public LoadingDialog(EntryCompleteHandler entryCompleteHandler)
            : base()
        {
            Height = Dialog_Height;
            TopYWhenActive = Definitions.Back_Buffer_Center.Y - (Dialog_Height / 2.0f);

            _boxCaption = Translator.Translation(Dialog_Caption);
            _entryCompleteHandler = entryCompleteHandler;
        }

        public override void Reset()
        {
            base.Reset();
            WorldPosition = new Vector2(0.0f, -Height);

            _spinnerRotation = 0.0f;
        }

        protected override void HandleDialogEntryCompletion()
        {
            base.HandleDialogEntryCompletion();

            if (_entryCompleteHandler != null)
            {
                _entryCompleteHandler();
            }
        }

        public override void Update(int millisecondsSinceLastUpdate)
        {
            base.Update(millisecondsSinceLastUpdate);

            _spinnerRotation -= MathHelper.ToRadians(Spin_Degrees_Per_Millisecond) * millisecondsSinceLastUpdate;
        }

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            spriteBatch.Draw(
                TextureManager.Textures["load-spinner"],
                GameBase.ScreenPosition(Definitions.Back_Buffer_Center.X, WorldPosition.Y + 175.0f),
                null,
                Color.White,
                _spinnerRotation,
                new Vector2(TextureManager.Textures["load-spinner"].Width, TextureManager.Textures["load-spinner"].Height) / 2.0f,
                GameBase.ScreenScale(),
                Microsoft.Xna.Framework.Graphics.SpriteEffects.None,
                0.1f);
        }

        private const int Dialog_Height = 400;
        private const string Dialog_Caption = "loading";
        private const float Spin_Degrees_Per_Millisecond = 0.1f;
        private const float Top_Line_Y = 80.0f;
        private const float Line_Height = 50.0f;
    }
}
