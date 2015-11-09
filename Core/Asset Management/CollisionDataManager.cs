using System;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Leda.Core;
using Leda.Core.Shapes;

namespace Leda.Core.Asset_Management
{
    public sealed class CollisionDataManager
    {
        private static XElement _collisionShapeData = null;

        public static XElement CollisionShapeData { set { _collisionShapeData = value; } }

        private static void AddCollisionShape(XElement toAdd)
        {
            if (_collisionShapeData == null) { _collisionShapeData = new XElement("collisionshapes"); }

            _collisionShapeData.Add(toAdd);
        }

        public static Rectangle BoxCollisionData(string objectName)
        {
            Rectangle collisionBox = Rectangle.Empty;
            XElement dataElement = GetCollisionDataElement(objectName, ColliderShape.BoxCollider);

            if (dataElement != null)
            {
                collisionBox.X = Convert.ToInt32(dataElement.Attribute("x").Value);
                collisionBox.Y = Convert.ToInt32(dataElement.Attribute("y").Value);
                collisionBox.Width = Convert.ToInt32(dataElement.Attribute("width").Value);
                collisionBox.Height = Convert.ToInt32(dataElement.Attribute("height").Value);
            }

            return collisionBox;
        }

        public static Polygon PolygonCollisionData(string objectName)
        {
            Polygon collisionShape = new Polygon();
            XElement dataElement = GetCollisionDataElement(objectName, ColliderShape.PolygonCollider);

            if ((dataElement != null) && (dataElement.Elements("vertex").Count() > 0))
            {
                foreach (XElement vertex in dataElement.Elements("vertex"))
                {
                    // TODO: Fix this
                    //collisionShape.Vertices.Add(new Vector2(Utility.ToFloat(vertex.Attribute("x").Value), Utility.ToFloat(vertex.Attribute("y").Value)));
                }
            }

            return collisionShape;
        }

        public static List<Polygon> CompoundPolygonCollisionData(string objectName)
        {
            List<Polygon> collisionShapes = new List<Polygon>();
            XElement dataElement = GetCollisionDataElement(objectName, ColliderShape.CompoundPolygonCollider);

            if ((dataElement != null) && (dataElement.Elements("polygon").Count() > 0))
            {
                foreach (XElement polygon in dataElement.Elements("polygon"))
                {
                    if (polygon.Elements("vertex").Count() > 0)
                    {
                        Polygon component = new Polygon();
                        foreach (XElement vertex in polygon.Elements("vertex"))
                        {  
                            // TODO: Fix this
                            //component.Vertices.Add(new Vector2(Utility.ToFloat(vertex.Attribute("x").Value), Utility.ToFloat(vertex.Attribute("y").Value)));
                        }
                        collisionShapes.Add(component);
                    }
                }
            }

            return collisionShapes;
        }

        private static XElement GetCollisionDataElement(string objectName, ColliderShape shapeType)
        {
            if (_collisionShapeData != null)
            {
                List<XElement> dataElements = (from el
                    in _collisionShapeData.Elements("collisionshape")
                    where (
                        ((string)el.Attribute("objectname") == objectName) &&
                        ((string)el.Attribute("type") == shapeType.ToString().ToLower()))
                    select el).ToList();

                if (dataElements.Count > 0) { return dataElements[0]; }
            }

            return null;
        }

        private enum ColliderShape
        {
            BoxCollider,
            PolygonCollider,
            CompoundPolygonCollider
        }
    }
}
