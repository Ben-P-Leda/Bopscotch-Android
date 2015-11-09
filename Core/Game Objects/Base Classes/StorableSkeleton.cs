using System;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Serialization;
using Leda.Core.Asset_Management;
using Leda.Core.Animation.Skeletons;

namespace Leda.Core.Game_Objects.Base_Classes
{
    public class StorableSkeleton : DisposableSkeleton, ISerializableWithPostDeserialize
    {
        public string ID { get; set; }

        public override void AddBone(IBone boneToAdd, string parentBoneID)
        {
            if (!(boneToAdd is ISerializable)) { throw new Exception("Bones for StorableSkeleton must implement ISerializable"); }
            else { base.AddBone(boneToAdd, parentBoneID); }
        }

        public XElement Serialize()
        {
            return Serialize(new Serializer(this));
        }

        protected virtual XElement Serialize(Serializer serializer)
        {
            serializer.AddDataItem("world-position", WorldPosition);
            serializer.AddDataItem("rotation", Rotation);
            serializer.AddDataItem("scale", Scale);
            serializer.AddDataItem("mirror", Mirror);
            serializer.AddDataItem("tint", Tint);
            serializer.AddDataItem("render-depth", RenderDepth);
            serializer.AddDataItem("render-layer", RenderLayer);
            serializer.AddDataItem("visible", Visible);

            foreach (KeyValuePair<string, IBone> kvp in Bones) { serializer.AddDataItem(kvp.Key, kvp.Value); }

            return serializer.SerializedData;
        }

        public void Deserialize(XElement serializedData)
        {
            Deserialize(new Serializer(serializedData));
        }

        protected virtual Serializer Deserialize(Serializer serializer)
        {
            Bones.Clear();
            PrimaryBoneID = "";

            List<XElement> subObjects = (from el
                                    in serializer.SerializedData.Elements("dataitem").Elements("object")
                                    select el).ToList();

            for (int i = 0; i < subObjects.Count; i++) 
            {
                if ((Type.GetType((string)subObjects[i].Attribute("type")) != null) && 
                    (Type.GetType((string)subObjects[i].Attribute("type")).GetInterfaces().Contains(typeof(IBone)))) 
                { 
                    ReinstateBone(subObjects[i]);
                }
            }

            WorldPosition = serializer.GetDataItem<Vector2>("world-position");
            Rotation = serializer.GetDataItem<float>("rotation");
            Scale = serializer.GetDataItem<float>("scale");
            Mirror = serializer.GetDataItem<bool>("mirror");
            Tint = serializer.GetDataItem<Color>("tint");
            RenderDepth = serializer.GetDataItem<float>("render-depth");
            RenderLayer = serializer.GetDataItem<int>("render-layer");
            Visible = serializer.GetDataItem<bool>("visible");

            return serializer;
        }

        private void ReinstateBone(XElement boneData)
        {
            IBone boneToReinstate = (IBone)Activator.CreateInstance(Type.GetType(boneData.Attribute("type").Value));
            boneToReinstate.ID = boneData.Attribute("id").Value;
            boneToReinstate.Deserialize(boneData);

            if (!string.IsNullOrEmpty(boneToReinstate.TextureReference))
            {
                boneToReinstate.Texture = TextureManager.Textures[boneToReinstate.TextureReference];
            }

            string parentBoneID = (from el
                                   in boneData.Elements("dataitem")
                                   where el.Attribute("name").Value == "parent-id"
                                   select el).First().Value;

            AddBone(boneToReinstate, parentBoneID);
        }

        public void HandlePostDeserialize()
        {
            Bones[PrimaryBoneID].CalculateWorldPosition();
            Bones[PrimaryBoneID].CalculateRenderDepth();
        }
    }
}
