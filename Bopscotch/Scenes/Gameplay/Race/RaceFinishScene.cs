﻿using Microsoft.Xna.Framework;

using Leda.Core.Game_Objects.Controllers;
using Leda.Core.Game_Objects.Behaviours;

using Bopscotch.Data;
using Bopscotch.Scenes.BaseClasses;
using Bopscotch.Scenes.NonGame;
using Bopscotch.Interface.Dialogs.RaceFinishScene;

namespace Bopscotch.Scenes.Gameplay.Race
{
    public class RaceFinishScene : MenuDialogScene
    {
        private AnimationController _animationController;
        private Bopscotch.Facebook.ShareAction _exitShareAction;

        public RaceFinishScene()
            : base()
        {
            _animationController = new AnimationController();

            _dialogs.Add("results", new ResultsDialog(RegisterGameObject) { ExitCallback = ReturnToTitleScene } );
        }

        private void ReturnToTitleScene(string buttonCaption)
        {
            NextSceneParameters.Clear();
            NextSceneParameters.Set(TitleScene.First_Dialog_Parameter_Name, "start");
            NextSceneParameters.Set("share-action", _exitShareAction);
            NextSceneType = typeof(TitleScene);
            Deactivate();
        }

        protected override void CompletePostStartupLoadInitialization()
        {
            base.CompletePostStartupLoadInitialization();
            CreateBackgroundForScene(Background_Texture_Name, new int[] { 0, 1, 2, 3 });

            ((ResultsDialog)_dialogs["results"]).InitializeComponents();
        }

        protected override void RegisterGameObject(IGameObject toRegister)
        {
            if (toRegister is IAnimated) { _animationController.AddAnimatedObject((IAnimated)toRegister); }
            base.RegisterGameObject(toRegister);
        }

        public override void Activate()
        {
            base.Activate();

            Definitions.RaceOutcome outcome = NextSceneParameters.Get<Definitions.RaceOutcome>(Outcome_Parameter_Name);
            bool awardLives = NextSceneParameters.Get<bool>("race-lives-awarded");

            ((ResultsDialog)_dialogs["results"]).Outcome = outcome;
            ((ResultsDialog)_dialogs["results"]).LivesAwarded = awardLives;

            if (outcome == Definitions.RaceOutcome.OwnPlayerWin)
            {
                _exitShareAction = awardLives ? Bopscotch.Facebook.ShareAction.RaceWonAddLives : Bopscotch.Facebook.ShareAction.RaceWon;
            }
            else
            {
                _exitShareAction = Facebook.ShareAction.None;
            }
        }

        protected override void CompleteActivation()
        {
            base.CompleteActivation();

            ActivateDialog("results");
        }

        public override void Update(GameTime gameTime)
        {
            _animationController.Update(MillisecondsSinceLastUpdate);

            base.Update(gameTime);
        }

        private const string Background_Texture_Name = "background-2";

        public const string Outcome_Parameter_Name = "race-outcome";
    }
}