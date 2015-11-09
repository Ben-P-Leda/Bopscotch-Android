using System;
using System.Xml.Linq;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Leda.Core;
using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Gamestate_Management;
using Leda.Core.Asset_Management;

namespace Bopscotch.Gameplay.Objects.Environment.Signposts
{
    public sealed class SignpostFactory
    {
        private static SignpostFactory _factory = null;
        private static SignpostFactory Factory { get { if (_factory == null) { _factory = new SignpostFactory(); } return _factory; } }

        public static Scene.ObjectRegistrationHandler ObjectRegistrationHandler { set { Factory._registerComponent = value; } }

        public static void LoadSignposts(XElement signpostDataGroup)
        {
            Factory.Reset();

            if (signpostDataGroup != null)
            {
                foreach (XElement node in signpostDataGroup.Elements())
                {
                    if (node.Name == RouteMarker.Data_Node_Name) { Factory.CreateRouteMarkerFromXmlNode(node); }
                    else { Factory.CreateSignpostFromXmlNode(node); }
                }
            }
        }

        private int _nextSignpostID;
        private Scene.ObjectRegistrationHandler _registerComponent;

        private void Reset()
        {
            _nextSignpostID = 0;
        }

        private void CreateRouteMarkerFromXmlNode(XElement node)
        {
            RouteMarker newMarker = new RouteMarker();
            newMarker.ID = string.Concat(Route_Marker_Serialized_Data_Identifier, _nextSignpostID++);
            newMarker.SetTexture(node.Attribute("texture").Value);

            newMarker.WorldPosition = new Vector2((float)node.Attribute("x"), (float)node.Attribute("y")) + new Vector2(Definitions.Grid_Cell_Pixel_Size);
            newMarker.Rotation = MathHelper.PiOver2 * (int)node.Attribute("quadrant");

            _registerComponent(newMarker);
        }

        private void CreateSignpostFromXmlNode(XElement node)
        {
            SignpostBase newSignpost = null;

            switch (node.Name.ToString())
            {
                case OneWaySignpost.Data_Node_Name:
                    newSignpost = new OneWaySignpost();
                    ((OneWaySignpost)newSignpost).Mirror = (bool)node.Attribute("mirror");
                    break;
                case SpeedLimitSignpost.Data_Node_Name:
                    newSignpost = new SpeedLimitSignpost();
                    ((SpeedLimitSignpost)newSignpost).SpeedRange = new Range((int)node.Attribute("minimum-speed"), (int)node.Attribute("maximum-speed"));
                    break;
            }

            if (newSignpost != null) 
            {
                newSignpost.ID = string.Concat(Signpost_Serialized_Data_Identifier, _nextSignpostID++);
                newSignpost.TextureReference = node.Attribute("texture").Value;
                newSignpost.Texture = TextureManager.Textures[node.Attribute("texture").Value];
                newSignpost.SetCollisionBoundingBox((float)node.Attribute("zone-top"));

                newSignpost.WorldPosition = new Vector2(
                    (float)node.Attribute("x") + (Definitions.Grid_Cell_Pixel_Size / 2.0f),
                    (float)node.Attribute("y") + (Definitions.Grid_Cell_Pixel_Size / 2.0f) + SignpostBase.Plate_Vertical_Offset);

                _registerComponent(newSignpost);
            }
        }

        private void SetObjectWorldPosition(IWorldObject objectToPosition, XElement dataSource)
        {

        }

        public static void ReinstateSerializedSignposts(List<XElement> signpostData)
        {
            foreach (XElement el in signpostData)
            {
                SignpostBase newSignpost = (SignpostBase)Activator.CreateInstance(Type.GetType(el.Attribute("type").Value));
                newSignpost.ID = el.Attribute("id").Value;
                Factory._registerComponent(newSignpost);
            }
        }

        public static void ReinstateSerializedRouteMarkers(List<XElement> markerData)
        {
            foreach (XElement el in markerData)
            {
                RouteMarker newMarker = new RouteMarker();
                newMarker.ID = el.Attribute("id").Value;
                Factory._registerComponent(newMarker);
            }
        }


        public const string Data_Group_Node_Name = "signposts";
        public const string Signpost_Serialized_Data_Identifier = "signpost-";
        public const string Route_Marker_Serialized_Data_Identifier = "routemarker-";
    }
}
