using System.Xml.Linq;

namespace Leda.Core.Animation
{
    public abstract class KeyframeBase : IKeyframe
    {
        public int DurationInMilliseconds { get; set; }

        public virtual XElement Serialize()
        {
            XElement frameData = new XElement("keyframe");
            frameData.Add(new XAttribute("duration", DurationInMilliseconds));

            return frameData;
        }

        public virtual void Deserialize(XElement serializedData)
        {
            DurationInMilliseconds = (int)serializedData.Attribute("duration");
        }
    }
}
