using System.Xml.Linq;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Animation.Skeletons;
using Leda.Core.Serialization;

namespace Leda.Core.Animation
{
    public class SkeletalAnimationEngine : AnimationEngineBase
    {
        private ISkeleton _target;
        private Dictionary<string, SkeletalKeyframe.DataContainer> _boneFrameStartValues;
        private Dictionary<string, SkeletalKeyframe.DataContainer> _boneFrameEndValues;

        public SkeletalAnimationEngine(ISkeleton target)
        {
            _target = target;
            if (_target is ISerializable) { ID = string.Concat(((ISerializable)_target).ID, "-skeletal-animation-engine"); }

            _boneFrameStartValues = new Dictionary<string, SkeletalKeyframe.DataContainer>();
            _boneFrameEndValues = new Dictionary<string, SkeletalKeyframe.DataContainer>();
        }

        protected override void StartNextKeyframe()
        {
            base.StartNextKeyframe();

            foreach (KeyValuePair<string, IBone> bone in _target.Bones)
            {
                SetCurrentValuesAsAnimationStartPoints(bone.Key, bone.Value);
                SetFrameEndpoints(bone.Key, (SkeletalKeyframe)_sequence.Keyframes[_currentKeyframeIndex]);
                UpdateSpritesheetSettings(bone.Key, bone.Value, (SkeletalKeyframe)_sequence.Keyframes[_currentKeyframeIndex]);
            }
        }

        private void SetCurrentValuesAsAnimationStartPoints(string boneID, IBone bone)
        {
            if (!_boneFrameStartValues.ContainsKey(boneID)) { _boneFrameStartValues.Add(boneID, new SkeletalKeyframe.DataContainer()); }

            _boneFrameStartValues[boneID].Offset = bone.RelativePosition;
            _boneFrameStartValues[boneID].Rotation = Utility.RectifyAngle(bone.RelativeRotation);
            _boneFrameStartValues[boneID].Scale = bone.RelativeScale;
        }

        private void SetFrameEndpoints(string boneID, SkeletalKeyframe keyFrame)
        {
            if (!_boneFrameEndValues.ContainsKey(boneID)) { _boneFrameEndValues.Add(boneID, new SkeletalKeyframe.DataContainer()); }

            if (keyFrame.BoneAnimationData.ContainsKey(boneID))
            {
                if (!float.IsNegativeInfinity(keyFrame.BoneAnimationData[boneID].Offset.X))
                {
                    _boneFrameEndValues[boneID].Offset = keyFrame.BoneAnimationData[boneID].Offset;
                }
                else
                {
                    _boneFrameEndValues[boneID].Offset = _boneFrameStartValues[boneID].Offset;
                }

                if (keyFrame.BoneAnimationData[boneID].Rotation >= 0.0f)
                {
                    _boneFrameEndValues[boneID].Rotation = _boneFrameStartValues[boneID].Rotation +
                        Utility.AngleDifference(_boneFrameStartValues[boneID].Rotation, keyFrame.BoneAnimationData[boneID].Rotation);
                }
                else
                {
                    _boneFrameEndValues[boneID].Rotation = _boneFrameStartValues[boneID].Rotation;
                }

                if (keyFrame.BoneAnimationData[boneID].Scale >= 0.0f)
                {
                    _boneFrameEndValues[boneID].Scale = keyFrame.BoneAnimationData[boneID].Scale;
                }
                else
                {
                    _boneFrameEndValues[boneID].Scale = _boneFrameStartValues[boneID].Scale;
                }
            }
            else
            {
                _boneFrameEndValues[boneID] = _boneFrameStartValues[boneID];
            }
        }

        private void UpdateSpritesheetSettings(string boneID, IBone bone, SkeletalKeyframe keyFrame)
        {
            if (keyFrame.BoneAnimationData.ContainsKey(boneID))
            {
                bone.ApplySkin(
                    keyFrame.BoneAnimationData[boneID].TextureName,
                    keyFrame.BoneAnimationData[boneID].Origin, 
                    keyFrame.BoneAnimationData[boneID].SourceArea);
            }
        }

        public override void UpdateAnimation(int millisecondsSinceLastUpdate)
        {
            if (AnimationIsRunning)
            {
                base.UpdateAnimation(millisecondsSinceLastUpdate);

                if (AnimationIsRunning)
                {
                    UpdateTransformation(KeyframeProgress);
                }
            }
        }

        private void UpdateTransformation(float progressFraction)
        {
            foreach (KeyValuePair<string, IBone> bone in _target.Bones)
            {
                if ((_boneFrameStartValues.ContainsKey(bone.Key)) && (_boneFrameEndValues.ContainsKey(bone.Key)))
                {
                    bone.Value.RelativePosition = Vector2.Lerp(_boneFrameStartValues[bone.Key].Offset, _boneFrameEndValues[bone.Key].Offset, progressFraction);
                    bone.Value.RelativeRotation = MathHelper.Lerp(_boneFrameStartValues[bone.Key].Rotation, _boneFrameEndValues[bone.Key].Rotation, progressFraction);
                    bone.Value.RelativeScale = MathHelper.Lerp(_boneFrameStartValues[bone.Key].Scale, _boneFrameEndValues[bone.Key].Scale, progressFraction);
                }
            }
        }

        protected override void HandleKeyframeCompletion()
        {
            UpdateTransformation(1.0f);

            base.HandleKeyframeCompletion();
        }

        protected override XElement Serialize(Serializer serializer)
        {
            base.Serialize(serializer);

            serializer.AddDataElement(CreateStateNode("starting-values",_boneFrameStartValues));
            serializer.AddDataElement(CreateStateNode("target-values", _boneFrameEndValues));

            return serializer.SerializedData;
        }

        private XElement CreateStateNode(string nodeName, Dictionary<string, SkeletalKeyframe.DataContainer> data)
        {
            XElement node = new XElement(nodeName);
            foreach (KeyValuePair<string, SkeletalKeyframe.DataContainer> bone in data)
            {
                node.Add(bone.Value.Serialize(bone.Key));
            }

            return node;
        }

        protected override void Deserialize(Serializer serializer)
        {
            _boneFrameStartValues = GetStateData(serializer.GetDataElement("starting-values"));
            _boneFrameEndValues = GetStateData(serializer.GetDataElement("target-values"));

            foreach (KeyValuePair<string, SkeletalKeyframe.DataContainer> kvp in _boneFrameEndValues)
            {
                if (kvp.Value.Rotation < 0.0f) { kvp.Value.Rotation = _boneFrameStartValues[kvp.Key].Rotation; }
                if (kvp.Value.Scale < 0.0f) { kvp.Value.Rotation = _boneFrameStartValues[kvp.Key].Scale; }
            }

            base.Deserialize(serializer);
        }

        private Dictionary<string, SkeletalKeyframe.DataContainer> GetStateData(XElement serializedData)
        {
            Dictionary<string, SkeletalKeyframe.DataContainer> data = new Dictionary<string, SkeletalKeyframe.DataContainer>();
            foreach (XElement bone in serializedData.Elements("bone"))
            {
                SkeletalKeyframe.DataContainer boneData = new SkeletalKeyframe.DataContainer();
                boneData.Deserialize(bone);
                data.Add(bone.Attribute("id").Value, boneData);
            }

            return data;
        }

        protected override IKeyframe DeserializeKeyframe(XElement serializedData)
        {
            SkeletalKeyframe newKeyframe = new SkeletalKeyframe();
            newKeyframe.Deserialize(serializedData);

            return newKeyframe;
        }
    }
}
