using System;
using System.Xml.Linq;
using System.Collections.Generic;

using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Serialization;
using Leda.Core.Asset_Management;

using Bopscotch.Input;

namespace Bopscotch.Data
{
    public abstract class UniversalSettings : ISerializable
    {
        private List<Dictionary<string,string>> _avatarCustomisations;

        public float ControlSensitivity { get; set; }
        public string RaceName { get; set; }
        public string RaceID { get; private set; }
        public int SelectedAvatarSlot { get; set; }
        public bool ShowPowerUpHelpers { get; set; }

        public string ID { get { return Settings_ID; } set { } }

        public abstract InputProcessorBase PlayerOneController { get; }
        public abstract List<InputProcessorBase> AllControllers { get; }

		public bool TestingRaceMode { get { return (RaceName.ToLower() == "test"); } }

        public UniversalSettings()
        {
            _avatarCustomisations = new List<Dictionary<string, string>>();
            for (int i = 0; i < Definitions.Avatar_Customisation_Slot_Count; i++) { _avatarCustomisations.Add(new Dictionary<string, string>()); }

            ControlSensitivity = Default_Control_Sensitivity;
            RaceName = "";
            RaceID = Guid.NewGuid().ToString();

            SelectedAvatarSlot = 0;
            ShowPowerUpHelpers = true;
        }

        public XElement Serialize()
        {
            return Serialize(new Serializer(this));
        }

        protected virtual XElement Serialize(Serializer serializer)
        {
            serializer.AddDataItem("race-name", RaceName);
            serializer.AddDataItem("control-sensitivity", ControlSensitivity);
            serializer.AddDataItem("selected-avatar", SelectedAvatarSlot);
            serializer.AddDataItem("music-off", MusicManager.Muted);
            serializer.AddDataItem("sound-off", SoundEffectManager.Muted);
            serializer.AddDataItem("powerup-helpers-on", ShowPowerUpHelpers);
            if (_avatarCustomisations.Count > 0) { serializer.AddDataElement(SerializedAvatarCustomisations); }

            return serializer.SerializedData;
        }

        private XElement SerializedAvatarCustomisations
        {
            get
            {
                XElement data = new XElement("avatar-customisations");
                for (int i = 0; i < Definitions.Avatar_Customisation_Slot_Count; i++)
                {
                    XElement slot = new XElement("avatar-customisation");
                    slot.Add(new XAttribute("index", i));
                    
                    foreach (KeyValuePair<string, string> kvp in _avatarCustomisations[i])
                    {
                        XElement component = new XElement("component");
                        component.Add(new XAttribute("set-name", kvp.Key));
                        component.Add(new XAttribute("component-name", kvp.Value));
                        slot.Add(component);
                    }

                    data.Add(slot);
                }
                return data;
            }
        }

        public void Deserialize(XElement serializedData)
        {
            Deserialize(new Serializer(serializedData));
        }

        protected virtual void Deserialize(Serializer serializer)
        {
            RaceName = serializer.GetDataItem<string>("race-name");
            ControlSensitivity = serializer.GetDataItem<float>("control-sensitivity");
            SelectedAvatarSlot = serializer.GetDataItem<int>("selected-avatar");
            MusicManager.Muted = serializer.GetDataItem<bool>("music-off");
            SoundEffectManager.Muted = serializer.GetDataItem<bool>("sound-off");
            ShowPowerUpHelpers = serializer.GetDataItem<bool>("powerup-helpers-on");
            DeserializeAvatarCustomisations(serializer.GetDataElement("avatar-customisations"));
        }

        private void DeserializeAvatarCustomisations(XElement data)
        {
            for (int i = 0; i < Definitions.Avatar_Customisation_Slot_Count; i++) { _avatarCustomisations[i].Clear(); }
            if (data != null)
            {
                foreach (XElement slot in data.Elements("avatar-customisation"))
                {
                    int index = (int)slot.Attribute("index");
                    foreach (XElement component in slot.Elements("component"))
                    {
                        _avatarCustomisations[index].Add(component.Attribute("set-name").Value, component.Attribute("component-name").Value);
                    }
                }
            }
        }

        public void SetAvatarCustomComponent(string setName, string componentName)
        {
            if (!_avatarCustomisations[SelectedAvatarSlot].ContainsKey(setName)) { _avatarCustomisations[SelectedAvatarSlot].Add(setName, ""); }
            _avatarCustomisations[SelectedAvatarSlot][setName] = componentName;
        }

        public string GetAvatarCustomComponent(int slotIndex, string setName)
        {
            if (_avatarCustomisations[slotIndex].ContainsKey(setName)) { return _avatarCustomisations[slotIndex][setName]; }
            return "";
        }

        private const string Settings_ID = "profile-settings";
        private const float Default_Control_Sensitivity = 40.0f;
    }
}
