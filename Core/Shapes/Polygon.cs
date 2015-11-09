using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

namespace Leda.Core.Shapes
{
    public class Polygon
    {
        public List<Vector2> Vertices;

        public static Polygon Empty { get { return new Polygon(); } }

        public Rectangle BoundingBox
        {
            get
            {
                if (Vertices.Count > 2)
                {
                    Range xDimensions = new Range(Vertices[0].X);
                    Range yDimensions = new Range(Vertices[0].Y);

                    for (int i = 1; i < Vertices.Count; i++)
                    {
                        xDimensions.Expand(Vertices[i].X);
                        yDimensions.Expand(Vertices[i].Y);
                    }

                    return new Rectangle((int)xDimensions.Minimum, (int)yDimensions.Minimum, (int)(xDimensions.Interval), (int)(yDimensions.Interval));
                }
                else
                {
                    return Rectangle.Empty;
                }
            }
        }

        public Vector2 Center
        {
            get
            {
                Vector2 total = Vector2.Zero;
                for (int i = 0; i < Vertices.Count; i++) { total += Vertices[i]; }
                return total / Vertices.Count;
            }
        }

        public Polygon()
        {
            Vertices = new List<Vector2>();
        }

        public Polygon(List<Vector2> vertices)
            : this()
        {
            Vertices = vertices;
        }

        public Polygon Clone()
        {
            Polygon copy = new Polygon();
            for (int i = 0; i < Vertices.Count; i++) { copy.Vertices.Add(new Vector2(Vertices[i].X, Vertices[i].Y)); }

            return copy;
        }

        public Polygon Transform(Vector2 translation, float rotation, float scale, bool mirror)
        {
            Polygon transformed = this.Clone();
            for (int i = 0; i < transformed.Vertices.Count; i++)
            {
                transformed.Vertices[i] = Utility.Rotate(transformed.Vertices[i], rotation) * scale;
                if (mirror) { transformed.Vertices[i] = new Vector2(-transformed.Vertices[i].X, transformed.Vertices[i].Y); }
                transformed.Vertices[i] += translation;
            }

            return transformed;
        }

        public bool Intersects(Polygon target)
        {
            if ((this.Vertices.Count > 1) && (target.Vertices.Count > 1))
            {
                bool hasIntersected = true;
                List<Vector2> edges = new List<Vector2>();
                edges.AddRange(GetEdges(this));
                edges.AddRange(GetEdges(target));

                for (int i = 0; i < edges.Count; i++)
                {
                    Vector2 axis = new Vector2(-edges[i].Y, edges[i].X);
                    axis.Normalize();

                    Range subjectProjection = ProjectOntoAxis(axis, this);
                    Range targetProjection = ProjectOntoAxis(axis, target);

                    if (IntervalDistance(subjectProjection, targetProjection) > 0) { hasIntersected = false; break; }
                }

                return hasIntersected;
            }

            return false;
        }

        public Vector2 Separate(Polygon target, Vector2 velocity)
        {
            if ((this.Vertices.Count > 1) && (target.Vertices.Count > 1))
            {
                bool hasIntersected = true;
                bool willIntersect = true;

                Vector2 separationAxis = Vector2.Zero;
                float separationInterval = float.PositiveInfinity;

                List<Vector2> edges = new List<Vector2>();
                edges.AddRange(GetEdges(this));
                edges.AddRange(GetEdges(target));

                for (int i = 0; i < edges.Count; i++)
                {
                    Vector2 axis = new Vector2(-edges[i].Y, edges[i].X);
                    axis.Normalize();

                    Range subjectProjection = ProjectOntoAxis(axis, this);
                    Range targetProjection = ProjectOntoAxis(axis, target);

                    if (IntervalDistance(subjectProjection, targetProjection) > 0) { hasIntersected = false; }

                    float velocityProjection = Vector2.Dot(axis, velocity);
                    if (velocityProjection < 0) { subjectProjection.Minimum += velocityProjection; }
                    else { subjectProjection.Maximum += velocityProjection; }

                    float intervalDistance = IntervalDistance(subjectProjection, targetProjection);
                    if (intervalDistance > 0) { willIntersect = false; }

                    if ((!hasIntersected) && (!willIntersect)) { break; }

                    if ((intervalDistance <= 0.0f) && (Math.Abs(intervalDistance) < separationInterval))
                    {
                        separationInterval = Math.Min(Math.Abs(intervalDistance), separationInterval);
                        separationAxis = axis;

                        if (Vector2.Dot(this.Center - target.Center, separationAxis) < 0.0f) { separationAxis = -separationAxis; }
                    }
                }

                if (willIntersect) { return separationAxis * separationInterval; }
            }

            return Vector2.Zero;
        }

        private List<Vector2> GetEdges(Polygon edgeSource)
        {
            List<Vector2> edges = new List<Vector2>();

            for (int i = 0; i < edgeSource.Vertices.Count; i++)
            {
                edges.Add(edgeSource.Vertices[(i + 1) % edgeSource.Vertices.Count] - edgeSource.Vertices[i]);
            }

            return edges;
        }

        private Range ProjectOntoAxis(Vector2 axis, Polygon toProject)
        {
            Range projection = new Range(Vector2.Dot(axis, toProject.Vertices[0]));
            for (int i = 1; i < toProject.Vertices.Count; i++)
            {
                projection.Expand(Vector2.Dot(toProject.Vertices[i], axis));
            }

            return projection;
        }

        private float IntervalDistance(Range rangeOne, Range rangeTwo)
        {
            if (rangeOne.Minimum < rangeTwo.Minimum) { return rangeTwo.Minimum - rangeOne.Maximum; }
            else { return rangeOne.Minimum - rangeTwo.Maximum; }
        }

        public static Polygon FromRectangle(Rectangle source)
        {
            Polygon convertedRectangle = new Polygon();
            convertedRectangle.Vertices.Add(new Vector2(source.X, source.Y));
            convertedRectangle.Vertices.Add(new Vector2(source.X + source.Width, source.Y));
            convertedRectangle.Vertices.Add(new Vector2(source.X + source.Width, source.Y + source.Height));
            convertedRectangle.Vertices.Add(new Vector2(source.X, source.Y + source.Height));

            return convertedRectangle;
        }
    }
}
