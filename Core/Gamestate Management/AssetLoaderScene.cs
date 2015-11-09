using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Threading;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

using Leda.Core.Asset_Management;

namespace Leda.Core.Gamestate_Management
{
    public abstract class AssetLoaderScene : Scene
    {
        public delegate void AssetLoadCompletionHandler(Type loaderSceneType);

        private XDocument _assetsForThreadedLoad;
        private Thread _loader = null;

        private string _assetListFileToLoad;
        private int _totalAssetsToLoad;
        private int _loadedAssetCount;

        public string AssetListFileName { set { _assetListFileToLoad = value; } }
        public AssetLoadCompletionHandler LoadCompletionHandler { set; private get; }
        public Type PostLoadTargetSceneType { set { NextSceneType = value; } }
        public float AssetLoadProgress { get { return _totalAssetsToLoad > 0 ? (float)_loadedAssetCount / (float)_totalAssetsToLoad : 0.0f; } }

        private bool ReadyToLoad { get { return ((AssetLoadProgress == 0.0f) && (_loader == null)); } }
        private bool LoadCompleted { get { return ((AssetLoadProgress == 1.0f) && (_loader != null)); } }

        public AssetLoaderScene()
            : base()
        {
            _assetListFileToLoad = "";
            _totalAssetsToLoad = 0;
            _loadedAssetCount = 0;

            LoadCompletionHandler = null;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (CurrentState == Status.Active)
            {
                if (string.IsNullOrEmpty(_assetListFileToLoad)) { Deactivate(); }
                else if (ReadyToLoad) { StartThreadedAssetLoad(); }
                else if (LoadCompleted) { CompleteThreadedAssetLoad(); }
            }
        }

        public override void Activate()
        {
            _totalAssetsToLoad = 0;
            _loader = null;

            base.Activate();
        }

        protected override void CompleteDeactivation()
        {
            if (LoadCompletionHandler != null) { LoadCompletionHandler(this.GetType()); }

            base.CompleteDeactivation();
        }

        private void StartThreadedAssetLoad()
        {
            _assetsForThreadedLoad = FileManager.LoadXMLContentFile(_assetListFileToLoad);

            CalculateTotalAssetCount();

            ThreadStart threadStarter = new ThreadStart(ThreadedAssetLoader);
            _loader = new Thread(threadStarter);
            _loader.Start();
        }

        private void CalculateTotalAssetCount()
        {
            _totalAssetsToLoad = 0;
            foreach (XElement at in _assetsForThreadedLoad.Element("assets").Elements()) { _totalAssetsToLoad += at.Elements().Count(); }
        }

        private void ThreadedAssetLoader()
        {
            // Spin through the attribute list and load based on type
            foreach (XElement at in _assetsForThreadedLoad.Element("assets").Elements())
            {
                // Expected XML structure is "assets" - "[assettype]s" - "[asset]"
                foreach (XElement asset in at.Elements())
                {
                    switch (at.Name.ToString())
                    {
                        case "textures":
                            TextureManager.AddTexture(asset.Attribute("name").Value, Game.Content.Load<Texture2D>(asset.Attribute("file").Value));
                            break;
                        case "music":
                            MusicManager.AddTune(asset.Attribute("name").Value, Game.Content.Load<Song>(asset.Attribute("file").Value));
                            break;
                        case "sound-effects":
                            if (asset.Attribute("use-instance") != null)
                            {
                                SoundEffectManager.AddEffect(
                                    asset.Attribute("name").Value,
                                    Game.Content.Load<SoundEffect>(asset.Attribute("file").Value),
                                    (bool)asset.Attribute("use-instance"));
                            }
                            else
                            {
                                SoundEffectManager.AddEffect(
                                    asset.Attribute("name").Value,
                                    Game.Content.Load<SoundEffect>(asset.Attribute("file").Value),
                                    false);
                            }
                            break;
                        case "custom":
                            LoadCustomContent(asset);
                            break;
                    }

                    _loadedAssetCount++;
                }
            }
        }

        protected virtual void LoadCustomContent(XElement asset)
        {
        }

        private void CompleteThreadedAssetLoad()
        {
            if (_loader != null)
            {
                _loader.Join();
                _loader = null;
            }

            Deactivate();
        }
    }
}
