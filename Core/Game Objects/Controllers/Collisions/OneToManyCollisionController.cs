using System.Collections.Generic;

using Leda.Core.Game_Objects.Behaviours;

namespace Leda.Core.Game_Objects.Controllers.Collisions
{
    public class OneToManyCollisionController : CollisionControllerBase
    {
        private ColliderType _targetObjectType;

        private IBoxCollidable _boxColliderToTest;
        private ICircularCollidable _circularColliderToTest;
        private IPolygonCollidable _polygonColliderToTest;
        private ICompoundPolygonCollidable _compoundPolygonColliderToTest;

        public ICollidable ObjectToTest
        {
            set
            {
                if (value is IBoxCollidable) { _boxColliderToTest = (IBoxCollidable)value; _targetObjectType = ColliderType.Box; }
                if (value is ICircularCollidable) { _circularColliderToTest = (ICircularCollidable)value; _targetObjectType = ColliderType.Circular; }
                if (value is IPolygonCollidable) { _polygonColliderToTest = (IPolygonCollidable)value; _targetObjectType = ColliderType.SinglePolygon; }
                if (value is ICompoundPolygonCollidable) { _compoundPolygonColliderToTest = (ICompoundPolygonCollidable)value; _targetObjectType = ColliderType.CompoundPolygon; }
            }
        }

        public OneToManyCollisionController() 
            : base()
        {
            _targetObjectType = ColliderType.None;
        }

        public override void CheckForCollisions()
        {
            switch (_targetObjectType)
            {
                case ColliderType.Box: CheckIndividualBoxColliderToAllTypes(_boxColliderToTest); break;
                case ColliderType.Circular: CheckIndividualCircularColliderToAllTypes(_circularColliderToTest); break;
                case ColliderType.SinglePolygon: break;
                case ColliderType.CompoundPolygon: break;
            }
        }
    }
}
