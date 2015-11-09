using System;
using System.Xml.Linq;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Game_Objects.Tile_Map;
using Leda.Core.Game_Objects.Controllers;
using Leda.Core.Gamestate_Management;
using Leda.Core.Asset_Management;
using Leda.Core.Timing;

using Bopscotch.Effects.Particles;

namespace Bopscotch.Gameplay.Objects.Environment.Blocks
{
    public sealed class BlockFactory
    {
        private static BlockFactory _factory = null;
        private static BlockFactory Factory { get { if (_factory == null) { _factory = new BlockFactory(); } return _factory; } }

        public static TimerController.TickCallbackRegistrationHandler TimerTickHandler { set { Factory._registerTimerTick = value; } }
        public static AnimationController AnimationController { set { Factory._animationController = value; } }
        public static SmashBlock.SmashCallbackMethod SmashBlockCallback { set { Factory._smashBlockCallback = value; } }
        public static AdditiveLayerParticleEffectManager.CloudBurstEffectInitiator SmashBlockRegerationCallback { set { Factory._smashBlockRegenerationCallback = value; } }

        public static BlockMap CreateLevelBlockMap(XElement blockDataGroup) 
        {
            return Factory.GenerateBlockMap(blockDataGroup); 
        }

        public static BlockMap ReinstateLevelBlockMap(XElement blockDataGroup)
        {
            return Factory.GenerateBlockMap(blockDataGroup);
        }

        private TimerController.TickCallbackRegistrationHandler _registerTimerTick;
        private SmashBlock.SmashCallbackMethod _smashBlockCallback;
        private AdditiveLayerParticleEffectManager.CloudBurstEffectInitiator _smashBlockRegenerationCallback;
        private AnimationController _animationController;

        private void Reset()
        {
        }

        private BlockMap GenerateBlockMap(XElement blockDataGroup)
        {
            List<Block> blocks = new List<Block>();
            Point worldDimensions = Point.Zero;

            Reset();

            foreach (XElement node in blockDataGroup.Elements())
            {
                Block toAdd = Factory.CreateBlockFromXmlNode(node);

                blocks.Add(toAdd);

                worldDimensions.X = Math.Max(worldDimensions.X, (int)toAdd.WorldPosition.X + Definitions.Grid_Cell_Pixel_Size);
                worldDimensions.Y = Math.Max(worldDimensions.Y, (int)toAdd.WorldPosition.Y + Definitions.Grid_Cell_Pixel_Size);
            }

            return CreateBlockMap(worldDimensions, blocks);
        }

        private Block CreateBlockFromXmlNode(XElement node)
        {
            Block newBlock = null;

            switch (node.Name.ToString())
            {
                case SmashBlock.Data_Node_Name: newBlock = CreateSmashBlock(node); break;
                case SpringBlock.Data_Node_Name: newBlock = CreateSpringBlock(node); break;
                case SpikeBlock.Data_Node_Name: newBlock = CreateSpikeBlock(node); break;
                case ObstructionBlock.Data_Node_Name: newBlock = CreateObstructionBlock(node); break;
                case IceBlock.Data_Node_Name: newBlock = CreateIceBlock(node); break;
                case Block.Data_Node_Name: newBlock = CreateBlock(node); break;
            }

            if (newBlock != null) 
            { 
                if (newBlock is IAnimated) { _animationController.AddAnimatedObject((IAnimated)newBlock); }
            }

            return newBlock;
        }

        private Block CreateBlock(XElement node)
        {
            Block newBlock = new Block();
            newBlock.WorldPosition = new Vector2((float)node.Attribute("x"), (float)node.Attribute("y"));
            newBlock.Texture = TextureManager.Textures[node.Attribute("texture").Value];

            return newBlock;
        }

        private ObstructionBlock CreateObstructionBlock(XElement node)
        {
            ObstructionBlock newBlock = new ObstructionBlock();
            newBlock.WorldPosition = new Vector2((float)node.Attribute("x"), (float)node.Attribute("y"));
            newBlock.Texture = TextureManager.Textures[node.Attribute("texture").Value];

            return newBlock;
        }

        private IceBlock CreateIceBlock(XElement node)
        {
            IceBlock newBlock = new IceBlock();
            newBlock.WorldPosition = new Vector2((float)node.Attribute("x"), (float)node.Attribute("y"));
            newBlock.Texture = TextureManager.Textures[node.Attribute("texture").Value];

            return newBlock;
        }

        private SpringBlock CreateSpringBlock(XElement node)
        {
            SpringBlock newSpringBlock = new SpringBlock();
            newSpringBlock.WorldPosition = new Vector2((float)node.Attribute("x"), (float)node.Attribute("y"));

            return newSpringBlock;
        }

        private SmashBlock CreateSmashBlock(XElement node)
        {
            SmashBlock newSmashBlock;

            if (Data.Profile.PlayingRaceMode)
            {
                newSmashBlock = new RaceModePowerUpSmashBlock();
                ((RaceModePowerUpSmashBlock)newSmashBlock).TickCallback = _registerTimerTick;
                ((RaceModePowerUpSmashBlock)newSmashBlock).RegenerationParticleEffect = _smashBlockRegenerationCallback;
            }
            else
            {
                newSmashBlock = new SurvivalModeItemSmashBlock();
            }

            newSmashBlock.WorldPosition = new Vector2((float)node.Attribute("x"), (float)node.Attribute("y"));
            newSmashBlock.SmashCallback = _smashBlockCallback;

            foreach (XElement el in node.Elements("contains-item")) 
            {
                Data.SmashBlockItemData item = CreateSmashBlockItem(el, newSmashBlock.WorldPosition);
                if (item != null) { newSmashBlock.Contents.Add(item); }
            }

            return newSmashBlock;
        }

        private Data.SmashBlockItemData CreateSmashBlockItem(XElement node, Vector2 smashBlockWorldPosition)
        {
            if ((node.Attribute("action").Value == "add-ticket") && (Data.Profile.CurrentAreaData.GoldenTicketHasBeenCollectedFromCrate(smashBlockWorldPosition)))
            { 
                return null; 
            }

            Data.SmashBlockItemData itemData = new Data.SmashBlockItemData();
            itemData.TextureName = node.Attribute("texture").Value;
            itemData.Count = (int)node.Attribute("units");

            switch (node.Attribute("action").Value)
            {
                case "add-ticket": itemData.AffectsItem = Data.SmashBlockItemData.AffectedItem.GoldenTicket; break;
                case "score": itemData.AffectsItem = Data.SmashBlockItemData.AffectedItem.Score; break;
            }

            itemData.Value = (int)node.Attribute("value");

            return itemData;
        }

        private SpikeBlock CreateSpikeBlock(XElement node)
        {
            SpikeBlock newSpikeBlock = new SpikeBlock();
            newSpikeBlock.WorldPosition = new Vector2((float)node.Attribute("x"), (float)node.Attribute("y")) + 
                (new Vector2(Definitions.Grid_Cell_Pixel_Size) / 2.0f);

            return newSpikeBlock;
        }

        private BlockMap CreateBlockMap(Point worldDimensions, List<Block> tiles)
        {
            BlockMap map = new BlockMap(
                worldDimensions.X / Definitions.Grid_Cell_Pixel_Size,
                worldDimensions.Y / Definitions.Grid_Cell_Pixel_Size,
                Definitions.Grid_Cell_Pixel_Size,
                Definitions.Grid_Cell_Pixel_Size,
                Map_Render_Layer);

            for (int i = 0; i < tiles.Count; i++)
            {
                map.SetTile(
                    (int)(tiles[i].WorldPosition.X / Definitions.Grid_Cell_Pixel_Size), 
                    (int)(tiles[i].WorldPosition.Y / Definitions.Grid_Cell_Pixel_Size), 
                    tiles[i]);
            }

            return map;
        }

        public const string Data_Group_Node_Name = "blocks";

        private const int Map_Render_Layer = 2;
    }
}
