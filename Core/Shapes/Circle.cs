using System.Collections.Generic;

using Microsoft.Xna.Framework;

namespace Leda.Core.Shapes
{
    public class Circle
    {
        public Vector2 Center;
        public float Radius;

        public static Circle Empty { get { return new Circle(Vector2.Zero, 0.0f); } }

        public Circle(Vector2 center, float radius)
        {
            Center = center;
            Radius = radius;
        }

        public bool Intersects(Circle target)
        {
            return (Vector2.DistanceSquared(Center, target.Center) < (Radius * Radius) + (target.Radius * target.Radius));
        }

        public bool Intersects(Rectangle target)
        {
            if ((target.X > Center.X + Radius) || (target.X + target.Width < Center.X - Radius)) { return false; }
            if ((target.Y > Center.Y + Radius) || (target.Y + target.Height < Center.Y - Radius)) { return false; }

            Vector2 NearestPoint = new Vector2(
                MathHelper.Clamp(Center.X, target.X, target.X + target.Width),
                MathHelper.Clamp(Center.Y, target.Y, target.Y + target.Height));

            return (Vector2.DistanceSquared(Center, NearestPoint) < (Radius * Radius));
        }

        public bool IntersectsAny(List<Circle> targetList)
        {
            bool intersection = false;

            for (int i = 0; i < targetList.Count; i++)
            {
                if (Intersects(targetList[i])) { intersection = true; break; }
            }

            return intersection;
        }

        public bool IntersectsAny(List<Rectangle> targetList)
        {
            bool intersection = false;

            for (int i = 0; i < targetList.Count; i++)
            {
                if (Intersects(targetList[i])) { intersection = true; break; }
            }

            return intersection;
        }

        public bool Contains(Vector2 location)
        {
            return Vector2.DistanceSquared(Center, location) < Radius * Radius;
        }
    }
}
