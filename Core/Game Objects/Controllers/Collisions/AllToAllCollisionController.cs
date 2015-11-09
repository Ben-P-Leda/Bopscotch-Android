//using System;
//using System.Collections.Generic;

//using Microsoft.Xna.Framework;

//using Leda.Core;
//using Leda.Core.Shapes;
//using Leda.Core.Game_Objects.Behaviours;

//namespace Leda.Core.Game_Objects.Controllers.Collisions
//{
//    public class AllToAllCollisionController : CollisionControllerBase
//    {
//        private List<ICollidable> _collidableObjects;

//        public AllToAllCollisionController()
//        {
//            _collidableObjects = new List<ICollidable>();
//        }

//        public void AddCollidableObject(ICollidable toAdd)
//        {
//            if (!_collidableObjects.Contains(toAdd)) { _collidableObjects.Add(toAdd); }
//        }

//        public void RemoveCollidableObject(ICollidable toRemove)
//        {
//            if (_collidableObjects.Contains(toRemove)) { _collidableObjects.Remove(toRemove); }
//        }

//        public override void CheckForCollisions()
//        {
//            for (int subject = 0; subject < _collidableObjects.Count; subject++)
//            {
//                if (_collidableObjects[subject].Collidable)
//                {
//                    for (int target = 0; target < _collidableObjects.Count; target++)
//                    {
//                        if ((subject != target) && (_collidableObjects[target].Collidable))
//                        {
//                            CheckForAndHandleCollision(_collidableObjects[subject], _collidableObjects[target]);

//                            if (!_collidableObjects[target].Collidable) { break; }
//                        }
//                    }
//                    if (!_collidableObjects[subject].Collidable) { break; }
//                }
//            }
//        }
//    }
//}
