using System;
using System.Xml.Linq;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core;
using Leda.Core.Game_Objects.Base_Classes;
using Leda.Core.Asset_Management;
using Leda.Core.Serialization;

namespace Leda.Core.Animation.Skeletons
{
    public class StorableBone : StorableSimpleDrawableObject, IBone
    {
        private Vector2 _relativePosition;
        private float _relativeRotation;
        private float _relativeScale;
        private float _relativeDepth;
        private Color _relativeTint;
        private Vector2 _relativeOrigin;

        private Color _masterTint;

        public IBone Parent { get; set; }
        public List<IBone> Children { get; private set; }

        public Vector2 RelativePosition
        {
            get { return _relativePosition; }
            set { _relativePosition = value; CalculateWorldPosition(); }
        }
        public Vector2 RelativeOrigin
        {
            get { return _relativeOrigin; }
            set { _relativeOrigin = value; if (Mirror) { value.X = -value.X; } Origin = value; CalculateWorldPosition(); }
        }
        public float RelativeRotation
        {
            get { return _relativeRotation; }
            set { _relativeRotation = Utility.RectifyAngle(value); CalculateWorldPosition(); }
        }
        public float RelativeScale
        {
            get { return _relativeScale; }
            set { _relativeScale = Math.Max(value, 0.0f); CalculateWorldPosition(); }
        }
        public float RelativeDepth
        {
            get { return _relativeDepth; }
            set { _relativeDepth = value; CalculateRenderDepth(); }
        }

        public override Color Tint
        {
            get { return base.Tint; }
            set { _relativeTint = value; SetDisplayTint(); CascadeRelativeTintToChildBones(); }
        }

        public virtual Color MasterTint
        {
            get { return _masterTint; }
            set { _masterTint = value; SetDisplayTint(); }
        }

        public StorableBone()
        {
            Parent = null;
            Children = new List<IBone>();

            _relativePosition = Vector2.Zero;
            _relativeOrigin = Vector2.Zero;
            _relativeRotation = 0;
            _relativeScale = 1.0f;
            _relativeTint = Color.White;
            _relativeDepth = 0.0f;

            _masterTint = Color.White;
        }

        public void CalculateWorldPosition()
        {
            if (Parent != null)
            {
                Vector2 offset = _relativePosition * Parent.Scale;
                Origin = _relativeOrigin;
                Rotation = Utility.RectifyAngle(Parent.Rotation + _relativeRotation);
                Scale = Parent.Scale * _relativeScale;
                Mirror = Parent.Mirror;

                if (Mirror)
                {
                    offset.X = -offset.X;
                    Origin = new Vector2(Frame.Width - _relativeOrigin.X, _relativeOrigin.Y);
                    Rotation = Utility.RectifyAngle(Parent.Rotation + (MathHelper.TwoPi - _relativeRotation));
                }

                offset = Utility.Rotate(offset, Parent.Rotation);
                WorldPosition = Parent.WorldPosition + offset;
            }

            for (int i = 0; i < Children.Count; i++) { Children[i].CalculateWorldPosition(); }
        }

        public void CalculateRenderDepth()
        {
            if (this.Parent != null) { CalculateRenderDepthFromLimbTreeLevel(); }

            for (int i = 0; i < Children.Count; i++) { Children[i].CalculateRenderDepth(); }
        }

        private void CalculateRenderDepthFromLimbTreeLevel()
        {
            IBone currentBone = this;
            float limbTreeLevelRenderDepthModifier = Depth_Modifier_From_Primary_To_Secondary_Limb_Levels;

            while (currentBone.Parent != null)
            {
                limbTreeLevelRenderDepthModifier *= Depth_Modifier_From_Secondary_To_Following_Limb_Levels;
                currentBone = currentBone.Parent;
            }

            RenderDepth = Parent.RenderDepth + (_relativeDepth / limbTreeLevelRenderDepthModifier);
        }

        private void SetDisplayTint()
        {
            base.Tint = new Color(MasterTint.ToVector4() * (_relativeTint.ToVector4() / Color.White.ToVector4()));
        }

        private void CascadeRelativeTintToChildBones()
        {
            if (Children != null)
            {
                for (int i = 0; i < Children.Count; i++) { Children[i].Tint = _relativeTint; }
            }
        }

        private void UpdateMirrorValueForChildren()
        {
            if (Children != null)
            {
                for (int i = 0; i < Children.Count; i++) { Children[i].Mirror = base.Mirror; }
                CalculateWorldPosition();
            }
        }

        public void ApplySkin(string skinTextureName, Vector2 originWithinSkinFrame, Rectangle frameAreaWithinSkinTexture)
        {
            if (!string.IsNullOrEmpty(skinTextureName))
            {
                TextureReference = skinTextureName;
                Texture = TextureManager.Textures[skinTextureName];
            }

            if (originWithinSkinFrame.X != float.NegativeInfinity)
            {
                _relativeOrigin = originWithinSkinFrame;
                Origin = _relativeOrigin;
            }

            if (frameAreaWithinSkinTexture != Rectangle.Empty)
            {
                Frame = frameAreaWithinSkinTexture;
            }
        }

        public void ClearSkin()
        {
            TextureReference = "";
            Texture = null;
            Frame = Rectangle.Empty;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            for (int i = 0; i < Children.Count; i++) { Children[i].Draw(spriteBatch); }
        }

        protected override XElement Serialize(Serializer serializer)
        {
            base.Serialize(serializer);

            serializer.AddDataItem("parent-id", Parent == null ? "" : Parent.ID);
            serializer.AddDataItem("relative-position", _relativePosition);
            serializer.AddDataItem("relative-origin", _relativeOrigin);
            serializer.AddDataItem("relative-rotation", _relativeRotation);
            serializer.AddDataItem("relative-scale", _relativeScale);
            serializer.AddDataItem("relative-depth", _relativeDepth);
            serializer.AddDataItem("relative-tint", _relativeTint);
            serializer.AddDataItem("master-tint", _masterTint);

            return serializer.SerializedData;
        }

        protected override Serializer Deserialize(Serializer serializer)
        {
            base.Deserialize(serializer);

            _relativePosition = serializer.GetDataItem<Vector2>("relative-position");
            _relativeOrigin = serializer.GetDataItem<Vector2>("relative-origin");
            _relativeRotation = serializer.GetDataItem<float>("relative-rotation");
            _relativeScale = serializer.GetDataItem<float>("relative-scale");
            _relativeDepth = serializer.GetDataItem<float>("relative-depth");
            _relativeTint = serializer.GetDataItem<Color>("relative-tint");
            _masterTint = serializer.GetDataItem<Color>("master-tint");

            SetDisplayTint();

            return serializer;
        }

        public void SetMetricsAndCreateChildrenFromXml(ISkeleton skeleton, XElement boneData)
        {
            _relativePosition = new Vector2((float)boneData.Attribute("offset-x"), (float)boneData.Attribute("offset-y"));
            _relativeRotation = boneData.Attribute("rotation") == null ? 0.0f : MathHelper.ToRadians((float)boneData.Attribute("rotation"));
            _relativeScale = boneData.Attribute("scale") == null ? 1.0f : (float)boneData.Attribute("scale");
            _relativeDepth = (float)boneData.Attribute("depth");
            _relativeTint = Color.White;

            if (boneData.Elements("bone") != null)
            {
                foreach (XElement bone in boneData.Elements("bone"))
                {
                    StorableBone newBone = new StorableBone();
                    newBone.ID = bone.Attribute("id").Value;
                    skeleton.AddBone(newBone, boneData.Attribute("id").Value);
                    newBone.SetMetricsAndCreateChildrenFromXml(skeleton, bone);
                }
            }
        }

        private const float Depth_Modifier_From_Primary_To_Secondary_Limb_Levels = 100.0f;
        private const float Depth_Modifier_From_Secondary_To_Following_Limb_Levels = 10.0f;
    }
}
