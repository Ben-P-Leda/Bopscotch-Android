using System;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Leda.Core.Serialization;
using Leda.Core.Timing;
using Leda.Core.Game_Objects.Behaviours;

namespace Leda.Core.Gamestate_Management
{
    public abstract class StorableScene : Scene, ISerializable
    {
        private List<ISerializable> _objectsToSerialize;
        private List<IObjectCreator> _dynamicObjectCreators;
        protected Type _nextSceneType;

        protected List<ISerializable> ObjectsToSerialize { get { return _objectsToSerialize; } }
        protected bool RecoveredFromTombstone { get; private set; }

        protected override Type NextSceneType { set { _nextSceneType = value;  base.NextSceneType = value; } }

        public string ID { get; set; }
        public XElement TombstoneRecoveryData { get; set; }

        public StorableScene(string id)
            : base()
        {
            ID = id;
            TombstoneRecoveryData = null;

            _objectsToSerialize = new List<ISerializable>();
            _dynamicObjectCreators = new List<IObjectCreator>();
        }

        protected override void RegisterGameObject(IGameObject toRegister)
        {
            base.RegisterGameObject(toRegister);

            if ((toRegister is ISerializable) && (!_objectsToSerialize.Contains((ISerializable)toRegister))) { _objectsToSerialize.Add((ISerializable)toRegister); }
            if ((toRegister is IObjectCreator) && (!_dynamicObjectCreators.Contains((IObjectCreator)toRegister))) { _dynamicObjectCreators.Add((IObjectCreator)toRegister); }
        }

        protected override void UnregisterGameObject(IGameObject toUnregister)
        {
            base.UnregisterGameObject(toUnregister);

            if ((toUnregister is ISerializable) && (_objectsToSerialize.Contains((ISerializable)toUnregister))) { _objectsToSerialize.Remove((ISerializable)toUnregister); }
            if ((toUnregister is IObjectCreator) && (_dynamicObjectCreators.Contains((IObjectCreator)toUnregister))) { _dynamicObjectCreators.Remove((IObjectCreator)toUnregister); }
        }

        public XElement Serialize()
        {
            return Serialize(new Serializer(this));
        }

        protected virtual XElement Serialize(Serializer serializer)
        {
            serializer.AddDataItem("state", CurrentState);
            if (_nextSceneType != null) { serializer.AddDataItem("nextscene", _nextSceneType.AssemblyQualifiedName); }

            serializer.AddDataForStaticListOfSerializables("objects", _objectsToSerialize);

            return serializer.SerializedData;
        }

        private void ReinstateDynamicObjects()
        {
            if (_dynamicObjectCreators.Count > 0)
            {
                List<XElement> dataItems = (from el
                                            in TombstoneRecoveryData.Elements("dataitem")
                                            where (string)el.Attribute("name") == "objects"
                                            select el).ToList();

                if (dataItems.Count > 0)
                {
                    for (int i = 0; i < _dynamicObjectCreators.Count; i++) { _dynamicObjectCreators[i].ReinstateDynamicObjects(dataItems[0]); }
                }
            }
        }

        public void Deserialize(XElement serializedData)
        {
            Deserialize(new Serializer(serializedData));
        }

        protected virtual Serializer Deserialize(Serializer serializer)
        {
            try
            {
                Status lastSceneState = serializer.GetDataItem<Status>("state");
                string nextSceneTypeName = serializer.GetDataItem<string>("nextscene");

                if (!string.IsNullOrEmpty(nextSceneTypeName)) { NextSceneType = Type.GetType(nextSceneTypeName); }

                serializer.GetDataForStaticListOfSerializables("objects", _objectsToSerialize);

                HandlePostDeserializationResaturation();              
            }
            catch (Exception ex)
            {
            }

            return serializer;
        }

        protected override void CompleteDeactivation()
        {
            _nextSceneType = null;

            base.CompleteDeactivation();
        }

        protected virtual void HandlePostDeserializationResaturation()
        {
            for (int i = 0; i < _objectsToSerialize.Count; i++)
            {
                if (_objectsToSerialize[i] is ISerializableWithPostDeserialize) { ((ISerializableWithPostDeserialize)_objectsToSerialize[i]).HandlePostDeserialize(); }
            }
        }

        public override void Activate()
        {
            base.Activate();

            RecoveredFromTombstone = (TombstoneRecoveryData != null);
            if (RecoveredFromTombstone)
            {
                ReinstateDynamicObjects();
                Deserialize(TombstoneRecoveryData);
                TombstoneRecoveryData = null;
            }
        }
    }
}
