using System;
using System.Xml.Linq;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Leda.Core.Gamestate_Management;
using Leda.Core.Asset_Management;

namespace Bopscotch.Gameplay.Objects.Environment.Collectables
{
    public sealed class CollectableFactory
    {
        private static CollectableFactory _factory = null;
        private static CollectableFactory Factory { get { if (_factory == null) { _factory = new CollectableFactory(); } return _factory; } }

        public static Scene.ObjectRegistrationHandler ObjectRegistrationHandler { set { Factory._registerComponent = value; } }

        public static void LoadCollectables(XElement collectableDataGroup)
        {
            Factory.Reset();

            if (collectableDataGroup != null)
            {
                foreach (XElement node in collectableDataGroup.Elements())
                {
                    Vector2 worldPosition = new Vector2((float)node.Attribute("x"), (float)node.Attribute("y")) +
                            (new Vector2(Definitions.Grid_Cell_Pixel_Size) / 2.0f);

                    Collectable toAdd = Factory.CreateCollectableFromXmlNode(node, worldPosition);
                    if (toAdd != null)
                    {
                        toAdd.WorldPosition = worldPosition;
                        Factory._registerComponent(toAdd);
                    }
                }
            }
        }

        private int _nextCollectableID;
        private Scene.ObjectRegistrationHandler _registerComponent;

        private void Reset()
        {
            _nextCollectableID = 0;
        }

        private Collectable CreateCollectableFromXmlNode(XElement node, Vector2 worldPosition)
        {
            Collectable newCollectable = null;

            switch (node.Name.ToString())
            {
                case GoldenTicket.Data_Node_Name:
                    if (!Data.Profile.CurrentAreaData.GoldenTicketHasBeenCollectedFromLevel(worldPosition)) { newCollectable = new GoldenTicket(); }
                    break;
                case ScoringCandy.Data_Node_Name:
                    newCollectable = new ScoringCandy();
                    ((ScoringCandy)newCollectable).ScoreValue = (int)node.Attribute("score");
                    break;
            }

            if (newCollectable != null) 
            {
                newCollectable.ID = string.Concat(Serialized_Data_Identifier, _nextCollectableID++);
                newCollectable.SetTextureAndRelatedValues(node.Attribute("texture").Value);
            }

            return newCollectable;
        }

        public static void ReinstateSerializedCollectables(List<XElement> collectableData)
        {
            foreach (XElement el in collectableData)
            {
                Collectable newCollectable = (Collectable)Activator.CreateInstance(Type.GetType(el.Attribute("type").Value));
                newCollectable.ID = el.Attribute("id").Value;
                Factory._registerComponent(newCollectable);
            }
        }

        public const string Data_Group_Node_Name = "collectables";
        public const string Serialized_Data_Identifier = "collectable-";
    }
}