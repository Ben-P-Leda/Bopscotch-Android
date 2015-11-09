using System;
using System.Xml.Linq;

using Microsoft.Xna.Framework;

using Leda.Core;
using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Serialization;

namespace Leda.Core.Motion.Engines
{
    public class LinearMotionEngine : IMotionEngine, ISerializable
    {
        public string ID { get; set; }

        private float _angle;
        private float _negativeLimitDistance;
        private float _positiveLimitDistance;

        private Vector2 _delta;
        private Vector2 _origin;
        private Vector2 _negativeLimitEndpoint;
        private Vector2 _positiveLimitEndpoint;
        private Vector2 _currentTarget;

        public Vector2 Origin { set { _origin = value; UpdateMetrics(); } }
        public float Angle { set { _angle = value; UpdateMetrics(); } }
        public float NegativeLimitDistance { set { _negativeLimitDistance = value; UpdateMetrics(); } }
        public float PositiveLimitDistance { set { _positiveLimitDistance = value; UpdateMetrics(); } }

        public float MovementSpeed { private get; set; }
        public IWorldObject ObjectToMove { private get; set; }

        public Vector2 Delta { get { return _delta * MovementSpeed; } }
        public Vector2 MotionRange 
        {
            get
            {
                return new Vector2(
                    Math.Abs(_positiveLimitEndpoint.X - _negativeLimitEndpoint.X),
                    Math.Abs(_positiveLimitEndpoint.Y - _negativeLimitEndpoint.Y));
            }
        }

        public LinearMotionEngine()
        {
            MovementSpeed = 0.0f;

            _delta = Vector2.Zero;
            _currentTarget = Vector2.Zero;
        }

        public void Update(int millisecondsSinceLastUpdate)
        {
            if (_currentTarget == Vector2.Zero) { _currentTarget = _negativeLimitEndpoint; }

            if (Vector2.DistanceSquared(_origin, _currentTarget) < Vector2.DistanceSquared(_origin, ObjectToMove.WorldPosition))
            {
                float angleToEndpoint = Utility.PointToPointAngle(_origin, _currentTarget);
                float angleToObject = Utility.PointToPointAngle(_origin, ObjectToMove.WorldPosition);

                if (Math.Abs(Utility.AngleDifference(angleToEndpoint, angleToObject)) < AngleMargin)
                {
                    ObjectToMove.WorldPosition = _currentTarget;
                    if (_currentTarget == _negativeLimitEndpoint) { _currentTarget = _positiveLimitEndpoint; } else { _currentTarget = _negativeLimitEndpoint; }
                }
            }

            _delta = Vector2.Normalize(_currentTarget - ObjectToMove.WorldPosition) * millisecondsSinceLastUpdate;
        }

        public XElement Serialize()
        {
            Serializer serializer = new Serializer(this);

            serializer.AddDataItem("angle", _angle);
            serializer.AddDataItem("negativedistance", _negativeLimitDistance);
            serializer.AddDataItem("positivedistance", _positiveLimitDistance);

            serializer.AddDataItem("origin", _origin);
            serializer.AddDataItem("negativeendpoint", _negativeLimitEndpoint);
            serializer.AddDataItem("positiveendpoint", _positiveLimitEndpoint);
            serializer.AddDataItem("target", _currentTarget);

            serializer.AddDataItem("speed", MovementSpeed);

            return serializer.SerializedData;
        }

        public void Deserialize(XElement serializedData)
        {
            Serializer serializer = new Serializer(serializedData);

            _angle = serializer.GetDataItem<float>("angle");
            _negativeLimitDistance = serializer.GetDataItem<float>("negativedistance");
            _positiveLimitDistance = serializer.GetDataItem<float>("positivedistance");

            _origin = serializer.GetDataItem<Vector2>("origin");
            _negativeLimitEndpoint = serializer.GetDataItem<Vector2>("negativeendpoint");
            _positiveLimitEndpoint = serializer.GetDataItem<Vector2>("positiveendpoint");
            _currentTarget = serializer.GetDataItem<Vector2>("target");

            MovementSpeed = serializer.GetDataItem<float>("speed");
        }

        private void UpdateMetrics()
        {
            _negativeLimitEndpoint = (Utility.RotatedNormal(_angle) * _negativeLimitDistance) + _origin;
            _positiveLimitEndpoint = (Utility.RotatedNormal(_angle) * _positiveLimitDistance) + _origin;
        }

        private const float AngleMargin = 0.035f;       // Approx 2 degrees
    }
}
