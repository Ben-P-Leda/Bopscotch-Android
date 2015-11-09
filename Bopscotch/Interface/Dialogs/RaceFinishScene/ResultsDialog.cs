using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

using Leda.Core;
using Leda.Core.Gamestate_Management;
using Leda.Core.Game_Objects.Base_Classes;
using Leda.Core.Asset_Management;
using Leda.Core.Timing;

using Bopscotch.Data;
using Bopscotch.Data.Avatar;
using Bopscotch.Gameplay.Objects.Characters.Player;

namespace Bopscotch.Interface.Dialogs.RaceFinishScene
{
    public class ResultsDialog : ButtonDialog
    {
        private Scene.ObjectRegistrationHandler _registerObject;

        private ResultsAvatar _avatar;
        private Timer _resultsAnnouncementTimer;
        private ResultsPopup _outcomePopup;
        private Effects.GlowBurst _glowBurst;
        private SoundEffectInstance _resultsSoundInstance;

        public Definitions.RaceOutcome Outcome { private get; set; }

        public ResultsDialog(Scene.ObjectRegistrationHandler objectRegistrationHandler)
            : base()
        {
            _registerObject = objectRegistrationHandler;

            _boxCaption = Translator.Translation("Race Results");

            Height = Dialog_Height;
            TopYWhenActive = Top_Y_When_Active;

            AddButton("Exit", new Vector2(Definitions.Left_Button_Column_X, 625), Button.ButtonIcon.Play, Color.Red, 0.7f);

            _defaultButtonCaption = "Exit";
            _cancelButtonCaption = "Exit";

            _resultsAnnouncementTimer = new Timer("", AnnounceResults);
            GlobalTimerController.GlobalTimer.RegisterUpdateCallback(_resultsAnnouncementTimer.Tick);
        }

        private void AnnounceResults()
        {
            switch (Outcome)
            {
                case Definitions.RaceOutcome.OwnPlayerWin:
                    _avatar.AnimationEngine.Sequence = AnimationDataManager.Sequences["player-front-win"];
                    SoundEffectManager.PlayEffect("race-winner");
                    _glowBurst.Visible = true;
                    break;
                case Definitions.RaceOutcome.OpponentPlayerWin:
                    _avatar.AnimationEngine.Sequence = AnimationDataManager.Sequences["player-front-lose"];
                    SoundEffectManager.PlayEffect("race-loser");
                    break;
                case Definitions.RaceOutcome.Incomplete:
                    _avatar.AnimationEngine.Sequence = AnimationDataManager.Sequences["player-front-lose"];
                    SoundEffectManager.PlayEffect("race-loser");
                    break;
            }

            _outcomePopup.Activate();
        }

        public void InitializeComponents()
        {
            _avatar = new ResultsAvatar();
            _avatar.CreateBonesFromDataManager(Definitions.Avatar_Skeleton_Front);
            _registerObject(_avatar);

            _outcomePopup = new ResultsPopup();
            _registerObject(_outcomePopup);

            _glowBurst = new Effects.GlowBurst(3, 1.0f, 0.1f);
            _glowBurst.RenderDepth = 0.095f;
        }

        public override void Activate()
        {
            base.Activate();

            _avatar.SkinBones(AvatarComponentManager.FrontFacingAvatarSkin(Profile.Settings.SelectedAvatarSlot));
            _avatar.StartRestingAnimationSequence();

            SynchroniseComponentsWithDialog();

            _outcomePopup.Outcome = Outcome;
            _outcomePopup.Reset();

            _resultsAnnouncementTimer.NextActionDuration = Delay_Before_Result_Announcement_In_Milliseconds;
            _resultsSoundInstance = SoundEffectManager.PlayEffect("race-results");

            _glowBurst.Visible = false;
        }

        private void SynchroniseComponentsWithDialog()
        {
            _avatar.ParentDialogY = WorldPosition.Y;
            _outcomePopup.ParentDialogY = WorldPosition.Y;
            _glowBurst.Position = _avatar.WorldPosition;
        }

        public override void Update(int millisecondsSinceLastUpdate)
        {
            base.Update(millisecondsSinceLastUpdate);
            SynchroniseComponentsWithDialog();

            _glowBurst.Update(millisecondsSinceLastUpdate);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            _avatar.Draw(spriteBatch);
            _glowBurst.Draw(spriteBatch);
        }

        private const float Top_Y_When_Active = 100.0f;
        private const int Dialog_Height = 700;
        private const int Delay_Before_Result_Announcement_In_Milliseconds = 3000;
    }
}
