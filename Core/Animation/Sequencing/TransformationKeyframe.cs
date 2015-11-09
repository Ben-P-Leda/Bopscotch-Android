using System.Xml.Linq;

using Microsoft.Xna.Framework;

namespace Leda.Core.Animation
{
    public class TransformationKeyframe : KeyframeBase
    {
        public float TargetRotation { get; set; }
        public float TargetScale { get; set; }

        public override XElement Serialize()
        {
            XElement frameData = base.Serialize();
            frameData.Add(new XAttribute("rotation", TargetRotation));
            frameData.Add(new XAttribute("scale", TargetScale));

            return frameData;
        }

        public override void Deserialize(XElement serializedData)
        {
            base.Deserialize(serializedData);

            TargetRotation = (float)serializedData.Attribute("rotation");
            TargetScale = (float)serializedData.Attribute("scale");
        }
    }
}
