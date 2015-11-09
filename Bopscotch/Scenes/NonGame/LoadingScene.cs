using System;
using System.Xml.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

//using Windows.ApplicationModel.Store;

using Leda.Core;
using Leda.Core.Renderable;
using Leda.Core.Game_Objects.Base_Classes;
using Leda.Core.Gamestate_Management;
using Leda.Core.Asset_Management;

namespace Bopscotch.Scenes.NonGame
{
    public class StartupLoadingScene : AssetLoaderScene
    {
        private int _progressBarX;
		private int _progressBarY;
		private int _progressBarWidth;
		private DisposableSimpleDrawableObject _progressGaugeSpinner;

        public StartupLoadingScene()
            : base()
        {
            NextSceneType = typeof(TitleScene); 
			ClearColour = Color.White;

			_progressGaugeSpinner = null;
        }

		public override void Update (GameTime gameTime)
		{
			_progressGaugeSpinner.Rotation -= MathHelper.ToRadians(Spin_Degrees_Per_Millisecond) * MillisecondsSinceLastUpdate;

			base.Update(gameTime);
		}

        protected override void Render()
        {
            base.Render();

            SpriteBatch.Begin();

            SpriteBatch.Draw(
                TextureManager.Textures[Logo_Texture],
                GameBase.ScreenPosition(Definitions.Back_Buffer_Center),
                null,
                Color.White,
                0.0f,
                new Vector2(TextureManager.Textures[Logo_Texture].Width, TextureManager.Textures[Logo_Texture].Height) / 2.0f,
                GameBase.ScreenScale(),
                SpriteEffects.None,
                0.5f);

            Vector2 topLeft = GameBase.ScreenPosition(_progressBarX, _progressBarY);
            Rectangle targetArea = new Rectangle(
                (int)topLeft.X, (int)topLeft.Y, (int)GameBase.ScreenScale(_progressBarWidth * AssetLoadProgress), (int)GameBase.ScreenScale(Progress_Bar_Height));

            SpriteBatch.Draw(TextureManager.Textures[Pixel_Texture], targetArea, Color.CornflowerBlue);

            SpriteBatch.End();
        }

        public override void Activate()
        {
            AssetListFileName = Asset_File_Name_And_Path;

			if (_progressGaugeSpinner == null) { SetupProgressGauge (); }

            base.Activate();
        }

		private void SetupProgressGauge()
        {
			float progressGaugeLeftEdge = Definitions.Back_Buffer_Center.X - (Progress_Gauge_Total_Width / 2);
			float spinnerRadius = TextureManager.Textures [Spinner_Texture].Width / 2.0f;

			_progressGaugeSpinner = new DisposableSimpleDrawableObject () 
			{
				Texture = TextureManager.Textures[Spinner_Texture],
				Frame = TextureManager.Textures[Spinner_Texture].Bounds,
				Origin = new Vector2(spinnerRadius, spinnerRadius),
				RenderLayer = 2,
				Visible = true
			};

			_progressGaugeSpinner.WorldPosition = new Vector2(progressGaugeLeftEdge + spinnerRadius, Progress_Gauge_Center_Y);
			RegisterGameObject(_progressGaugeSpinner);

			Vector2 progressBarContainerTopLeft = new Vector2(
				progressGaugeLeftEdge + ((spinnerRadius * 2.0f) + Progress_Container_Margin),
				Progress_Gauge_Center_Y - ((Progress_Bar_Height / 2.0f) + Progress_Container_Margin));
            
			Point progressBarContainerDimensions = new Point(
				Progress_Gauge_Total_Width - (int)((spinnerRadius * 2.0f) + Progress_Container_Margin),
				Progress_Bar_Height + (Progress_Container_Margin * 2));

            RegisterGameObject(
                new Box()
                {
					Position = progressBarContainerTopLeft,
					Dimensions = progressBarContainerDimensions,
                    EdgeTexture = TextureManager.Textures[Pixel_Texture],
                    EdgeTint = Color.CornflowerBlue,
                    BackgroundTexture = TextureManager.Textures[Pixel_Texture],
                    BackgroundTint = Color.White,
                    RenderLayer = 4,
                    Visible = true
                }); 

			_progressBarX = (int)progressBarContainerTopLeft.X + Progress_Container_Margin;
			_progressBarY = (int)progressBarContainerTopLeft.Y + Progress_Container_Margin;
			_progressBarWidth = progressBarContainerDimensions.X - (Progress_Container_Margin * 2);
        }

        protected override void LoadCustomContent(XElement asset)
        {
            switch (asset.Attribute("name").Value)
            {
                case "animationsequences":
                    XDocument animationSequenceData = FileManager.LoadXMLContentFile(asset.Attribute("file").Value);
                    foreach (XElement sequence in animationSequenceData.Element("animationsequences").Elements())
                    {
                        AnimationDataManager.AddSequence(sequence);
                    }
                    break;
                case "skeletons":
                    XDocument skeletonData = FileManager.LoadXMLContentFile(asset.Attribute("file").Value);
                    foreach (XElement skeleton in skeletonData.Element("skeletons").Elements())
                    {
                        SkeletonDataManager.AddSkeleton(skeleton);
                    }
                    break;
                case "skeleton-skins":
                    XDocument skinData = FileManager.LoadXMLContentFile(asset.Attribute("file").Value);
                    foreach (XElement skin in skinData.Element("skins").Elements())
                    {
                        SkeletonDataManager.AddSkin(skin);
                    }
                    break;
                case "emitterbehaviours":
                    XDocument emitterBehaviourData = FileManager.LoadXMLContentFile(asset.Attribute("file").Value);
                    foreach (XElement behaviour in emitterBehaviourData.Element("emitterbehaviours").Elements())
                    {
                        EmitterFactoryManager.AddEmitterFactory(behaviour);
                    }
                    break;
                case "translations":
                    Interface.Translator.Initialize();
                    break;
                case "avatarcomponents":
                    Data.Avatar.AvatarComponentManager.Initialize();
                    break;
                case "popupmappings":
                    Effects.Popups.MappingManager.Initialize();
                    break;
            }
        }
		
        private const string Asset_File_Name_And_Path = "Content/Files/Loadables.xml";
        private const string Logo_Texture = "leda-logo";
        private const string Pixel_Texture = "pixel";
		private const string Spinner_Texture = "load-spinner";
	
		private const int Progress_Gauge_Total_Width = 800;
		private const int Progress_Gauge_Center_Y = 725;
		private const int Progress_Bar_Height = 50;
		private const int Progress_Container_Margin = 5;
		private const float Spin_Degrees_Per_Millisecond = 0.1f;
    }
}
