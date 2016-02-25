using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Asset_Management;
using Leda.Core.Game_Objects.Controllers;
using Leda.Core.Animation;

using Bopscotch.Scenes.BaseClasses;
using Bopscotch.Data;
using Bopscotch.Data.Avatar;
using Bopscotch.Interface.Objects.EditCharacter;
using Bopscotch.Interface.Dialogs;
using Bopscotch.Interface.Dialogs.Carousel;
using Bopscotch.Interface.Dialogs.EditCharacter;

namespace Bopscotch.Scenes.NonGame
{
    public class AvatarCustomisationScene : MenuDialogScene
    {
        private CustomisationDisplayAvatar _avatar;
        private AnimationController _animationController;

        public AvatarCustomisationScene()
            : base()
        {
            _animationController = new AnimationController();

            _dialogs.Add("options", new ComponentSetSelectionCarouselDialog(RegisterGameObject, UnregisterGameObject));
            _dialogs.Add("change-component", new ComponentSelectionCarouselDialog(RegisterGameObject, UnregisterGameObject));

            _avatar = new CustomisationDisplayAvatar();
            RegisterGameObject(_avatar);

            CreateAndRegisterGroundBlocks();
        }

        private void CreateAndRegisterGroundBlocks()
        {
            for (int i = 0; i < Definitions.Back_Buffer_Width + Definitions.Grid_Cell_Pixel_Size; i += Definitions.Grid_Cell_Pixel_Size)
            {
                RegisterGameObject(new CustomisationDisplayBlock(i));
            }
        }

        protected override void CompletePostStartupLoadInitialization()
        {
            base.CompletePostStartupLoadInitialization();
            CreateBackgroundForScene("background-tutorial", new int[] { 0, 1, 2 });
            _animBackground.Wrap = true;

            foreach (KeyValuePair<string, ButtonDialog> kvp in _dialogs) { kvp.Value.ExitCallback = HandleActiveDialogExit; }

            InitializeGameObjects();
        }

        private void HandleActiveDialogExit(string selectedOption)
        {
            switch (selectedOption)
            {
                case "Back":
                    if (_lastActiveDialogName == "options") { NextSceneType = typeof(TitleScene); Deactivate(); }
                    else { ActivateDialog("options"); }
                    break;
                case "Change":
                    ActivateComponentSelector();
                    break;
                case "Select":
                    _avatar.ClearSkin();
                    _avatar.SkinBones(AvatarComponentManager.SideFacingAvatarSkin(Profile.Settings.SelectedAvatarSlot));
                    ActivateDialog("options");
                    break;
            }
        }

        private void ActivateComponentSelector()
        {
            ((ComponentSelectionCarouselDialog)_dialogs["change-component"]).ClearLastSelection();

            ((ComponentSelectionCarouselDialog)_dialogs["change-component"]).ComponentSet =
                ((ComponentSetSelectionCarouselDialog)_dialogs["options"]).SelectedComponentSet;

            ActivateDialog("change-component");
        }

        public override void Activate()
        {
            _avatar.ClearSkin();
            _avatar.SkinBones(AvatarComponentManager.SideFacingAvatarSkin(Profile.Settings.SelectedAvatarSlot));
            base.Activate();
        }

        protected override void CompleteActivation()
        {
            ((ComponentSetSelectionCarouselDialog)_dialogs["options"]).ClearLastSelection();
            ActivateDialog("options");

            base.CompleteActivation();
        }

        protected override void RegisterGameObject(IGameObject toRegister)
        {
            if (toRegister is IAnimated) { _animationController.AddAnimatedObject((IAnimated)toRegister); }
            base.RegisterGameObject(toRegister);
        }

        public override void Update(GameTime gameTime)
        {
            _animationController.Update(MillisecondsSinceLastUpdate);
            base.Update(gameTime);
        }

        private const string Background_Texture_Name = "background-5";
        private const int Dialog_Height = 500;
    }
}
