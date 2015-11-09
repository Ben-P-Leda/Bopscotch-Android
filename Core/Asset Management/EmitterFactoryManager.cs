using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

using Leda.Core.Effects.Particles;

namespace Leda.Core.Asset_Management
{
    public sealed class EmitterFactoryManager
    {
        private static Dictionary<string, EmitterFactory> _emitterFactoryLibrary = null;

        public static Dictionary<string, EmitterFactory> EmitterFactories
        {
            get { if (_emitterFactoryLibrary == null) { _emitterFactoryLibrary = new Dictionary<string, EmitterFactory>(); } return _emitterFactoryLibrary; }
        }

        public static void AddEmitterFactory(string name, EmitterFactory emitterFactory)
        {
            if (!EmitterFactories.ContainsKey(name)) { EmitterFactories.Add(name, emitterFactory); }
            else { EmitterFactories[name] = emitterFactory; }
        }

        public static void AddEmitterFactory(XElement xmlData)
        {
            AddEmitterFactory(xmlData.Attribute("name").Value, new EmitterFactory(xmlData));
        }
    }
}
