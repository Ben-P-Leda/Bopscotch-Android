using System.Xml.Linq;

using Microsoft.Xna.Framework;

namespace Leda.Core.Animation
{
    public class SpriteSheetKeyframe : KeyframeBase
    {
        public string TextureName { get; set; }
        public Rectangle SourceArea { get; set; }

        public override XElement Serialize()
        {
            XElement frameData = base.Serialize();
            frameData.Add(new XAttribute("texture-name", TextureName));
            frameData.Add(new XAttribute("source-area-x", SourceArea.X));
            frameData.Add(new XAttribute("source-area-y", SourceArea.Y));
            frameData.Add(new XAttribute("source-area-width", SourceArea.Width));
            frameData.Add(new XAttribute("source-area-height", SourceArea.Height));

            return frameData;
        }

        public override void Deserialize(XElement serializedData)
        {
            base.Deserialize(serializedData);

            TextureName = serializedData.Attribute("texture-name").Value;

            Rectangle sourceArea = new Rectangle(
                (int)serializedData.Attribute("source-area-x"), (int)serializedData.Attribute("source-area-y"),
                (int)serializedData.Attribute("source-area-width"), (int)serializedData.Attribute("source-area-height"));

            SourceArea = sourceArea;
        }
    }
}
