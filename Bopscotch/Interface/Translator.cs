using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Threading;

using Microsoft.Xna.Framework.Graphics;

using Leda.Core;
using Leda.Core.Asset_Management;

namespace Bopscotch.Interface
{
    public sealed class Translator
    {
        private static Translator _instance = null;
        private static Translator Instance { get { if (_instance == null) { _instance = new Translator(); } return _instance; } }

        public static string CultureCode { get { return Instance._selectedCulture; } }

        public static Translator Initialize() { return Instance; }
        public static string Translation(string key) { return Instance.GetTranslation(key); }

        private string _selectedCulture;
        private Dictionary<string, string> _translations;

        private Translator()
        {
            _translations = new Dictionary<string, string>();
            XDocument localisationXML = null;
            string fontFile = Default_Font_File;
            float fontSize = Default_Font_Size;
            float fontPadding = Default_Font_Padding;

            try
            {
                _selectedCulture = Thread.CurrentThread.CurrentCulture.Name;
                localisationXML = FileManager.LoadXMLContentFile(string.Format(Translations_File_Template, _selectedCulture));
            }
            catch
            {
                _selectedCulture = Default_Culture_Code;
                localisationXML = FileManager.LoadXMLContentFile(string.Format(Translations_File_Template, _selectedCulture));
            }

            if (localisationXML != null)
            {
                fontFile = localisationXML.Element("localisation").Element("font").Attribute("name").Value;
                fontSize = (float)localisationXML.Element("localisation").Element("font").Attribute("scale");
                fontPadding = (float)localisationXML.Element("localisation").Element("font").Attribute("padding");

                foreach (XElement el in localisationXML.Element("localisation").Element("translations").Elements("translation"))
                {
                    if (!_translations.ContainsKey(el.Attribute("key").Value)) { _translations.Add(el.Attribute("key").Value, ""); }
                    _translations[el.Attribute("key").Value] = el.Value.Trim();
                }
            }

            TextWriter.FontFile = string.Format(Font_File_Template, fontFile);
            TextWriter.SetDefaults(Game1.Instance.Content.Load<SpriteFont>(string.Format(Font_File_Template, fontFile)), fontSize, fontPadding);
        }

        private string GetTranslation(string key)
        {
            if (_translations.ContainsKey(key)) { return _translations[key]; }

            return key;
        }

        private const string Translations_File_Template = "Content/Files/Translations/{0}.xml";
        private const string Font_File_Template = "Fonts\\{0}";
        private const string Default_Culture_Code = "en-GB";
        private const string Default_Font_File = "komika-title-axis";
        private const float Default_Font_Size = 1.0f;
        private const float Default_Font_Padding = 0.0f;
    }
}
