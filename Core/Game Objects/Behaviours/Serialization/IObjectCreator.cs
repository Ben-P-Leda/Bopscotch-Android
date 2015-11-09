using System.Xml.Linq;

namespace Leda.Core.Game_Objects.Behaviours
{
    public interface IObjectCreator
    {
        void ReinstateDynamicObjects(XElement serializedData);
    }
}
