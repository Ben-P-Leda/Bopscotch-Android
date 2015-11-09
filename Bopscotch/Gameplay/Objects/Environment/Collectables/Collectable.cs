using System.Linq;
using System.Xml.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core;
using Leda.Core.Game_Objects.Base_Classes;
using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Serialization;
using Leda.Core.Asset_Management;
using Leda.Core.Motion;

using Bopscotch.Gameplay.Objects.Behaviours;

namespace Bopscotch.Gameplay.Objects.Environment.Collectables
{
    public abstract class Collectable : StorableSimpleDrawableObject, IBoxCollidable, IHasLifeCycle, IMobile, IPausable
    {
        private CollectableExitMotionEngine _motionEngine;
        public IMotionEngine MotionEngine { get { return _motionEngine; } }

        public delegate void CollectionCallbackMethod(Collectable collectedItem);

        public bool Collidable { get; set; }
        public Rectangle CollisionBoundingBox { get { return Frame; } }
        public Rectangle PositionedCollisionBoundingBox { get; set; }

        protected LifeCycleStateValue _lifeCycleState;
        public LifeCycleStateValue LifeCycleState { get { return _lifeCycleState; } set { UpdateLifeCycleState(value); } }
        public bool ReadyForDisposal { get; set; }

        public CollectionCallbackMethod CollectionCallback { private get; set; }

        public bool Paused { protected get; set; }

        public Collectable()
            : base()
        {
            _motionEngine = new CollectableExitMotionEngine();

            RenderLayer = Render_Layer;
            RenderDepth = Render_Depth;

            Visible = true;
            ReadyForDisposal = false;

            LifeCycleState = LifeCycleStateValue.Active;

            CollectionCallback = null;
        }

        public void SetTextureAndRelatedValues(string textureName)
        {
            if (!string.IsNullOrEmpty(textureName))
            {
                TextureReference = textureName;
                Texture = TextureManager.Textures[textureName];
                Frame = TextureManager.Textures[textureName].Bounds;
                Origin = new Vector2(Frame.Width, Frame.Height) / 2.0f;
            }
        }

        public virtual void Update(int millisecondsSinceLastUpdate)
        {
            if ((!Paused) && (MotionEngine != null))
            {
                MotionEngine.Update(millisecondsSinceLastUpdate);

                WorldPosition += MotionEngine.Delta;

                if (_lifeCycleState == LifeCycleStateValue.Exiting) 
                { 
                    Tint = Color.Lerp(Color.White, Color.Transparent, _motionEngine.Progress);
                    Scale = 1.0f + (_motionEngine.Progress / 2.0f);
                    if (_motionEngine.Progress == 1.0f) { UpdateLifeCycleState(LifeCycleStateValue.ReadyForRemoval); }
                }
            }
        }

        public void HandleCollision(ICollidable collider)
        {
            if (collider is Characters.Player.Player) { LifeCycleState = LifeCycleStateValue.Exiting; }
        }

        private void UpdateLifeCycleState(LifeCycleStateValue newLifeCycleState)
        {
            _lifeCycleState = newLifeCycleState;

            switch (newLifeCycleState)
            {
                case LifeCycleStateValue.Active: Collidable = true; break;
                case LifeCycleStateValue.Exiting: HandleCollection(); break;
                case LifeCycleStateValue.ReadyForRemoval: ReadyForDisposal = true; break;
            }
        }

        private void HandleCollection()
        {
            if (CollectionCallback != null) { CollectionCallback(this); }

            Collidable = false;
            StartExitSequence();
        }

        protected virtual void StartExitSequence()
        {
            _motionEngine.Activate(Default_Exit_Speed, Default_Exit_Duration);
        }

        public void PrepareForDisposal()
        {
            Visible = false;
        }

        protected override XElement Serialize(Serializer serializer)
        {
            base.Serialize(serializer);

            serializer.AddDataItem("collidable", Collidable);
            serializer.AddDataItem("life-cycle-state", _lifeCycleState);
            serializer.AddDataItem("ready-for-disposal", ReadyForDisposal);

            return serializer.SerializedData;
        }

        protected override Serializer Deserialize(Serializer serializer)
        {
            base.Deserialize(serializer);

            Collidable = serializer.GetDataItem<bool>("collidable");
            _lifeCycleState = serializer.GetDataItem<LifeCycleStateValue>("life-cycle-state");
            ReadyForDisposal = serializer.GetDataItem<bool>("ready-for-disposal");

            if (_lifeCycleState == LifeCycleStateValue.Exiting)
            {
                Visible = false;
                UpdateLifeCycleState(LifeCycleStateValue.ReadyForRemoval);
            }

            return serializer;
        }

        private const int Render_Layer = 2;
        private const float Render_Depth = 0.5f;

        private const float Default_Exit_Speed = 0.5f;
        private const int Default_Exit_Duration = 300;
    }
}
