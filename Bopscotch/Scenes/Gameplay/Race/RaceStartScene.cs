using System;
using System.Linq;

using Microsoft.Xna.Framework;

using Leda.Core;
using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Gamestate_Management;
using Leda.Core.Game_Objects.Controllers;
using Leda.Core.Asset_Management;

using Bopscotch.Scenes.BaseClasses;
using Bopscotch.Scenes.NonGame;
using Bopscotch.Interface.Dialogs;
using Bopscotch.Interface.Dialogs.Carousel;
using Bopscotch.Interface.Dialogs.RaceJoinScene;

namespace Bopscotch.Scenes.Gameplay.Race
{
    public class RaceStartScene : MenuDialogScene
    {
        public Communication.InterDeviceCommunicator Communicator { private get; set; }

        public RaceStartScene()
            : base()
        {
        }

        public override void HandleAssetLoadCompletion(Type loaderSceneType)
        {
            if (loaderSceneType == typeof(StartupLoadingScene)) { CreateDialogs(); }

            base.HandleAssetLoadCompletion(loaderSceneType);
        }

        private void CreateDialogs()
        {
            _dialogs.Add("joinoptions", new JoinRaceOptionsDialog(GameBase.SafeDisplayArea.X, GameBase.SafeDisplayArea.Width));
            _dialogs["joinoptions"].SelectionCallback = HandleOpponentStepButtonTouch;
            _dialogs["joinoptions"].ExitCallback = HandleJoinOptionsDialogActionSelection;

            _dialogs.Add("keyboard", new KeyboardDialog());
            _dialogs["keyboard"].ExitCallback = HandleKeyboardDialogActionSelection;

            _dialogs.Add("opponents", new RaceOpponentListDialog(GameBase.SafeDisplayArea.X, GameBase.SafeDisplayArea.Width));
            _dialogs["opponents"].SelectionCallback = HandleOpponentStepButtonTouch;
            _dialogs["opponents"].ExitCallback = HandleOpponentSelection;
            ((RaceOpponentListDialog)_dialogs["opponents"]).Communicator = Communicator;

            _dialogs.Add("courses", new CourseSelectionCarouselDialog(RegisterGameObject, UnregisterGameObject));
            _dialogs["courses"].SelectionCallback = HandleCourseDialogActionSelection;
            ((CourseSelectionCarouselDialog)_dialogs["courses"]).Communicator = Communicator;

            _dialogs.Add("disconnected", new DisconnectedDialog());
            _dialogs["disconnected"].SelectionCallback = HandleDisconnectionDialogClose;
        }

        protected override void CompletePostStartupLoadInitialization()
        {
            base.CompletePostStartupLoadInitialization();
            CreateBackgroundForScene(Background_Texture_Name, new int[] { 0, 1, 2 });
        }

        private void HandleOpponentStepButtonTouch(string buttonCaption)
        {
            if (_dialogs["joinoptions"].Active) { _dialogs["joinoptions"].DismissWithReturnValue(""); }
            if (_dialogs["opponents"].Active) { _dialogs["opponents"].DismissWithReturnValue(""); }
        }

        private void HandleJoinOptionsDialogActionSelection(string selectedOption)
        {
            switch (selectedOption)
            {
                case "Back": ReturnToTitleScene(); break;
                case "Change": ((KeyboardDialog)_dialogs["keyboard"]).Entry = Data.Profile.Settings.RaceName; ActivateDialog("keyboard"); break;
            }
        }

        private void ReturnToTitleScene()
        {
            NextSceneType = typeof(TitleScene);
            NextSceneParameters.Set("music-already-running", true);
            Deactivate();
        }

        private void HandleKeyboardDialogActionSelection(string selectedOption)
        {
            if (selectedOption == "OK")
            {
                Data.Profile.Settings.RaceName = ((KeyboardDialog)_dialogs["keyboard"]).Entry;
                Data.Profile.Save();
            }

            ReactivateOpponentSelectionDialogs();
        }

        private void ReactivateOpponentSelectionDialogs()
        {
            ActivateDialog("opponents");
            ActivateDialog("joinoptions");
        }

        private void HandleOpponentSelection(string selectedOpponentRaceID)
        {
            if (!string.IsNullOrEmpty(selectedOpponentRaceID)) 
            {
                Communicator.OtherPlayerRaceID = selectedOpponentRaceID;
                ActivateDialog("courses"); 
            }
        }

        private void HandleCourseDialogActionSelection(string selectedOption)
        {
            switch (selectedOption)
            {
                case "Back": ReactivateOpponentSelectionDialogs(); break;
                case "Disconnected": HandleOpponentConnectionLoss(); break;
                default: StartRaceForSelectedCourse(((CourseSelectionCarouselDialog)_dialogs["courses"]).SelectedCourseName); break;
            }
        }

        private void HandleOpponentConnectionLoss()
        {
            Communicator.OtherPlayerRaceID = "";
            ActivateDialog("disconnected");
        }

        private void HandleDisconnectionDialogClose(string buttonCaption)
        {
            ReactivateOpponentSelectionDialogs();
        }

        private void StartRaceForSelectedCourse(string courseName)
        {
            var areaData = (from el in Data.Profile.SimpleAreaData where el.Attribute("name").Value == courseName select el).First();
            NextSceneParameters.Set(RaceGameplayScene.Course_Area_Parameter, courseName);
            NextSceneParameters.Set(RaceGameplayScene.Course_Speed_Parameter, (int)areaData.Attribute("speed"));

            Data.Profile.DecreasePlaysToNextRatingReminder();

            NextSceneType = typeof(RaceGameplayScene);
            Deactivate();
        }

        public override void Activate()
        {
            Communicator.Message = "";

            base.Activate();
        }

        protected override void CompleteActivation()
        {
            base.CompleteActivation();

            ActivateDialog("opponents");
            ActivateDialog("joinoptions");
        }

        protected override void CompleteDeactivation()
        {
            ((RaceOpponentListDialog)_dialogs["opponents"]).TearDownCommunicator();

            if (NextSceneParameters.Get<string>(Gameplay.Race.RaceGameplayScene.Course_Area_Parameter) != null) { MusicManager.StopMusic(); }

            base.CompleteDeactivation();
        }

		private bool _doNotResumeMusic = false;

#if __IOS__
		public override void HandleGameActivated ()
		{
			if (!_doNotResumeMusic) { MusicManager.PlayLoopedMusic ("title"); }
			_doNotResumeMusic = false;
		}

		public override void HandleGameDeactivated()
		{
			_doNotResumeMusic = true;
			base.HandleGameDeactivated();
		}
#endif

        private const string Background_Texture_Name = "background-2";
        private const string Title_Texture_Name = "popup-join-race";
        private const float Title_Popup_Y = 60.0f;
    }
}
