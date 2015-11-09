using System;
using System.Xml.Linq;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Leda.Core.Gamestate_Management;
using Leda.Core.Asset_Management;

namespace Bopscotch.Gameplay.Objects.Environment.Flags
{
    public sealed class FlagFactory
    {
        private static FlagFactory _factory = null;
        private static FlagFactory Factory { get { if (_factory == null) { _factory = new FlagFactory(); } return _factory; } }

        public static Scene.ObjectRegistrationHandler ObjectRegistrationHandler { set { Factory._registerComponent = value; } }

        public static void LoadFlags(XElement flagDataGroup)
        {
            Factory.Reset();

            if (flagDataGroup != null)
            {
                foreach (XElement node in flagDataGroup.Elements())
                {
                    Flag toAdd = Factory.CreateFlagFromXmlNode(node);

                    toAdd.WorldPosition = new Vector2(
                        (float)node.Attribute("x") + ((Definitions.Grid_Cell_Pixel_Size - TextureManager.Textures[Flag.Pole_Texture].Width) / 2.0f),
                        (float)node.Attribute("y") + (Definitions.Grid_Cell_Pixel_Size / 2.0f));
                    if (node.Attribute("left-activation") != null) { toAdd.ActivatedWhenMovingLeft = (bool)node.Attribute("left-activation"); }
                    toAdd.SetUpPole();
                    toAdd.SetCollisionBoundingBox((float)node.Attribute("zone-top"));

                    Factory._registerComponent(toAdd);
                }
            }
        }

        private int _nextFlagID;
        private Scene.ObjectRegistrationHandler _registerComponent;

        private void Reset()
        {
            _nextFlagID = 0;
        }

        private Flag CreateFlagFromXmlNode(XElement node)
        {
            Flag newFlag = null;

            switch (node.Name.ToString())
            {
                case GoalFlag.Data_Node_Name:
                    newFlag = new GoalFlag();
                    break;
                case CheckpointFlag.Data_Node_Name:
                    newFlag = new CheckpointFlag((int)node.Attribute("index"));
                    break;

            }

            if (newFlag != null) { newFlag.ID = string.Concat(Serialized_Data_Identifier, _nextFlagID++); }

            return newFlag; ;
        }

        public static void ReinstateSerializedFlags(List<XElement> flagData)
        {
            foreach (XElement el in flagData)
            {
                Flag newFlag = (Flag)Activator.CreateInstance(Type.GetType(el.Attribute("type").Value));
                newFlag.ID = el.Attribute("id").Value;
                Factory._registerComponent(newFlag);
            }
        }

        public const string Data_Group_Node_Name = "flags";
        public const string Serialized_Data_Identifier = "flag-";
    }
}
