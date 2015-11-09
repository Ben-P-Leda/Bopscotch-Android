using System.Xml.Linq;

namespace Leda.Core.Animation
{
    public interface IKeyframe
    {
        int DurationInMilliseconds { get; set; }

        XElement Serialize();
        void Deserialize(XElement serializedData);
    }
}
