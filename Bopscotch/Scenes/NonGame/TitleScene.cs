using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Android.Content;

using Leda.Core.Gamestate_Management;
using Leda.Core.Asset_Management;
using Leda.Core.Game_Objects.Controllers;
using Leda.Core.Game_Objects.Behaviours;
using Leda.Core;

using Bopscotch.Scenes.BaseClasses;
using Bopscotch.Interface;
using Bopscotch.Interface.Dialogs;
using Bopscotch.Interface.Dialogs.TitleScene;
using Bopscotch.Interface.Content;
using Bopscotch.Effects;
using Bopscotch.Effects.Popups;

namespace Bopscotch.Scenes.NonGame
{
    public class TitleScene : MenuDialogScene
    {
        private AnimationController _animationController;
        private string _firstDialog;
        private string _musicToStartOnDeactivation;
        private bool _doNotExitOnTitleDismiss;

        private NewContentUnlockedDialog _unlockNotificationDialog;

        private PopupRequiringDismissal _titlePopup;
        //private BackgroundSnow _snowController;

        public TitleScene()
            : base()
        {
            _animationController = new AnimationController();

            _titlePopup = new PopupRequiringDismissal();
            _titlePopup.AnimationCompletionHandler = HandlePopupAnimationComplete;
            RegisterGameObject(_titlePopup);

            //_snowController = new BackgroundSnow();
            //RegisterGameObject(_snowController);

            _unlockNotificationDialog = new NewContentUnlockedDialog();

            _dialogs.Add("reminder", new RateBuyReminderDialog());
            _dialogs.Add("main", new MainMenuDialog());
            _dialogs.Add("start", new StartMenuDialog());
            _dialogs.Add("survival-levels", new SurvivalStartCarouselDialog(RegisterGameObject, UnregisterGameObject));
            _dialogs.Add("characters", new CharacterSelectionCarouselDialog(RegisterGameObject, UnregisterGameObject));
            _dialogs.Add("options", new OptionsDialog());
            _dialogs.Add("keyboard", new KeyboardDialog());
            _dialogs.Add("reset-areas", new ResetAreasConfirmDialog());
            _dialogs.Add("areas-reset", new ResetAreasCompleteDialog());
            _dialogs.Add("unlocks", _unlockNotificationDialog);
            _dialogs.Add(Race_Aborted_Dialog, new DisconnectedDialog("Connection Broken - Race Aborted!"));
            _dialogs.Add("info", new InfoMenuDialog());

            BackgroundTextureName = Background_Texture_Name;

            RegisterGameObject(
                new TextContent(Translator.Translation("Leda Entertainment Presents"), new Vector2(Definitions.Back_Buffer_Center.X, 60.0f))
                {
                    RenderLayer = 2,
                    RenderDepth = 0.5f,
                    Scale = 0.65f
                });
        }

        private void HandlePopupAnimationComplete()
        {
            if (_titlePopup.AwaitingDismissal)
            {
                ActivateDialog(_firstDialog); 
                _doNotExitOnTitleDismiss = false;
            }
            else if (!_doNotExitOnTitleDismiss)
            {
                Deactivate();
            }
        }

        protected override void CompletePostStartupLoadInitialization()
        {
            base.CompletePostStartupLoadInitialization();

            _titlePopup.MappingName = Title_Texture_Name;
            //_snowController.CreateSnowflakes();

            _dialogs["reminder"].ExitCallback = HandleReminderDialogActionSelection;
            _dialogs["main"].ExitCallback = HandleMainDialogActionSelection;
            _dialogs["start"].ExitCallback = HandleStartDialogActionSelection;
            _dialogs["survival-levels"].ExitCallback = HandleLevelSelectDialogSelection;
            _dialogs["characters"].ExitCallback = HandleCharacterSelectDialogSelection;
            _dialogs["keyboard"].ExitCallback = HandleKeyboardDialogActionSelection;
            _dialogs["options"].ExitCallback = HandleOptionsDialogClose;
            _dialogs["reset-areas"].ExitCallback = HandleResetAreasConfirmDialogClose;
            _dialogs["areas-reset"].ExitCallback = HandleConfirmationDialogClose;
            _dialogs["unlocks"].ExitCallback = HandleConfirmationDialogClose;
            _dialogs[Race_Aborted_Dialog].ExitCallback = HandleConfirmationDialogClose;
            _dialogs["info"].ExitCallback = HandleInfoDialogActionSelection;
        }

        private void HandleReminderDialogActionSelection(string selectedOption)
        {
            switch (selectedOption)
            {
                case "Rate Game": RateGame(); break;
                case "Back": ActivateDialog("main"); break;
            }
        }

        private void RateGame()
        {
            MainActivity.SwitchToBrowser = true;
            Intent market = new Intent(Intent.ActionView, UrlProvider.ReviewGameUrl);
            Bopscotch.Game1.Activity.StartActivity(market);
            Data.Profile.FlagAsRated();

            if (!Data.Profile.AvatarCostumeUnlocked("Angel"))
            {
                Data.Profile.UnlockCostume("Angel");
                DisplayRatingUnlockedContent();
            }
        }

        private void HandleMainDialogActionSelection(string selectedOption)
        {
            switch (selectedOption)
            {
                case "Start!": ActivateDialog("start"); break;
                case "Character": ActivateDialog("characters"); break;
                case "Info": ActivateDialog("info"); break;
                case "Options": ActivateDialog("options"); break;
                case "Store": NextSceneType = typeof(StoreScene); Deactivate(); break;
                case "Rate": RateGame(); break;
                case "Quit": ExitGame(); break;
            }
        }

        private void HandleInfoDialogActionSelection(string selectedOption)
        {
            switch (selectedOption)
            {
                case "Rankings": NextSceneType = typeof(RankingScene); Deactivate(); break;
                case "About": NextSceneType = typeof(CreditsScene); Deactivate(); break;
                case "More Games": OpenLedaPageOnStore(); ActivateDialog("main"); break;
                case "Rate Game": RateGame(); break;
                case "Back": ActivateDialog("main"); break;
            }
        }

        private void OpenLedaPageOnStore()
        {
            MainActivity.SwitchToBrowser = true;
            Intent browser = new Intent(Intent.ActionView, UrlProvider.MoreGamesUrl);
            Bopscotch.Game1.Activity.StartActivity(browser);
        }

        private void DisplayRatingUnlockedContent()
        {
            _unlockNotificationDialog.PrepareForActivation();
            _unlockNotificationDialog.AddItem("New Costume - Angel");

            if (CurrentState == Status.Active) { ActivateDialog("unlocks"); }
            else { _firstDialog = "unlocks"; }
        }

        private void ExitGame()
        {
            DeactivationHandler = DeactivateForExit;
            NextSceneType = this.GetType();
            Deactivate();
        }

        private void DeactivateForExit(Type deactivationHandlerRequiredParameter)
        {
            MusicManager.StopMusic();
            Game.Activity.Finish();
        }

        protected override void CompleteDeactivation()
        {
            if (!string.IsNullOrEmpty(_musicToStartOnDeactivation)) { MusicManager.PlayLoopedMusic(_musicToStartOnDeactivation); }

            base.CompleteDeactivation();
        }

        private void HandleStartDialogActionSelection(string selectedOption)
        {
            switch (selectedOption)
            {
                case "Add Lives": NextSceneType = typeof(StoreScene); Deactivate(); break;
                case "Adventure": Data.Profile.PlayingRaceMode = false; ActivateDialog("survival-levels"); break;
                case "Race": HandleRaceStartSelection(); break;
                case "Back": ActivateDialog("main"); break;
            }
        }

        private void HandleLevelSelectDialogSelection(string selectedOption)
        {
            switch (selectedOption)
            {
                case "Start!":
                    NextSceneType = typeof(Gameplay.Survival.SurvivalGameplayScene);
                    _musicToStartOnDeactivation = "survival-gameplay";
                    _titlePopup.Dismiss();
                    break;
                case "Back":
                    ActivateDialog("start");
                    break;
            }
        }

        private void HandleCharacterSelectDialogSelection(string selectedOption)
        {
            switch (selectedOption)
            {
                case "Back":
                    ActivateDialog("main");
                    break;
                case "Select":
                    UpdateSelectedCharacter();
                    ActivateDialog("main");
                    break;
                case "Edit":
                    UpdateSelectedCharacter();
                    NextSceneType = typeof(AvatarCustomisationScene);
                    _musicToStartOnDeactivation = "avatar-build";
                    Deactivate();
                    break;
            }
        }

        private void HandleContentDialogClose(string selectedOption)
        {
            _titlePopup.Activate();
        }

        private void HandleOptionsDialogClose(string selectedOption)
        {
            if (selectedOption == "Reset Game") { ActivateDialog("reset-areas"); }
            else { ActivateDialog("main"); }
        }

        private void HandleResetAreasConfirmDialogClose(string selectedOption)
        {
            if (selectedOption == "Confirm") { Data.Profile.ResetAreas(); ActivateDialog("areas-reset"); }
            else { ActivateDialog("options"); }
        }

        private void HandleConfirmationDialogClose(string selectedOption)
        {
            ActivateDialog("main");
        }

        private void UpdateSelectedCharacter()
        {
            Data.Profile.Settings.SelectedAvatarSlot = ((CharacterSelectionCarouselDialog)_dialogs["characters"]).SelectedAvatarSkinSlot;
            Data.Profile.Save();
        }

        private void HandleRaceStartSelection()
        {
            if (string.IsNullOrEmpty(Data.Profile.Settings.RaceName))
            {
                _dialogs["keyboard"].Reset();
                ActivateDialog("keyboard");
            }
            else
            {
                Data.Profile.PlayingRaceMode = true;
                NextSceneType = typeof(Gameplay.Race.RaceStartScene);
                _titlePopup.Dismiss();
            }
        }

        private void HandleKeyboardDialogActionSelection(string selectedOption)
        {
            switch (selectedOption)
            {
                case "Back": 
                    ActivateDialog("start"); 
                    break;
                case "OK": 
                    Data.Profile.Settings.RaceName = ((KeyboardDialog)_dialogs["keyboard"]).Entry;
                    Data.Profile.Save();
                    HandleRaceStartSelection();
                    break;
            }
        }

        protected override void RegisterGameObject(IGameObject toRegister)
        {
            if (toRegister is IAnimated) { _animationController.AddAnimatedObject((IAnimated)toRegister); }
            base.RegisterGameObject(toRegister);
        }

        public override void Activate()
        {
            if (!NextSceneParameters.Get<bool>("music-already-running")) { MusicManager.PlayLoopedMusic("title"); }

            _musicToStartOnDeactivation = "";

            base.Activate();
        }

        protected override void CompleteActivation()
        {
            _firstDialog = NextSceneParameters.Get<string>(First_Dialog_Parameter_Name);

            if (_firstDialog == Rate_Game_Dialog) { DisplayRatingUnlockedContent(); }
            else if (string.IsNullOrEmpty(_firstDialog)) { _firstDialog = Default_First_Dialog; }
            else if ((_firstDialog == "start") && (Data.Profile.RateBuyRemindersOn)) { _firstDialog = Reminder_Dialog; }

            if (_firstDialog != "unlocks") { UnlockFullVersionContent(); }

            _titlePopup.Activate(); 
            _doNotExitOnTitleDismiss = false;

            base.CompleteActivation();
        }

        private void UnlockFullVersionContent()
        {
            _unlockNotificationDialog.PrepareForActivation();

            if ((Data.Profile.AreaIsLocked("Waterfall")) && (Data.Profile.AreaHasBeenCompleted("Hilltops")))
            {
                Data.Profile.UnlockNamedArea("Waterfall");
                _unlockNotificationDialog.AddItem("New Levels - Waterfall Area");
            }

            if (!Data.Profile.AvatarCostumeUnlocked("Wizard"))
            {
                Data.Profile.UnlockCostume("Wizard");
                Data.Profile.UnlockCostume("Mummy");
                _unlockNotificationDialog.AddItem("New Costumes - Wizard, Mummy");
            }

            if (_unlockNotificationDialog.HasContent) { _firstDialog = "unlocks"; }
        }

        public override void Update(GameTime gameTime)
        {
            _animationController.Update(MillisecondsSinceLastUpdate);
            //_snowController.Update(MillisecondsSinceLastUpdate);

            base.Update(gameTime);
        }

        protected override void HandleBackButtonPress()
        {
            if ((!_titlePopup.AwaitingDismissal) && (CurrentState != Status.Deactivating) && (!_doNotExitOnTitleDismiss)) { ExitGame(); }

            base.HandleBackButtonPress();
        }

        private const string Background_Texture_Name = "background-1";
        private const string Title_Texture_Name = "popup-title";
        private const string Default_First_Dialog = "main";
        private const string Reminder_Dialog = "reminder";

        public const string First_Dialog_Parameter_Name = "first-dialog";
        public const string Race_Aborted_Dialog = "race-aborted";
        public const string Rate_Game_Dialog = "unlocks-rating";
    }
}
