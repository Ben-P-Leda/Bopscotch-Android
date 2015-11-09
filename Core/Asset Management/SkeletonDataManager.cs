using System.Xml.Linq;
using System.Collections.Generic;

namespace Leda.Core.Asset_Management
{
    public sealed class SkeletonDataManager
    {
        private static Dictionary<string, XElement> _skeletons = null;

        public static Dictionary<string, XElement> Skeletons
        {
            get
            {
                if (_skeletons == null) { _skeletons = new Dictionary<string, XElement>(); }
                return _skeletons;
            }
        }

        public static void AddSkeleton(XElement xmlSkeletonData)
        {
            if (!Skeletons.ContainsKey(xmlSkeletonData.Attribute("name").Value))
            {
                Skeletons.Add(xmlSkeletonData.Attribute("name").Value, xmlSkeletonData);
            }
        }

        private static Dictionary<string, XElement> _skins = null;

        public static Dictionary<string, XElement> Skins
        {
            get
            {
                if (_skins == null) { _skins = new Dictionary<string, XElement>(); }
                return _skins;
            }
        }

        public static void AddSkin(XElement xmlSkinData)
        {
            if (!Skins.ContainsKey(xmlSkinData.Attribute("name").Value))
            {
                Skins.Add(xmlSkinData.Attribute("name").Value, xmlSkinData);
            }
        }
    }
}
