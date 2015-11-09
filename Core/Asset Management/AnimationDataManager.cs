using System;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Leda.Core;
using Leda.Core.Animation;

namespace Leda.Core.Asset_Management
{
    public sealed class AnimationDataManager
    {
        private static Dictionary<string, AnimationSequence> _sequences;

        public static Dictionary<string, AnimationSequence> Sequences 
        {
            get { if (_sequences == null) { _sequences = new Dictionary<string, AnimationSequence>(); } return _sequences; } 
        }

        public static void AddSequence(string name, AnimationSequence.AnimationSequenceType sequenceType, bool loops)
        {
            AnimationSequence newSequence = new AnimationSequence(sequenceType);
            newSequence.Loops = loops;

            Sequences.Add(name, newSequence);
        }

        public static void AddKeyframe(string sequenceName, IKeyframe newKeyframe)
        {
            AnimationSequence.AnimationSequenceType sequenceTypeFromKeyframe = AnimationSequence.AnimationSequenceType.Transformation;
            if (newKeyframe is SpriteSheetKeyframe) { sequenceTypeFromKeyframe = AnimationSequence.AnimationSequenceType.SpriteSheet; }
            if (newKeyframe is ColourKeyframe) { sequenceTypeFromKeyframe = AnimationSequence.AnimationSequenceType.Colour; }
            if (newKeyframe is SkeletalKeyframe) { sequenceTypeFromKeyframe = AnimationSequence.AnimationSequenceType.Skeletal; }

            if (!Sequences.ContainsKey(sequenceName)) { AddSequence(sequenceName, sequenceTypeFromKeyframe, false); }

            if (Sequences[sequenceName].SequenceType == sequenceTypeFromKeyframe) { Sequences[sequenceName].Keyframes.Add(newKeyframe); }
        }

        public static void AddSequence(XElement xmlSequenceData)
        {
            string sequenceName = "";
            AnimationSequence.AnimationSequenceType sequenceType = AnimationSequence.AnimationSequenceType.Transformation;

            if (!Sequences.ContainsKey(xmlSequenceData.Attribute("name").Value))
            {
                foreach (AnimationSequence.AnimationSequenceType item in Enum.GetValues(typeof(AnimationSequence.AnimationSequenceType)))
                {
                    if (item.ToString().ToLower().Equals(xmlSequenceData.Attribute("type").Value.Trim().ToLower()))
                    {
                        sequenceName = xmlSequenceData.Attribute("name").Value;
                        sequenceType = item;

                        AddSequence(sequenceName, item, xmlSequenceData.Attribute("loops").Value == "yes");
                        break;
                    }
                }
            }

            if (!string.IsNullOrEmpty(sequenceName))
            {
                foreach (XElement frame in xmlSequenceData.Element("keyframes").Elements())
                {
                    switch (sequenceType)
                    {
                        case AnimationSequence.AnimationSequenceType.Transformation:
                            AddTransformationKeyframe(sequenceName, frame);
                            break;
                        case AnimationSequence.AnimationSequenceType.SpriteSheet:
                            AddSpriteSheetKeyframe(sequenceName, frame);
                            break;
                        case AnimationSequence.AnimationSequenceType.Colour:
                            AddColourKeyframe(sequenceName, frame);
                            break;
                        case AnimationSequence.AnimationSequenceType.Skeletal:
                            AddSkeletalKeyframe(sequenceName, frame);
                            break;
                    }
                }
            }
        }

        private static void AddTransformationKeyframe(string sequenceName, XElement xmlFrameData)
        {
            TransformationKeyframe newKeyframe = new TransformationKeyframe();
            newKeyframe.DurationInMilliseconds = Convert.ToInt32(xmlFrameData.Attribute("duration").Value);
            newKeyframe.TargetRotation = MathHelper.ToRadians((float)xmlFrameData.Attribute("rotation"));
            newKeyframe.TargetScale = (float)xmlFrameData.Attribute("scale");

            AddKeyframe(sequenceName, newKeyframe);
        }

        private static void AddSpriteSheetKeyframe(string sequenceName, XElement xmlFrameData)
        {
            SpriteSheetKeyframe newKeyframe = new SpriteSheetKeyframe();
            newKeyframe.DurationInMilliseconds = Convert.ToInt32(xmlFrameData.Attribute("duration").Value);

            if (xmlFrameData.Element("texturename") != null) { newKeyframe.TextureName = xmlFrameData.Element("texturename").Value; }
            else { newKeyframe.TextureName = ""; }

            newKeyframe.SourceArea = new Rectangle(
                Convert.ToInt32(xmlFrameData.Element("sourcearea").Attribute("left").Value),
                Convert.ToInt32(xmlFrameData.Element("sourcearea").Attribute("top").Value),
                Convert.ToInt32(xmlFrameData.Element("sourcearea").Attribute("width").Value),
                Convert.ToInt32(xmlFrameData.Element("sourcearea").Attribute("height").Value));

            AddKeyframe(sequenceName, newKeyframe);
        }

        private static void AddColourKeyframe(string sequenceName, XElement xmlFrameData)
        {
            ColourKeyframe newKeyframe = new ColourKeyframe();
            newKeyframe.DurationInMilliseconds = Convert.ToInt32(xmlFrameData.Attribute("duration").Value);

            newKeyframe.TargetTint = new Color(
                Convert.ToByte(xmlFrameData.Attribute("tint-r").Value),
                Convert.ToByte(xmlFrameData.Attribute("tint-g").Value),
                Convert.ToByte(xmlFrameData.Attribute("tint-b").Value),
                Convert.ToByte(xmlFrameData.Attribute("tint-a").Value));

            AddKeyframe(sequenceName, newKeyframe);
        }

        private static void AddSkeletalKeyframe(string sequenceName, XElement xmlFrameData)
        {
            SkeletalKeyframe newKeyframe = new SkeletalKeyframe();
            newKeyframe.DurationInMilliseconds = (int)xmlFrameData.Attribute("duration");

            foreach (XElement bone in xmlFrameData.Elements("bonedata"))
            {
                SkeletalKeyframe.DataContainer boneKeyframeData = new SkeletalKeyframe.DataContainer();

                if (bone.Attribute("rotation") != null) { boneKeyframeData.Rotation = Utility.RectifyAngle(MathHelper.ToRadians((float)bone.Attribute("rotation"))); }
                if (bone.Attribute("scale") != null) { boneKeyframeData.Scale = (float)bone.Attribute("scale"); }
                if ((bone.Attribute("offset-x") != null) && (bone.Attribute("offset-y") != null))
                {
                    boneKeyframeData.Offset = new Vector2((float)bone.Attribute("offset-x"), (float)bone.Attribute("offset-y"));
                }

                if (bone.Element("texture") != null)
                {
                    if (bone.Element("texture").Attribute("name") != null) { boneKeyframeData.TextureName = bone.Element("texture").Attribute("name").Value; }

                    if ((bone.Attribute("origin-x") != null) && (bone.Attribute("origin-y") != null))
                    {
                        boneKeyframeData.Origin = new Vector2((float)bone.Attribute("origin-x"), (float)bone.Attribute("origin-y"));
                    }

                    if ((bone.Element("texture").Attribute("frame-x") != null) && (bone.Element("texture").Attribute("frame-y") != null) &&
                        (bone.Element("texture").Attribute("frame-width") != null) && (bone.Element("texture").Attribute("frame-height") != null))
                    {
                        Rectangle frame = new Rectangle((int)bone.Element("texture").Attribute("frame-x"), (int)bone.Element("texture").Attribute("frame-y"),
                            (int)bone.Element("texture").Attribute("frame-width"), (int)bone.Element("texture").Attribute("frame-height"));
                        boneKeyframeData.SourceArea = frame;
                    }
                }

                newKeyframe.BoneAnimationData.Add(bone.Attribute("id").Value, boneKeyframeData);
            }

            AddKeyframe(sequenceName, newKeyframe);
        }
    }
}
