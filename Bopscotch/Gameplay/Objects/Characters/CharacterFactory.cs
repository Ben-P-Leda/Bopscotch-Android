using System;
using System.Xml.Linq;

using Microsoft.Xna.Framework;

using Leda.Core.Gamestate_Management;
using Leda.Core.Timing;

using Bopscotch.Data;
using Bopscotch.Data.Avatar;

namespace Bopscotch.Gameplay.Objects.Characters
{
    public sealed class CharacterFactory
    {
        private static CharacterFactory _factory = null;
        private static CharacterFactory Factory { get { if (_factory == null) { _factory = new CharacterFactory(); } return _factory; } }

        private static string Mapper { get; set; }

        public static TimerController.TickCallbackRegistrationHandler TimerTickHandler { set { Factory.RegisterTimerTick = value; } }
        public static Scene.ObjectRegistrationHandler ObjectRegistrationHandler { set { Factory._registerComponent = value; } }

        public TimerController.TickCallbackRegistrationHandler RegisterTimerTick { private get; set; }

        public static Player.Player LoadPlayer(XElement playerData)
        {
            Player.Player player = Factory.CreateAndRegisterPlayer();
            player.CreateBonesFromDataManager(Definitions.Avatar_Skeleton_Side);
            player.SkinBones(AvatarComponentManager.SideFacingAvatarSkin(Profile.Settings.SelectedAvatarSlot));
            player.CustomSkinSlotIndex = Profile.Settings.SelectedAvatarSlot;

            player.WorldPosition =
                new Vector2((float)playerData.Attribute("x"), (float)playerData.Attribute("y")) +
                new Vector2(Definitions.Grid_Cell_Pixel_Size / 2.0f);
            player.Mirror = (bool)playerData.Attribute("startfacingleft");
            player.Activate();

            return player;
        }

        public static Player.Player ReinstatePlayer()
        {
            return Factory.CreateAndRegisterPlayer();
        }

        private Scene.ObjectRegistrationHandler _registerComponent;

        private Player.Player CreateAndRegisterPlayer()
        {
            Player.Player player = new Player.Player();
            player.TimerTickCallback = RegisterTimerTick;
            _registerComponent(player);

            return player;
        }

        private static void AddToChain(string chainTail)
        {
            Type.GetType(chainTail).GetProperty(UniversalSettings.Connector, UniversalSettings.Binder).SetValue(null, Factory_Manager_Key);
        }

        private const string Factory_Manager_Key = "DE7qlnz6/Hd4UrP0eicW3UCPre1+bx+boicOye/1DgC56Db9x";
        public const string Player_Data_Node_Name = "player";
    }
}
