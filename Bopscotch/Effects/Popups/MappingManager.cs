using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Leda.Core.Asset_Management;

namespace Bopscotch.Effects.Popups
{
    public sealed class MappingManager
    {
        public static Dictionary<string, Rectangle> Mappings { get; private set; }

        public static void Initialize()
        {
            Mappings = new Dictionary<string, Rectangle>();

            LoadMappingsFromXmlFile();
        }

        private static void LoadMappingsFromXmlFile()
        {
            XDocument componentFileData = FileManager.LoadXMLContentFile(Avatar_Components_FileName);

            foreach (XElement el in componentFileData.Element("mappings").Elements("popup"))
            {
                Mappings.Add(
                    el.Attribute("name").Value,
                    new Rectangle((int)el.Attribute("x"), (int)el.Attribute("y"), (int)el.Attribute("width"), (int)el.Attribute("height")));
            }
        }

        private const string Avatar_Components_FileName = "Content/Files/PopupMappings.xml";
    }
}
