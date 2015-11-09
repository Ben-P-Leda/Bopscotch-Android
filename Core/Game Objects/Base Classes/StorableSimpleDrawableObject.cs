using System.Xml.Linq;

using Microsoft.Xna.Framework;

using Leda.Core.Asset_Management;
using Leda.Core.Serialization;
using Leda.Core.Game_Objects.Behaviours;

namespace Leda.Core.Game_Objects.Base_Classes
{
    public class StorableSimpleDrawableObject : DisposableSimpleDrawableObject, ITextureManaged
    {
        public string ID { set; get; }
        public virtual string TextureReference { set; get; }

        public StorableSimpleDrawableObject(string id)
            : this()
        {
            ID = id;
            TextureManager.AddManagedObject(this);
        }

        public StorableSimpleDrawableObject()
            : base()
        {
            TextureReference = "";
        }

        public XElement Serialize()
        {
            return Serialize(new Serializer(this));
        }

        protected virtual XElement Serialize(Serializer serializer)
        {
            serializer.AddDataItem("texture", TextureReference);
            serializer.AddDataItem("frame", base.Frame);
            serializer.AddDataItem("origin", base.Origin);
            serializer.AddDataItem("worldposition", base.WorldPosition);
            serializer.AddDataItem("cameraposition", base.CameraPosition);
            serializer.AddDataItem("renderlayer", base.RenderLayer);
            serializer.AddDataItem("visible", base.Visible);
            serializer.AddDataItem("scale", base.Scale);
            serializer.AddDataItem("rotation", base.Rotation);
            serializer.AddDataItem("mirror", base.Mirror);
            serializer.AddDataItem("tint", base.Tint);
            serializer.AddDataItem("renderdepth", base.RenderDepth);

            return serializer.SerializedData;
        }

        public void Deserialize(XElement serializedData)
        {
            Deserialize(new Serializer(serializedData));
        }

        protected virtual Serializer Deserialize(Serializer serializer)
        {
            TextureReference = serializer.GetDataItem<string>("texture");
            base.Frame = serializer.GetDataItem<Rectangle>("frame");
            base.Origin = serializer.GetDataItem<Vector2>("origin");
            base.WorldPosition = serializer.GetDataItem<Vector2>("worldposition");
            base.CameraPosition = serializer.GetDataItem<Vector2>("cameraposition");
            base.RenderLayer = serializer.GetDataItem<int>("renderlayer");
            base.Visible = serializer.GetDataItem<bool>("visible");
            base.Scale = serializer.GetDataItem<float>("scale");
            base.Rotation = serializer.GetDataItem<float>("rotation");
            base.Mirror = serializer.GetDataItem<bool>("mirror");
            base.RenderDepth = serializer.GetDataItem<float>("renderdepth");
            base.Tint = serializer.GetDataItem<Color>("tint");

            TextureManager.AddManagedObject(this);

            return serializer;
        }
    }
}
