using System;
using System.Xml.Linq;

using Microsoft.Xna.Framework;

using Leda.Core.Serialization;
using Leda.Core.Game_Objects.Behaviours;

namespace Leda.Core.Timing
{
    public sealed class Timer : ISerializable
    {
        public delegate void ActionCompletionCallback();

        private float _currentActionTotalDuration;
        private float _currentActionElapsedDuration;
        private float _actionSpeed;
        private ActionCompletionCallback _handleCompletionCallback;

        public string ID { get; set; }
        public float NextActionDuration { set { _currentActionTotalDuration = Math.Max(0.0f, value); _currentActionElapsedDuration = 0.0f; } }
        public float ActionSpeed { set { _actionSpeed = value; } }
        public ActionCompletionCallback ActionCompletionHandler { set { _handleCompletionCallback = value; } }

        public float CurrentActionProgress
        {
            get
            {
                if ((_currentActionTotalDuration < 1)) { return 1.0f; }
                else { return MathHelper.Clamp(_currentActionElapsedDuration / _currentActionTotalDuration, 0.0f, 1.0f); }
            }
        }

        public Timer(string id)
            : this(id, null)
        {
        }

        public Timer(string id, ActionCompletionCallback completionCallback)
        {
            ID = id;

            _handleCompletionCallback = completionCallback;

            Reset();
        }

        public void RegisterWithGlobalController()
        {
            GlobalTimerController.GlobalTimer.RegisterUpdateCallback(Tick);
        }

        public void Reset()
        {
            ActionSpeed = 1.0f;
            NextActionDuration = 0.0f;
        }

        public void Tick(int millisecondsSinceLast)
        {
            if (_handleCompletionCallback != null)
            {
                if (((CurrentActionProgress < 1.0f) && (_currentActionElapsedDuration + (millisecondsSinceLast * _actionSpeed) >= _currentActionTotalDuration)) ||
                    ((CurrentActionProgress == 0.0f) && (_currentActionTotalDuration <= MinimumActionDuration)))
                {
                    _handleCompletionCallback();
                }
            }

            _currentActionElapsedDuration = Math.Min(_currentActionElapsedDuration + (millisecondsSinceLast * _actionSpeed), _currentActionTotalDuration);
        }

        public XElement Serialize()
        {
            Serializer serializer = new Serializer(this);

            serializer.AddDataItem("duration", _currentActionTotalDuration);
            serializer.AddDataItem("elapsed", _currentActionElapsedDuration);
            serializer.AddDataItem("speed", _actionSpeed);

            return serializer.SerializedData;
        }

        public void Deserialize(XElement serializedData)
        {
            Serializer serializer = new Serializer(serializedData);

            _currentActionTotalDuration = serializer.GetDataItem<float>("duration");
            _currentActionElapsedDuration = serializer.GetDataItem<float>("elapsed");
            _actionSpeed = serializer.GetDataItem<float>("speed");
        }

        private const float MinimumActionDuration = 16.0f;
    }
}
