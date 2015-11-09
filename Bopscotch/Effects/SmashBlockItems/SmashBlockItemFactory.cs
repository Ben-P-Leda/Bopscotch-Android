using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Leda.Core.Gamestate_Management;
using Leda.Core.Asset_Management;
using Leda.Core.Timing;

using Bopscotch.Gameplay.Objects.Environment.Blocks;

namespace Bopscotch.Effects.SmashBlockItems
{
    public sealed class SmashBlockItemFactory
    {
        private Scene.ObjectRegistrationHandler _registerObject;
        private TimerController.TickCallbackRegistrationHandler _registerTimerTick;

        public SmashBlockItemFactory(Scene.ObjectRegistrationHandler objectRegistrationHandler,
            TimerController.TickCallbackRegistrationHandler timerTickRegistrationHandler)
        {
            _registerObject = objectRegistrationHandler;
            _registerTimerTick = timerTickRegistrationHandler;
        }

        public void CreateItemsForSmashBlock(SmashBlock smashedBlock)
        {
            for (int i = 0; i < smashedBlock.Contents.Count; i++)
            {
                CreateItemsForSingleTexture(
                    smashedBlock.Contents[i],
                    smashedBlock.WorldPosition + (new Vector2(Definitions.Grid_Cell_Pixel_Size) / 2.0f),
                    smashedBlock.CameraPosition);
            }
        }

        private void CreateItemsForSingleTexture(Data.SmashBlockItemData itemData, Vector2 WorldPosition, Vector2 CameraPosition)
        {
            if (_registerObject != null)
            {
                for (int i = 0; i < itemData.Count; i++)
                {
                    SmashBlockItem newItem = null;

                    foreach (string s in Glowing_Item_Texture_Prefixes.Split(','))
                    {
                        if (itemData.TextureName.StartsWith(s)) { newItem = new SmashBlockGlowingItem(); break; }
                    }
                    if (newItem == null) { newItem = new SmashBlockItem();}

                    newItem.TimerTickCallback = _registerTimerTick;
                    newItem.Texture = TextureManager.Textures[itemData.TextureName];
                    newItem.Frame = TextureManager.Textures[itemData.TextureName].Bounds;
                    newItem.WorldPosition = WorldPosition;
                    newItem.CameraPosition = CameraPosition;

                    _registerObject(newItem);
                }
            }
        }

        private const string Glowing_Item_Texture_Prefixes = "power,golden";
    }
}
