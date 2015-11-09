using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core;
using Leda.Core.Asset_Management;

#if WINDOWS_PHONE
using Microsoft.Xna.Framework.GamerServices;
#endif

namespace Bopscotch
{
    public class Game1 : GameBase
    {
		public Game1()
			: base(Orientation.Landscape)
        {
            TombstoneFileName = Tombstone_File_Name;
        }

        protected override void Initialize()
        {
            Communication.InterDeviceCommunicator communicator = new Communication.InterDeviceCommunicator();

            Data.Profile.Initialize();

            AddScene(new Scenes.NonGame.StartupLoadingScene());
            AddScene(new Scenes.NonGame.TitleScene());
            AddScene(new Scenes.NonGame.CreditsScene());
            AddScene(new Scenes.NonGame.StoreScene());
            AddScene(new Scenes.NonGame.AvatarCustomisationScene());
            AddScene(new Scenes.Gameplay.Survival.SurvivalGameplayScene());
            AddScene(new Scenes.Gameplay.Survival.SurvivalAreaCompleteScene());
            AddScene(new Scenes.Gameplay.Race.RaceStartScene() { Communicator = communicator });
            AddScene(new Scenes.Gameplay.Race.RaceGameplayScene() { Communicator = communicator });
            AddScene(new Scenes.Gameplay.Race.RaceFinishScene());

            base.Initialize();

            SetResolutionMetrics(Definitions.Back_Buffer_Width, Definitions.Back_Buffer_Height, ScalingAxis.X);
            SceneTransitionCrossFadeTextureName = "pixel";

            StartInitialScene(typeof(Scenes.NonGame.StartupLoadingScene));
        }

        protected override void LoadContent()
        {
            base.LoadContent();
            TextureManager.AddTexture("leda-logo", Content.Load<Texture2D>("Textures\\leda-logo"));
            TextureManager.AddTexture("pixel", Content.Load<Texture2D>("Textures\\WhitePixel"));
			TextureManager.AddTexture("load-spinner", Content.Load<Texture2D>("Textures\\load-spinner"));
        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            MusicManager.StopMusic();

            base.OnExiting(sender, args);
        }

        public const string Tombstone_File_Name = "ts-temp.xml";


    }
}
