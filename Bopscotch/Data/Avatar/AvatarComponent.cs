using System.Xml.Linq;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

namespace Bopscotch.Data.Avatar
{
    public class AvatarComponent
    {
        public string Name { get; private set; }
        public string TextureName { get; private set; }
        public bool Unlocked { get; set; }
        public bool TintedByParentSkin { get; set; }

        public List<AvatarComponentMapping> Mappings { get; private set; }
        public List<AvatarComponentChildTintOverride> TintOverrides { get; private set; }

        public AvatarComponent(string name, string textureName)
        {
            Name = name;
            TextureName = textureName;
            Unlocked = false;
            TintedByParentSkin = false;

            Mappings = new List<AvatarComponentMapping>();
            TintOverrides = new List<AvatarComponentChildTintOverride>();
        }

        public void AddMappings(List<XElement> mappings)
        {
            Mappings.Clear();

            foreach (XElement el in mappings)
            {
                Mappings.Add(
                    new AvatarComponentMapping(
                        el.Attribute("skeleton").Value,
                        el.Attribute("bone").Value,
                        (float)el.Attribute("origin-x"),
                        (float)el.Attribute("origin-y"),
                        (int)el.Attribute("frame-x"),
                        (int)el.Attribute("frame-y"),
                        (int)el.Attribute("frame-width"),
                        (int)el.Attribute("frame-height")));
            }
        }

        public void AddTintOverrides(List<XElement> overrides)
        {
            TintOverrides.Clear();

            foreach (XElement el in overrides)
            {
                TintOverrides.Add(
                    new AvatarComponentChildTintOverride(
                        el.Attribute("bone").Value,
                        (int)el.Attribute("red"),
                        (int)el.Attribute("green"),
                        (int)el.Attribute("blue"),
                        (int)el.Attribute("alpha")));
            }
        }
    }
}
