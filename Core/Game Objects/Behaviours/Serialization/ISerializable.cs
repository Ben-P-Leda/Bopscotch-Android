using System.Xml.Linq;

namespace Leda.Core.Game_Objects.Behaviours
{
    public interface ISerializable
    {
        string ID { get; set; }

        XElement Serialize();
        void Deserialize(XElement serializedData);
    }
}
