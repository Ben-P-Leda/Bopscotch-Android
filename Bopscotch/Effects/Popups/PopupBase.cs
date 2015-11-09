using System.Xml.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core;
using Leda.Core.Game_Objects.Base_Classes;
using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Asset_Management;
using Leda.Core.Animation;
using Leda.Core.Serialization;

namespace Bopscotch.Effects.Popups
{
    public abstract class PopupBase : ISimpleRenderable, IAnimated, ITransformationAnimatable, IColourAnimatable, ISerializable, ITextureManaged
    {
        private bool _hideWhenAnimationComplete;
        private string _mappingName;

        public string ID { get; set; }

        public IAnimationEngine AnimationEngine { get; protected set; }

        public string TextureReference { get { return Popup_Texture_Name; } }
        public string MappingName { private get { return _mappingName; } set { InitializeForNamedMapping(value); } }
        public Texture2D Texture { get; set; }
        public Rectangle Frame { get; set; }
        public Vector2 Origin { private get; set; }
        public Vector2 DisplayPosition { private get; set; }
        public int RenderLayer { get { return Render_Layer; } set { } }
        public float RenderDepth { private get; set; }
        public bool Visible { get; set; }
        public Color Tint { get; set; }
        public float Scale { get; set; }
        public float Rotation { get; set; }

        public PopupBase()
            : base()
        {
            DisplayPosition = new Vector2(Definitions.Back_Buffer_Width / 2.0f, Definitions.Back_Buffer_Height / 4.0f);
            Visible = false;

            _hideWhenAnimationComplete = true;

            Tint = Color.White;
            Scale = 0.0f;
            Rotation = 0.0f;
            RenderDepth = Default_Render_Depth;
        }

        public void Initialize()
        {
        }

        public virtual void Reset()
        {
            Tint = Color.White;
            Scale = 0.0f;
            Visible = false;
        }

        private void InitializeForNamedMapping(string mappingName)
        {
            if (!string.IsNullOrEmpty(mappingName))
            {
                _mappingName = mappingName;

                Texture = TextureManager.Textures["popups"];
                Frame = MappingManager.Mappings[mappingName];
                Origin = new Vector2(Frame.Width, Frame.Height) / 2.0f;
            }
        }

        public void RunPopupSequence(string sequenceName, bool hideWhenAnimationComplete)
        {
            Visible = true;

            _hideWhenAnimationComplete = hideWhenAnimationComplete;
            switch (AnimationDataManager.Sequences[sequenceName].SequenceType)
            {
                case AnimationSequence.AnimationSequenceType.Transformation:
                    if (!(AnimationEngine is TransformationAnimationEngine)) { AnimationEngine = new TransformationAnimationEngine(this); }
                    break;
                case AnimationSequence.AnimationSequenceType.Colour:
                    if (!(AnimationEngine is ColourAnimationEngine)) { AnimationEngine = new ColourAnimationEngine(this); }
                    break;
            }

            WireUpAnimationEngineHooks();
            AnimationEngine.Sequence = AnimationDataManager.Sequences[sequenceName];
        }

        private void WireUpAnimationEngineHooks()
        {
            if (AnimationEngine != null)
            {
                AnimationEngine.SequenceCompletionHandler = HandleAnimationSequenceCompletion;
                AnimationEngine.ID = "popup-animation-engine";
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, GameBase.ScreenPosition(DisplayPosition), Frame, Tint, Rotation, Origin, GameBase.ScreenScale(Scale * 2.0f), 
                SpriteEffects.None, RenderDepth);
        }

        protected virtual void HandleAnimationSequenceCompletion()
        {
            if (_hideWhenAnimationComplete) { Visible = false; }
        }

        public XElement Serialize()
        {
            return Serialize(new Serializer(this));
        }

        protected virtual XElement Serialize(Serializer serializer)
        {
            serializer.AddDataItem("mapping", _mappingName);
            serializer.AddDataItem("frame", Frame);
            serializer.AddDataItem("origin", Origin);
            serializer.AddDataItem("display-position", DisplayPosition);
            serializer.AddDataItem("visible", Visible);
            serializer.AddDataItem("scale", Scale);
            serializer.AddDataItem("rotation", Rotation);
            serializer.AddDataItem("tint", Tint);

            if (AnimationEngine is ColourAnimationEngine)
            {
                serializer.AddDataItem("engine-type", "colour");
                serializer.AddDataItem("animation-engine", AnimationEngine);
            }
            else if (AnimationEngine is TransformationAnimationEngine)
            {
                serializer.AddDataItem("engine-type", "transform");
                serializer.AddDataItem("animation-engine", AnimationEngine);
            }
            else
            {
                serializer.AddDataItem("engine-type", "none");
            }

            return serializer.SerializedData;
        }

        public void Deserialize(XElement serializedData)
        {
            Deserialize(new Serializer(serializedData));
        }

        protected virtual Serializer Deserialize(Serializer serializer)
        {
            MappingName = serializer.GetDataItem<string>("texture");
            Frame = serializer.GetDataItem<Rectangle>("frame");
            Origin = serializer.GetDataItem<Vector2>("origin");
            DisplayPosition = serializer.GetDataItem<Vector2>("display-position");
            Visible = serializer.GetDataItem<bool>("visible");
            Scale = serializer.GetDataItem<float>("scale");
            Rotation = serializer.GetDataItem<float>("rotation");
            Tint = serializer.GetDataItem<Color>("tint");

            AnimationEngine = null;
            switch (serializer.GetDataItem<string>("engine-type"))
            {
                case "colour":
                    AnimationEngine = new ColourAnimationEngine(this);
                    WireUpAnimationEngineHooks();
                    serializer.KnownSerializedObjects.Add(AnimationEngine);
                    AnimationEngine = serializer.GetDataItem<ColourAnimationEngine>("animation-engine");
                    break;
                case "transform":
                    AnimationEngine = new TransformationAnimationEngine(this);
                    WireUpAnimationEngineHooks();
                    serializer.KnownSerializedObjects.Add(AnimationEngine);
                    AnimationEngine = serializer.GetDataItem<TransformationAnimationEngine>("animation-engine");
                    break;
            }

            TextureManager.AddManagedObject(this);

            return serializer;
        }

        private const int Render_Layer = 4;
        private const float Default_Render_Depth = 0.7f;

        private const string Popup_Texture_Name = "popups";
    }
}
