using System;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Animation.Skeletons;
using Leda.Core.Asset_Management;

namespace Leda.Core.Game_Objects.Base_Classes
{
    public class DisposableSkeleton : ISkeleton, ICameraRelative
    {
        private Vector2 _worldPosition;
        private float _rotation;
        private float _scale;
        private bool _mirror;
        private Color _tint;
        private float _renderDepth;
        private int _renderLayer;
        private bool _visible;
        private bool _worldPositionNeverChanges;

        public virtual Vector2 WorldPosition { get { return _worldPosition; } set { _worldPosition = value; RecalculateBonePositions(); } }
        public virtual float Rotation { get { return _rotation; } set { _rotation = Utility.RectifyAngle(value); RecalculateBonePositions(); } }
        public virtual float Scale { get { return _scale; } set { _scale = value; RecalculateBonePositions(); } }
        public virtual bool Mirror { get { return _mirror; } set { _mirror = value; RecalculateBonePositions(); } }
        public virtual Color Tint { get { return _tint; } set { _tint = value; RecalculateSkinTints(); } }
        public virtual float RenderDepth { get { return _renderDepth; } set { _renderDepth = value; RecalculateBoneRenderDepths(); } }
        public int RenderLayer { get { return _renderLayer; } set { _renderLayer = value; UpdateBoneRenderLayers(); } }
        public virtual bool Visible { get { return _visible; } set { _visible = value; UpdateBoneVisibleFlags(); } }
        public virtual Vector2 CameraPosition { set { foreach (KeyValuePair<string, IBone> kvp in Bones) { kvp.Value.CameraPosition = value; } } }
        public bool WorldPositionIsFixed { get { return _worldPositionNeverChanges; } set { _worldPositionNeverChanges = value; } }

        public Dictionary<string, IBone> Bones { get; private set; }
        public string PrimaryBoneID { protected get; set; }

        public DisposableSkeleton()
        {
            _worldPositionNeverChanges = (!(this is IMobile));

            Bones = new Dictionary<string, IBone>();

            Scale = 1.0f;
            Rotation = 0.0f;
            Mirror = false;
            RenderDepth = 0.5f;
            Tint = Color.White;

            WorldPosition = Vector2.Zero;
            CameraPosition = Vector2.Zero;
            RenderLayer = -1;
            Visible = false;

            PrimaryBoneID = "";
        }

        public virtual void Initialize()
        {
        }

        public virtual void Reset()
        {
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            Bones[PrimaryBoneID].Draw(spriteBatch);
        }

        private void RecalculateBonePositions()
        {
            if ((Bones.Count > 0) && (!string.IsNullOrEmpty(PrimaryBoneID)))
            {
                Bones[PrimaryBoneID].WorldPosition = _worldPosition;
                if (_mirror) { Bones[PrimaryBoneID].Rotation = MathHelper.TwoPi - _rotation; }
                else { Bones[PrimaryBoneID].Rotation = _rotation; }
                Bones[PrimaryBoneID].Scale = _scale;
                Bones[PrimaryBoneID].Mirror = _mirror;
                Bones[PrimaryBoneID].CalculateWorldPosition();
            }
        }

        private void RecalculateSkinTints()
        {
            if ((Bones.Count > 0) && (!string.IsNullOrEmpty(PrimaryBoneID)))
            {
                Bones[PrimaryBoneID].Tint = _tint;
            }
        }

        private void RecalculateBoneRenderDepths()
        {
            if ((Bones.Count > 0) && (!string.IsNullOrEmpty(PrimaryBoneID)))
            {
                Bones[PrimaryBoneID].RenderDepth = _renderDepth;
                Bones[PrimaryBoneID].CalculateRenderDepth();
            }
        }

        private void UpdateBoneRenderLayers()
        {
            foreach (KeyValuePair<string, IBone> kvp in Bones)
            {
                kvp.Value.RenderLayer = _renderLayer;
            }
        }

        private void UpdateBoneVisibleFlags()
        {
            foreach (KeyValuePair<string, IBone> kvp in Bones)
            {
                kvp.Value.Visible = _visible;
            }
        }

        public virtual void AddBone(IBone boneToAdd, string parentBoneID)
        {
            if (string.IsNullOrEmpty(parentBoneID)) { AttemptToAddPrimaryBone(boneToAdd); }
            else { AttemptToAddChildBone(boneToAdd, parentBoneID); }

            RecalculateBonePositions();
            RecalculateBoneRenderDepths();
            RecalculateSkinTints();
        }

        private void AttemptToAddPrimaryBone(IBone boneToAdd)
        {
            if (!string.IsNullOrEmpty(PrimaryBoneID))
            {
                throw new Exception(string.Concat("Primary bone already exists - ID: ", PrimaryBoneID));
            }
            else
            {
                SetBoneStandardValues(boneToAdd);
                Bones.Add(boneToAdd.ID, boneToAdd);
                PrimaryBoneID = boneToAdd.ID;
            }
        }

        private void AttemptToAddChildBone(IBone boneToAdd, string parentBoneID)
        {
            if (Bones.ContainsKey(boneToAdd.ID))
            {
                throw new Exception(string.Concat("Bone IDs must be unique! ID: ", boneToAdd.ID));
            }
            else if (!Bones.ContainsKey(parentBoneID))
            {
                throw new Exception(string.Concat("Parent does not exist! Child ID: ", boneToAdd.ID, " expected parent ID: ", parentBoneID));
            }
            else
            {
                SetBoneStandardValues(boneToAdd);
                boneToAdd.Parent = Bones[parentBoneID];
                Bones.Add(boneToAdd.ID, boneToAdd);
                Bones[parentBoneID].Children.Add(boneToAdd);
            }
        }

        private void SetBoneStandardValues(IBone bone)
        {
            bone.RenderLayer = RenderLayer;
        }

        public void SetSkinForBone(string boneID, string skinTextureName, Vector2 originWithinSkinFrame, Rectangle frameAreaWithinSkinTexture)
        {
            if (!Bones.ContainsKey(boneID))
            {
                throw new Exception(string.Concat("Cannot apply skin to bone that does not exist! Bone ID: ", boneID));
            }
            else
            {
                Bones[boneID].ApplySkin(skinTextureName, originWithinSkinFrame, frameAreaWithinSkinTexture);
            }
        }

        public void ClearSkin()
        {
            foreach (KeyValuePair<string, IBone> kvp in Bones) { kvp.Value.ClearSkin(); }
        }

        public void CreateBonesFromDataManager(string skeletonName)
        {
            if (SkeletonDataManager.Skeletons.ContainsKey(skeletonName))
            {
                StorableBone newBone = new StorableBone();
                newBone.ID = SkeletonDataManager.Skeletons[skeletonName].Element("bone").Attribute("id").Value;
                AddBone(newBone, "");
                newBone.SetMetricsAndCreateChildrenFromXml(this, SkeletonDataManager.Skeletons[skeletonName].Element("bone"));

                RecalculateBonePositions();
                RecalculateBoneRenderDepths();
                RecalculateSkinTints();
            }
        }

        public void CreateBonesAndSkinsFromDataManager(string skeletonName, string skinName)
        {
            CreateBonesFromDataManager(skeletonName);

            if (SkeletonDataManager.Skins.ContainsKey(skinName)) { SkinBones(SkeletonDataManager.Skins[skinName]); }
        }

        public void SkinBones(XElement skinsData)
        {
            foreach (XElement skin in skinsData.Elements("bone"))
            {
                if (Bones.ContainsKey(skin.Attribute("id").Value))
                {
                    Vector2 origin = new Vector2((float)skin.Attribute("origin-x"), (float)skin.Attribute("origin-y"));

                    Rectangle frame = new Rectangle((int)skin.Attribute("frame-x"), (int)skin.Attribute("frame-y"),
                        (int)skin.Attribute("frame-width"), (int)skin.Attribute("frame-height"));

                    Bones[skin.Attribute("id").Value].ApplySkin(skin.Attribute("texture-name").Value, origin, frame);

                    if (skin.Element("tint") != null)
                    {
                        Bones[skin.Attribute("id").Value].MasterTint = new Color(
                            (int)skin.Element("tint").Attribute("red"),
                            (int)skin.Element("tint").Attribute("green"),
                            (int)skin.Element("tint").Attribute("blue"),
                            (int)skin.Element("tint").Attribute("alpha"));
                    }
                }
            }
        }
    }
}
