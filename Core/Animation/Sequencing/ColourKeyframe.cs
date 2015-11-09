using System;
using System.Xml.Linq;

using Microsoft.Xna.Framework;

namespace Leda.Core.Animation
{
    public class ColourKeyframe : KeyframeBase
    {
        public Color TargetTint { get; set; }

        public override XElement Serialize()
        {
            XElement frameData = base.Serialize();
            frameData.Add(new XAttribute("tint-r", TargetTint.R));
            frameData.Add(new XAttribute("tint-g", TargetTint.G));
            frameData.Add(new XAttribute("tint-b", TargetTint.B));
            frameData.Add(new XAttribute("tint-a", TargetTint.A));

            return frameData;
        }

        public override void Deserialize(XElement serializedData)
        {
            base.Deserialize(serializedData);

            TargetTint = new Color(
                Convert.ToByte(serializedData.Attribute("tint-r").Value),
                Convert.ToByte(serializedData.Attribute("tint-g").Value),
                Convert.ToByte(serializedData.Attribute("tint-b").Value),
                Convert.ToByte(serializedData.Attribute("tint-a").Value));
        }
    }
}
