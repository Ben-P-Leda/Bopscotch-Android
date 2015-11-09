using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Leda.Core;
using Leda.Core.Shapes;
using Leda.Core.Game_Objects.Behaviours;

namespace Leda.Core.Game_Objects.Controllers.Collisions
{
    public abstract class CollisionControllerBase
    {
        private List<IBoxCollidable> _boxColliders;
        private List<ICircularCollidable> _circularColliders;
        private List<IPolygonCollidable> _polygonColliders;
        private List<ICompoundPolygonCollidable> _compoundPolygonColliders;

        public CollisionControllerBase()
        {
            _boxColliders = new List<IBoxCollidable>();
            _circularColliders = new List<ICircularCollidable>();
            _polygonColliders = new List<IPolygonCollidable>();
            _compoundPolygonColliders = new List<ICompoundPolygonCollidable>();
        }

        public virtual void AddCollidableObject(ICollidable toAdd)
        {
            IBoxCollidable box = toAdd as IBoxCollidable;
            if ((box != null) && (!_boxColliders.Contains(box))) { _boxColliders.Add(box); return; }

            ICircularCollidable circle = toAdd as ICircularCollidable;
            if ((circle != null) && (!_circularColliders.Contains(circle))) { _circularColliders.Add(circle); return; }

            IPolygonCollidable poly = toAdd as IPolygonCollidable;
            if ((poly != null) && (!_polygonColliders.Contains(poly))) { _polygonColliders.Add(poly); return; }

            ICompoundPolygonCollidable compound = toAdd as ICompoundPolygonCollidable;
            if ((compound != null) && (!_compoundPolygonColliders.Contains(compound))) { _compoundPolygonColliders.Add(compound); return; }
        }

        public virtual void RemoveCollidableObject(ICollidable toRemove)
        {
            IBoxCollidable box = toRemove as IBoxCollidable;
            if ((box != null) && (_boxColliders.Contains(box))) { _boxColliders.Remove(box); return; }

            ICircularCollidable circle = toRemove as ICircularCollidable;
            if ((circle != null) && (_circularColliders.Contains(circle))) { _circularColliders.Remove(circle); return; }

            IPolygonCollidable poly = toRemove as IPolygonCollidable;
            if ((poly != null) && (_polygonColliders.Contains(poly))) { _polygonColliders.Remove(poly); return; }

            ICompoundPolygonCollidable compound = toRemove as ICompoundPolygonCollidable;
            if ((compound != null) && (_compoundPolygonColliders.Contains(compound))) { _compoundPolygonColliders.Remove(compound); return; }
        }

        public abstract void CheckForCollisions();

        protected void CheckAllBoxCollidersAgainstAllTypes()
        {
            int boxColliderCount = _boxColliders.Count;
            for (int i = 0; i < boxColliderCount; i++)
            {
                CheckIndividualBoxColliderToAllTypes(_boxColliders[i]);
            }
        }

        protected void CheckIndividualBoxColliderToAllTypes(IBoxCollidable subject)
        {
            if (subject.Collidable)
            {
                CheckIndividualBoxColliderToAllBoxColliders(subject);
                CheckIndividualBoxColliderToAllCircularColliders(subject);

                // TODO
                //CheckIndividualBoxColliderToAllPolygonColliders(_boxColliders[i]);
                //CheckIndividualBoxColliderToAllCompoundPolygonColliders(_boxColliders[i]);
            }
        }

        private void CheckIndividualBoxColliderToAllBoxColliders(IBoxCollidable subject)
        {
            if (subject.Collidable)
            {
                int boxColliderCount = _boxColliders.Count;
                for (int i = 0; i < boxColliderCount; i++)
                {
                    if ((_boxColliders[i] != subject) && (_boxColliders[i].Collidable) && (BoxCollidersHaveCollided(subject, _boxColliders[i])))
                    {
                        subject.HandleCollision(_boxColliders[i]);
                        _boxColliders[i].HandleCollision(subject);
                    }
                }
            }
        }

        public bool BoxCollidersHaveCollided(IBoxCollidable subject, IBoxCollidable target)
        {
            if (PositionedColliderBoundingBoxRequiresUpdate(subject)) { PositionBoxColliderBoundingBox(subject); }
            if (PositionedColliderBoundingBoxRequiresUpdate(target)) { PositionBoxColliderBoundingBox(target); }

            return BoundingBoxesHaveCollided(subject.PositionedCollisionBoundingBox, target.PositionedCollisionBoundingBox);
        }

        private bool PositionedColliderBoundingBoxRequiresUpdate(IBoxCollidable toCheck)
        {
            if (!toCheck.WorldPositionIsFixed) { return true; }

            return ((toCheck.PositionedCollisionBoundingBox == Rectangle.Empty) || (toCheck.PositionedCollisionBoundingBox == null));
        }

        private void PositionBoxColliderBoundingBox(IBoxCollidable sourceCollidable)
        {
            if (sourceCollidable.PositionedCollisionBoundingBox == null)
            {
                sourceCollidable.PositionedCollisionBoundingBox = Rectangle.Empty;
            }

            Rectangle temp = sourceCollidable.PositionedCollisionBoundingBox;

            temp.X = (int)sourceCollidable.WorldPosition.X + sourceCollidable.CollisionBoundingBox.X;
            temp.Y = (int)sourceCollidable.WorldPosition.Y + sourceCollidable.CollisionBoundingBox.Y;
            temp.Width = sourceCollidable.CollisionBoundingBox.Width;
            temp.Height = sourceCollidable.CollisionBoundingBox.Height;

            sourceCollidable.PositionedCollisionBoundingBox = temp;
        }

        private bool BoundingBoxesHaveCollided(Rectangle subject, Rectangle target)
        {
            return ((subject.Intersects(target)) || (subject.Contains(target)) || (target.Contains(subject)));
        }

        private void CheckIndividualBoxColliderToAllCircularColliders(IBoxCollidable subject)
        {
            if (subject.Collidable)
            {
                int circularColliderCount = _circularColliders.Count;
                for (int i = 0; i < circularColliderCount; i++)
                {
                    if ((_circularColliders[i].Collidable) && (BoxAndCircularCollidersHaveCollided(subject, _circularColliders[i])))
                    {
                        subject.HandleCollision(_circularColliders[i]);
                        _circularColliders[i].HandleCollision(subject);
                    }
                }
            }
        }

        public bool BoxAndCircularCollidersHaveCollided(IBoxCollidable subject, ICircularCollidable target)
        {
            if (PositionedColliderBoundingBoxRequiresUpdate(subject)) { PositionBoxColliderBoundingBox(subject); }
            if (PositionedColliderBoundingCircleRequiresUpdate(target)) { PositionCircularColliderBoundingCircle(target); }

            return target.PositionedCollisionBoundingCircle.Intersects(subject.PositionedCollisionBoundingBox);
        }

        private bool PositionedColliderBoundingCircleRequiresUpdate(ICircularCollidable toCheck)
        {
            if (!toCheck.WorldPositionIsFixed) { return true; }

            return ((toCheck.PositionedCollisionBoundingCircle == Circle.Empty) || (toCheck.PositionedCollisionBoundingCircle == null));
        }

        private void PositionCircularColliderBoundingCircle(ICircularCollidable sourceCollidable)
        {
            if (sourceCollidable.PositionedCollisionBoundingCircle == null)
            {
                sourceCollidable.PositionedCollisionBoundingCircle = Circle.Empty;
            }

            sourceCollidable.PositionedCollisionBoundingCircle.Center = sourceCollidable.WorldPosition;
            sourceCollidable.PositionedCollisionBoundingCircle.Radius = sourceCollidable.CollisionBoundingCircle.Radius;
        }

        protected void CheckAllCircularCollidersAgainstAllTypes()
        {
            int circularColliderCount = _circularColliders.Count;
            for (int i = 0; i < circularColliderCount; i++)
            {
                CheckIndividualCircularColliderToAllTypes(_circularColliders[i]);
            }
        }

        protected void CheckIndividualCircularColliderToAllTypes(ICircularCollidable subject)
        {
            if (subject.Collidable)
            {
                CheckIndividualCircularColliderToAllBoxColliders(subject);
                CheckIndividualCircularColliderToAllCircularColliders(subject);

                // TODO
                //CheckIndividualCircularColliderToAllPolygonColliders(_boxColliders[i]);
                //CheckIndividualCircularColliderToAllCompoundPolygonColliders(_boxColliders[i]);
            }
        }

        private void CheckIndividualCircularColliderToAllBoxColliders(ICircularCollidable subject)
        {
            if (subject.Collidable)
            {
                int boxColliderCount = _boxColliders.Count;
                for (int i = 0; i < boxColliderCount; i++)
                {
                    if ((_boxColliders[i].Collidable) && (BoxAndCircularCollidersHaveCollided(_boxColliders[i], subject)))
                    {
                        subject.HandleCollision(_boxColliders[i]);
                        _boxColliders[i].HandleCollision(subject);
                    }
                }
            }
        }

        private void CheckIndividualCircularColliderToAllCircularColliders(ICircularCollidable subject)
        {
            if (subject.Collidable)
            {
                int circularColliderCount = _circularColliders.Count;
                for (int i = 0; i < circularColliderCount; i++)
                {
                    if ((_circularColliders[i] != subject) && (_circularColliders[i].Collidable) && (CircularCollidersHaveCollided(subject, _circularColliders[i])))
                    {
                        subject.HandleCollision(_circularColliders[i]);
                        _circularColliders[i].HandleCollision(subject);
                    }
                }
            }
        }

        public bool CircularCollidersHaveCollided(ICircularCollidable subject, ICircularCollidable target)
        {
            if (PositionedColliderBoundingCircleRequiresUpdate(subject)) { PositionCircularColliderBoundingCircle(subject); }
            if (PositionedColliderBoundingCircleRequiresUpdate(target)) { PositionCircularColliderBoundingCircle(target); }

            return subject.PositionedCollisionBoundingCircle.Intersects(target.PositionedCollisionBoundingCircle);
        }
    }
}
