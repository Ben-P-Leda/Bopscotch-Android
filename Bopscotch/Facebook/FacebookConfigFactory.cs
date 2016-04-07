namespace Bopscotch.Facebook
{
    public class FacebookConfigurator
    {
        public string ModalPrompt { get; private set; }
        public string ModalDefaultText { get; private set; }
        public string PostText { get; private set; }
        public int LivesToAdd { get; private set; }

        public void ConfigureForShareAction(ShareAction shareAction, string areaName)
        {
            switch (shareAction)
            {
                case ShareAction.Progress: ConfigureForProgress(areaName); break;
                case ShareAction.AreaComplete: ConfigureForAreaComplete(areaName); break;
                case ShareAction.RaceWon: ConfigureForRaceWon(areaName); break;
                case ShareAction.RaceWonAddLives: ConfigureForRaceWonWithLivesReward(areaName); break;
            }
        }

        private void ConfigureForProgress(string areaName)
        {
            Data.AreaDataContainer areaData = Data.Profile.GetDataForNamedArea(areaName);

            ModalPrompt = "Share your progress on Facebook";
            ModalDefaultText = "I reached level " + areaData.UnlockedLevelCount.ToString() + " of the " + areaName + " area in Bopscotch!";
            PostText = "I reached level " + areaData.UnlockedLevelCount.ToString() + " of the " + areaName + " area in Bopscotch! Download Bopscotch now for FREE on Android, iPhone, iPad and Windows Phone and see how far you can get - or challenge me to a race!";
            LivesToAdd = 0;
        }

        private void ConfigureForAreaComplete(string areaName)
        {
            ModalPrompt = "Share your achievement for 10 extra lives!";
            ModalDefaultText = "I completed the " + areaName + " area in Bopscotch!";
            PostText = "I just completed the " + areaName + " area in Bopscotch! Download Bopscotch now for FREE on Android, iPhone, iPad and Windows Phone and see if you can do it too - or challenge me to a race!";
            LivesToAdd = 10;
        }

        private void ConfigureForRaceWon(string areaName)
        {
            ModalPrompt = "Say something about your victory";
            ModalDefaultText = "I won a race on Bopscotch!";
            PostText = "I just won a race in Bopscotch's " + areaName + " zone! Why don't you race me sometime? Download Bopscotch now for FREE on Android, iPhone, iPad and Windows Phone.";
            LivesToAdd = 0;
        }

        private void ConfigureForRaceWonWithLivesReward(string areaName)
        {
            ModalPrompt = "Say something about your victory for extra adventure lives";
            ModalDefaultText = "I won a race on Bopscotch!";
            PostText = "I just won a race in Bopscotch's " + areaName + " zone! Why don't you race me sometime? Download Bopscotch now for FREE on Android, iPhone, iPad and Windows Phone.";
            LivesToAdd = 3;
        }
    }
}