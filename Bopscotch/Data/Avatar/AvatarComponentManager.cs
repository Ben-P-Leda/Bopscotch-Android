using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Leda.Core.Asset_Management;

namespace Bopscotch.Data.Avatar
{
    public static class AvatarComponentManager
    {
        public static Dictionary<string, XElement> CostumeComponents { get; private set; }
        public static Dictionary<string, AvatarComponentSet> ComponentSets { get; private set; }

        public static void Initialize()
        {
            CostumeComponents = new Dictionary<string, XElement>();
            LoadCostumesFromXmlFile();

            ComponentSets = new Dictionary<string, AvatarComponentSet>();
            LoadComponentsFromXmlFile();
        }

        private static void LoadCostumesFromXmlFile()
        {
            XDocument costumeFileData = FileManager.LoadXMLContentFile(Avatar_Costumes_FileName);

            foreach (XElement el in costumeFileData.Element("costumes").Elements("costume")) { CostumeComponents.Add(el.Attribute("name").Value, el); }
        }

        private static void LoadComponentsFromXmlFile()
        {
            XDocument componentFileData = FileManager.LoadXMLContentFile(Avatar_Components_FileName);

            foreach (XElement el in componentFileData.Element("avatar-components").Elements("component-set")) { AddComponentSetFromXml(el); }
        }

        private static void AddComponentSetFromXml(XElement dataSource)
        {
            if (!ComponentSets.ContainsKey(dataSource.Attribute("name").Value))
            {
                ComponentSets.Add(
                    dataSource.Attribute("name").Value,
                    new AvatarComponentSet(
                        dataSource.Attribute("name").Value,
                        dataSource.Attribute("display-skeleton").Value,
                        (dataSource.Attribute("allow-none") == null) ? false : (bool)dataSource.Attribute("allow-none")));
            }

            foreach (XElement el in dataSource.Elements("component"))
            {
                AvatarComponent component = new AvatarComponent(el.Attribute("name").Value, el.Attribute("texture-name").Value);
                component.Unlocked = (((bool)el.Attribute("unlocked")) ||
                    (Data.Profile.AvatarComponentUnlocked(dataSource.Attribute("name").Value, el.Attribute("name").Value)));
                if (el.Attribute("tint-by-parent") != null) { component.TintedByParentSkin = (bool)el.Attribute("tint-by-parent"); }

                component.AddMappings(el.Element("mappings").Elements("mapping").ToList());
                if (el.Element("tint-overrides") != null) { component.AddTintOverrides(el.Element("tint-overrides").Elements("tint-override").ToList()); }

                ComponentSets[dataSource.Attribute("name").Value].Components.Add(component);
            }
        }

        public static void UnlockComponent(string setName, string componentName)
        {
            if (ComponentSets.ContainsKey(setName)) { ComponentSets[setName].UnlockComponent(componentName); }
        }

        public static AvatarComponent Component(string setName, string componentName)
        {
            return (ComponentSets.ContainsKey(setName) ? ComponentSets[setName].Component(componentName) : null);
        }

        public static string DisplaySkeletonForSet(string setName)
        {
            return (ComponentSets.ContainsKey(setName) ? ComponentSets[setName].DisplaySkeleton : "");
        }

        public static XElement FrontFacingAvatarSkin(int slotIndex)
        {
            string defaultSkinName = string.Format(Avatar_Skin_Front_Base, Default_Skin_Player_Names.Split(',')[slotIndex]);

            return CustomiseSkin(slotIndex, Definitions.Avatar_Skeleton_Front, defaultSkinName);
        }

        public static XElement SideFacingAvatarSkin(int slotIndex)
        {
            string defaultSkinName = string.Format(Avatar_Skin_Side_Base, Default_Skin_Player_Names.Split(',')[slotIndex]);

            return CustomiseSkin(slotIndex, Definitions.Avatar_Skeleton_Side, defaultSkinName);
        }

        private static XElement CustomiseSkin(int slotIndex, string skeletonName, string baseSkinName)
        {
            XElement defaultSkinData = SkeletonDataManager.Skins[baseSkinName];
            XElement skinData = new XElement("skin");
            foreach (XElement el in defaultSkinData.Elements()) { skinData.Add(el); }

            Dictionary<string, Color> tintOverrides = new Dictionary<string, Color>();
            List<XElement> bonesNotAllowingTintOverride = new List<XElement>();

            foreach (KeyValuePair<string, AvatarComponentSet> kvp in ComponentSets)
            {
                string componentName = Profile.Settings.GetAvatarCustomComponent(slotIndex, kvp.Key);
                if (!string.IsNullOrEmpty(componentName))
                {
                    AvatarComponent component = (from item in kvp.Value.Components where item.Name == componentName select item).FirstOrDefault();
                    if (component != null)
                    {
                        foreach (AvatarComponentMapping mapping in component.Mappings)
                        {
                            if (mapping.TargetSkeleton == skeletonName)
                            {
                                XElement bone = (from item in skinData.Elements() where item.Attribute("id").Value == mapping.TargetBone select item).FirstOrDefault();

                                bone.SetAttributeValue("texture-name", component.TextureName);
                                bone.SetAttributeValue("frame-x", mapping.Frame.X);
                                bone.SetAttributeValue("frame-y", mapping.Frame.Y);
                                bone.SetAttributeValue("frame-width", mapping.Frame.Width);
                                bone.SetAttributeValue("frame-height", mapping.Frame.Height);
                                bone.SetAttributeValue("origin-x", mapping.Origin.X);
                                bone.SetAttributeValue("origin-y", mapping.Origin.Y);

                                if (component.TintOverrides.Count > 0)
                                {
                                    foreach (AvatarComponentChildTintOverride tint in component.TintOverrides)
                                    {
                                        if (!tintOverrides.ContainsKey(tint.TargetBone)) { tintOverrides.Add(tint.TargetBone, Color.White); }
                                        tintOverrides[tint.TargetBone] = tint.OverridingTint;
                                    }
                                }

                                if ((!component.TintedByParentSkin) && (bonesNotAllowingTintOverride.Contains(bone))) { bonesNotAllowingTintOverride.Add(bone); }
                            }
                        }
                    }
                }
            }

            foreach (XElement bone in skinData.Elements())
            {
                if ((tintOverrides.ContainsKey(bone.Attribute("id").Value)) && (!bonesNotAllowingTintOverride.Contains(bone)))
                {
                    bone.Descendants("tint").Remove();
                    XElement tint = new XElement("tint");
                    tint.SetAttributeValue("red", (int)tintOverrides[bone.Attribute("id").Value].R);
                    tint.SetAttributeValue("green", (int)tintOverrides[bone.Attribute("id").Value].G);
                    tint.SetAttributeValue("blue", (int)tintOverrides[bone.Attribute("id").Value].B);
                    tint.SetAttributeValue("alpha", (int)tintOverrides[bone.Attribute("id").Value].A);
                    bone.Add(tint);
                }
                else if (bonesNotAllowingTintOverride.Contains(bone))
                {
                    bone.Descendants("tint").Remove();
                }
            }

            return skinData;
        }

        private const string Avatar_Components_FileName = "Content/Files/Avatars/Components.xml";
        private const string Avatar_Costumes_FileName = "Content/Files/Avatars/Costumes.xml";

        private const string Avatar_Skin_Front_Base = "player-{0}-dialog";
        private const string Avatar_Skin_Side_Base = "player-{0}-ingame";
        private const string Default_Skin_Player_Names = "dusty,dustin,dustina,dasuto";

        private const string Default_Avatar_Skin_Front = "player-dusty-dialog";
        private const string Default_Avatar_Skin_Side = "player-dusty-ingame";
    }
}
