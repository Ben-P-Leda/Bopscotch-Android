using System.Xml.Linq;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

namespace Leda.Core.Animation
{
    public class SkeletalKeyframe : KeyframeBase
    {
        public Dictionary<string, DataContainer> BoneAnimationData { get; private set; }

        public SkeletalKeyframe()
        {
            BoneAnimationData = new Dictionary<string, DataContainer>();
        }

        public override XElement Serialize()
        {
            XElement frameData = base.Serialize();

            XElement boneData = new XElement("bones");
            foreach (KeyValuePair<string, DataContainer> bone in BoneAnimationData) { boneData.Add(bone.Value.Serialize(bone.Key)); }
            frameData.Add(boneData);

            return frameData;
        }

        public override void Deserialize(XElement serializedData)
        {
            base.Deserialize(serializedData);

            foreach (XElement bone in serializedData.Element("bones").Elements())
            {
                DataContainer boneData = new DataContainer();
                boneData.Deserialize(bone);
                BoneAnimationData.Add(bone.Attribute("id").Value, boneData);
            }
        }

        public class DataContainer
        {
            public string TextureName { get; set; }
            public Vector2 Origin { get; set; }
            public Rectangle SourceArea { get; set; }
            public Vector2 Offset { get; set; }
            public float Rotation { get; set; }
            public float Scale { get; set; }

            public DataContainer()
            {
                TextureName = "";
                Origin = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
                SourceArea = Rectangle.Empty;
                Offset = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
                Rotation = -1.0f;
                Scale = -1.0f;
            }

            public XElement Serialize(string boneName)
            {
                XElement boneData = new XElement("bone");
                boneData.Add(new XAttribute("id", boneName));

                if (!string.IsNullOrEmpty(TextureName)) { boneData.Add(new XAttribute("texture-name", TextureName)); }

                if (!float.IsNegativeInfinity(Offset.X))
                {
                    boneData.Add(new XAttribute("origin-x", Origin.X));
                    boneData.Add(new XAttribute("origin-y", Origin.Y));
                }

                if (SourceArea != Rectangle.Empty)
                {
                    boneData.Add(new XAttribute("source-area-x", SourceArea.X));
                    boneData.Add(new XAttribute("source-area-y", SourceArea.Y));
                    boneData.Add(new XAttribute("source-area-width", SourceArea.Width));
                    boneData.Add(new XAttribute("source-area-height", SourceArea.Height));
                }

                if (!float.IsNegativeInfinity(Offset.X))
                {
                    boneData.Add(new XAttribute("offset-x", Offset.X));
                    boneData.Add(new XAttribute("offset-y", Offset.Y));
                }

                if (Rotation >= 0.0f) { boneData.Add(new XAttribute("rotation", Rotation)); }
                if (Scale >= 0.0f) { boneData.Add(new XAttribute("scale", Scale)); }

                return boneData;
            }

            public void Deserialize(XElement serializedData)
            {
                if (serializedData.Attribute("texture-name") != null) { TextureName = serializedData.Attribute("texture-name").Value; }

                if (serializedData.Attribute("origin-x") != null)
                {
                    Origin = new Vector2((float)serializedData.Attribute("origin-x"), (float)serializedData.Attribute("origin-y"));
                }

                if (serializedData.Attribute("source-area-x") != null)
                {
                    Rectangle sourceArea = new Rectangle(
                        (int)serializedData.Attribute("source-area-x"), (int)serializedData.Attribute("source-area-y"),
                        (int)serializedData.Attribute("source-area-width"), (int)serializedData.Attribute("source-area-height"));

                    SourceArea = sourceArea;
                }

                if (serializedData.Attribute("offset-x") != null)
                {
                    Offset = new Vector2((float)serializedData.Attribute("offset-x"), (float)serializedData.Attribute("offset-y"));
                }

                if (serializedData.Attribute("rotation") != null) { Rotation = (float)serializedData.Attribute("rotation"); }
                if (serializedData.Attribute("scale") != null) { Scale = (float)serializedData.Attribute("scale"); }
            }
        }
    }
}
