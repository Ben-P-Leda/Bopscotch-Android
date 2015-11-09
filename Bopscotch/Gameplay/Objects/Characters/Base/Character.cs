using System.Xml.Linq;

using Microsoft.Xna.Framework;

using Leda.Core;
using Leda.Core.Game_Objects.Base_Classes;
using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Motion;
using Leda.Core.Animation;
using Leda.Core.Serialization;

namespace Bopscotch.Gameplay.Objects.Characters.Base
{
    public class Character : StorableSkeleton, IMobile, IAnimated, ICameraRelative, ICollidable, IHasLifeCycle, IPausable
    {
        public virtual IMotionEngine MotionEngine { get; set; }

        private SkeletalAnimationEngine _animationEngine;
        public IAnimationEngine AnimationEngine { get { return _animationEngine; } }

        protected LifeCycleStateValue _lifeCycleState;
        public LifeCycleStateValue LifeCycleState { get { return _lifeCycleState; } set { UpdateLifeCycleState(value); } }
        public bool ReadyForDisposal { get; set; }

        public bool Collidable { get; set; }
        public bool Paused { private get; set; }

        public Character()
            : base()
        {
            MotionEngine = null;
            _animationEngine = new SkeletalAnimationEngine(this);

            Visible = true;
            ReadyForDisposal = false;

            LifeCycleState = LifeCycleStateValue.Active;
        }

        public virtual void Update(int millisecondsSinceLastUpdate)
        {
            if ((!Paused) && (MotionEngine != null))
            {
                MotionEngine.Update(millisecondsSinceLastUpdate);

                WorldPosition += MotionEngine.Delta;
            }
        }

        public virtual void HandleCollision(ICollidable collider)
        {
        }

        private void UpdateLifeCycleState(LifeCycleStateValue newLifeCycleState)
        {
            _lifeCycleState = newLifeCycleState;

            switch (newLifeCycleState)
            {
                case LifeCycleStateValue.Active: Collidable = true; break;
                case LifeCycleStateValue.Exiting: StartExitSequence(); break;
                case LifeCycleStateValue.ReadyForRemoval: ReadyForDisposal = true; break;
            }
        }

        protected virtual void StartExitSequence()
        {
            Collidable = false;
        }

        public void PrepareForDisposal()
        {
            Visible = false;
        }

        protected override XElement Serialize(Serializer serializer)
        {
            base.Serialize(serializer);

            serializer.AddDataItem("life-cycle-state", _lifeCycleState);
            serializer.AddDataItem("collidable", Collidable);
            serializer.AddDataItem("ready-for-disposal", ReadyForDisposal);

            return serializer.SerializedData;
        }

        protected override Serializer Deserialize(Serializer serializer)
        {
            base.Deserialize(serializer);

            _lifeCycleState = serializer.GetDataItem<LifeCycleStateValue>("life-cycle-state");
            Collidable = serializer.GetDataItem<bool>("collidable");
            ReadyForDisposal = serializer.GetDataItem<bool>("ready-for-disposal");

            return serializer;
        }
    }
}
