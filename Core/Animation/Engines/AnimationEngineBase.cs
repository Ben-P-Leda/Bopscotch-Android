using System;
using System.Xml.Linq;

using Microsoft.Xna.Framework;

using Leda.Core.Serialization;

namespace Leda.Core.Animation
{
    public abstract class AnimationEngineBase : IAnimationEngine
    {
        public delegate void KeyframeCompletionCallback(int frameIndex);
        public delegate void SequenceCompletionCallback();

        private float _currentKeyframeMillisecondsElapsed;
        private KeyframeCompletionCallback _keyframeCompletionHandler;
        private SequenceCompletionCallback _sequenceCompletionHandler;

        protected int _currentKeyframeIndex;
        protected AnimationSequence _sequence;

        public string ID { get; set; }
        public AnimationSequence Sequence { set { _sequence = value; _currentKeyframeIndex = 0; StartNextKeyframe(); } }
        public KeyframeCompletionCallback KeyframeCompletionHandler { set { _keyframeCompletionHandler = value; } }
        public SequenceCompletionCallback SequenceCompletionHandler { set { _sequenceCompletionHandler = value; } }

        protected bool AnimationIsRunning { get { return ((_sequence != null) && (_currentKeyframeIndex < _sequence.FrameCount)); } }
        protected float KeyframeProgress
        {
            get { return MathHelper.Clamp(_currentKeyframeMillisecondsElapsed / _sequence.Keyframes[_currentKeyframeIndex].DurationInMilliseconds, 0.0f, 1.0f); }
        }


        public AnimationEngineBase()
        {
            ID = "";
            _keyframeCompletionHandler = null;
            _sequenceCompletionHandler = null;
        }

        protected virtual void StartNextKeyframe()
        {
            _currentKeyframeMillisecondsElapsed = 0;
        }

        public virtual void UpdateAnimation(int millisecondsSinceLastUpdate)
        {
            if (AnimationIsRunning)
            {
                _currentKeyframeMillisecondsElapsed += millisecondsSinceLastUpdate;
                if (_currentKeyframeMillisecondsElapsed >= _sequence.Keyframes[_currentKeyframeIndex].DurationInMilliseconds) { HandleKeyframeCompletion(); }
            }
        }

        protected virtual void HandleKeyframeCompletion()
        {
            if (_keyframeCompletionHandler != null) { _keyframeCompletionHandler(_currentKeyframeIndex); }

            if (AnimationIsRunning)
            {
                _currentKeyframeIndex++;

                if (_currentKeyframeIndex >= _sequence.FrameCount)
                {
                    if (_sequence.Loops) { _currentKeyframeIndex = 0; StartNextKeyframe(); }
                    else { HandleSequenceCompletion(); }
                }
                else 
                { 
                    StartNextKeyframe(); 
                }
            }
        }

        protected virtual void HandleSequenceCompletion()
        {
            if (_sequenceCompletionHandler != null) { _sequenceCompletionHandler(); }
        }

        public XElement Serialize()
        {
            if (string.IsNullOrEmpty(ID)) { throw new Exception("Cannot serialize animation engine - ID not set (check target is serializable)"); }

            return Serialize(new Serializer(this));
        }

        protected virtual XElement Serialize(Serializer serializer)
        {
            serializer.AddDataItem("current-frame-index", _currentKeyframeIndex);
            serializer.AddDataItem("current-frame-elapsed", _currentKeyframeMillisecondsElapsed);

            if (_sequence != null)
            {
                serializer.AddDataItem("sequence-type", _sequence.SequenceType);
                serializer.AddDataItem("loop-sequence", _sequence.Loops);

                XElement frames = new XElement("keyframes");
                for (int i = 0; i < _sequence.FrameCount; i++) { frames.Add(_sequence.Keyframes[i].Serialize()); }
                serializer.AddDataElement(frames);
            }

            return serializer.SerializedData;
        }

        public void Deserialize(XElement serializedData)
        {
            Deserialize(new Serializer(serializedData));
        }

        protected virtual void Deserialize(Serializer serializer)
        {
            _currentKeyframeIndex = serializer.GetDataItem<int>("current-frame-index");
            _currentKeyframeMillisecondsElapsed = serializer.GetDataItem<float>("current-frame-elapsed");

            _sequence = new AnimationSequence(serializer.GetDataItem<AnimationSequence.AnimationSequenceType>("sequence-type"));
            _sequence.Loops = serializer.GetDataItem<bool>("loop-sequence");

            XElement frames = serializer.GetDataElement("keyframes");
            if (frames != null)
            {
                foreach (XElement frame in frames.Elements("keyframe")) { _sequence.Keyframes.Add(DeserializeKeyframe(frame)); }
            }
        }

        protected abstract IKeyframe DeserializeKeyframe(XElement serializedData);
    }
}
