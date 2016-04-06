namespace Bopscotch.Facebook
{
    public class FacebookConfigurator
    {
        public string ModalPrompt { get; private set; }
        public string ModalDefaultText { get; private set; }
        public string PostText { get; private set; }
        public int LivesToAdd { get; private set; }

        public void ConfigureForShareAction(ShareAction shareAction)
        {
            switch (shareAction)
            {
                case ShareAction.Progress: ConfigureForProgress(); break;
                case ShareAction.AreaComplete: ConfigureForAreaComplete(); break;
                case ShareAction.RaceWon: ConfigureForRaceWon(); break;
                case ShareAction.RaceWonAddLives: ConfigureForRaceWonWithLivesReward(); break;
            }
        }

        private void ConfigureForProgress()
        {
            ModalPrompt = "Share your progress on Facebook";
            ModalDefaultText = "I reached level " + Data.Profile.CurrentAreaData.UnlockedLevelCount.ToString() + " of the " + Data.Profile.CurrentAreaData.Name + " area in Bopscotch!";
            PostText = "I reached level " + Data.Profile.CurrentAreaData.UnlockedLevelCount.ToString() + " of the " + Data.Profile.CurrentAreaData.Name + " area in Bopscotch! Ddownload Bopscotch now for FREE on Android, iPhone, iPad and Windows Phone and see how far you can get - or challenge me to a race!";
            LivesToAdd = 0;
        }

        private void ConfigureForAreaComplete()
        {
            ModalPrompt = "Share your achievement for 10 extra lives!";
            ModalDefaultText = "I completed the " + Data.Profile.CurrentAreaData.Name + " area in Bopscotch!";
            PostText = "I just completed the " + Data.Profile.CurrentAreaData.Name + " area in Bopscotch! Ddownload Bopscotch now for FREE on Android, iPhone, iPad and Windows Phone and see if you can do it too - or challenge me to a race!";
            LivesToAdd = 10;
        }

        private void ConfigureForRaceWon()
        {
            ModalPrompt = "Say something about your victory";
            ModalDefaultText = "I won a race on Bopscotch!";
            PostText = "I just won a race in Bopscotch's " + Data.Profile.CurrentAreaData.Name + " zone! Why don't you race me sometime? Download Bopscotch now for FREE on Android, iPhone, iPad and Windows Phone.";
            LivesToAdd = 0;
        }

        private void ConfigureForRaceWonWithLivesReward()
        {
            ModalPrompt = "Say something about your victory for extra adventure lives";
            ModalDefaultText = "I won a race on Bopscotch!";
            PostText = "I just won a race in Bopscotch's " + Data.Profile.CurrentAreaData.Name + " zone! Why don't you race me sometime? Download Bopscotch now for FREE on Android, iPhone, iPad and Windows Phone.";
            LivesToAdd = 3;
        }
    }
}