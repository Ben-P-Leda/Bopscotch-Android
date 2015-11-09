using System;
using System.Xml.Linq;

using Microsoft.Xna.Framework;

using Leda.Core;
using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Serialization;

namespace Leda.Core.Motion.Engines
{
    public abstract class EasingMotionEnginebase : IMotionEngine, ISerializable
    {
        public delegate void MovementCompletionCallback();

        protected Vector2 _delta;
        protected float _movementAngle;
        protected Vector2 _movementVector;
        protected float _speed;

        public string ID { get; set; }
        public Vector2 Delta { get { return _delta; } }

        public IMobile ObjectToTrack { protected get; set; }
        public Vector2 TargetWorldPosition { protected get; set; }
        public float Speed { protected get; set; }
        public float RecoilRate { protected get; set; }
        public float RecoilMultiplier { protected get; set; }
        public MovementCompletionCallback CompletionCallback { protected get; set; }

        public EasingMotionEnginebase() 
        {
            _delta = Vector2.Zero;
            _speed = 0.0f;

            TargetWorldPosition = Vector2.Zero;
            ObjectToTrack = null;
            Speed = Default_Speed;
            RecoilRate = Default_Recoil_Rate;
            RecoilMultiplier = Default_Recoil_Multiplier;

            CompletionCallback = null;
        }

        public virtual void Activate()
        {
            _speed = Speed;
        }

        public void Update(int millisecondsSinceLastUpdate)
        {
            if (_movementVector != Vector2.Zero) { CalculateDelta(millisecondsSinceLastUpdate); }
            else { _delta = Vector2.Zero; }
        }

        public abstract void CalculateDelta(int millisecondsSinceLastUpdate);

        public XElement Serialize()
        {
            return Serialize(new Serializer(this));
        }

        protected virtual XElement Serialize(Serializer serializer)
        {
            return serializer.SerializedData;
        }

        public void Deserialize(XElement serializedData)
        {
            Deserialize(new Serializer(serializedData));
        }

        protected virtual void Deserialize(Serializer serializer)
        {
        }

        private const float Default_Speed = 7.0f;
        private const float Default_Recoil_Rate = 0.9f;
        private const int Default_Recoil_Multiplier = 1;

        protected const float Direction_Switch_Speed = 0.5f;
    }
}
