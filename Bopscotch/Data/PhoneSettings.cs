using System;
using System.Xml.Linq;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Serialization;

using Bopscotch.Input;

namespace Bopscotch.Data
{
    public sealed class PhoneSettings : UniversalSettings
    {
        private PlayerController _playerOneControls;
     
        public PhoneSettings()
            : base()
        {
            _identity = "";
            _playerOneControls = PlayerController.ScreenSections;
        }

        protected override XElement Serialize(Serializer serializer)
        {
            base.Serialize(serializer);

            serializer.AddDataItem("player-1-controls", _playerOneControls);

            return serializer.SerializedData;
        }

        protected override void Deserialize(Serializer serializer)
        {
            base.Deserialize(serializer);

            _playerOneControls = serializer.GetDataItem<PlayerController>("player-1-controls");
        }

        private InputProcessorBase CreatePlayerController(PlayerController method)
        {
            switch (method)
            {
                case PlayerController.ScreenSections: return ScreenSectionControls.CreateController(); break;
                case PlayerController.DragAndTap: return DragTapControls.CreateController(); break;
            }

            return null;
        }

        public override InputProcessorBase PlayerOneController { get { return CreatePlayerController(_playerOneControls); } }

        public override List<InputProcessorBase> AllControllers { get { return new List<InputProcessorBase>() { PlayerOneController }; } }

        private enum PlayerController
        {
            ScreenSections,
            DragAndTap
        }

        private string _identity;
        public string Identity { get { return _identity; } set { _identity = value; } }
    }
}
