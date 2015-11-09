using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Leda.Core.Game_Objects.Tile_Map;
using System.Xml.Linq;

using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Serialization;

using Bopscotch.Gameplay.Objects.Environment.Blocks;

namespace Bopscotch.Gameplay.Objects.Environment
{
    public class BlockMap : TileMap, ISerializable
    {
        private List<Vector2> _smashedBlockWorldPositions;

        public string ID { get { return "block-map"; } set { } }

        public BlockMap(int mapWidth, int mapHeight, int tileWidth, int tileHeight, int renderLayer)
            : base(mapWidth, mapHeight, tileWidth, tileHeight, renderLayer)
        {
            _smashedBlockWorldPositions = new List<Vector2>();
        }

        public void RecordBlockHasBeenSmashed(SmashBlock smashedBlock)
        {
            if (!_smashedBlockWorldPositions.Contains(smashedBlock.WorldPosition)) { _smashedBlockWorldPositions.Add(smashedBlock.WorldPosition); }
        }

        public XElement Serialize()
        {
            Serializer serializer = new Serializer(this);

            serializer.AddDataItem("smashed-block-count", _smashedBlockWorldPositions.Count);
            for (int i = 0; i < _smashedBlockWorldPositions.Count; i++)
            {
                serializer.AddDataItem(string.Concat("smashed-block-", i), _smashedBlockWorldPositions[i]);
            }

            return serializer.SerializedData;
        }

        public void Deserialize(XElement serializedData)
        {
            Serializer serializer = new Serializer(serializedData);

            _smashedBlockWorldPositions.Clear();
            for (int i = 0; i < serializer.GetDataItem<int>("smashed-block-count"); i++)
            {
                Vector2 blockWorldPosition = serializer.GetDataItem<Vector2>(string.Concat("smashed-block-",i));
                Point blockMapPosition = new Point(
                    (int)(blockWorldPosition.X / Definitions.Grid_Cell_Pixel_Size), (int)(blockWorldPosition.Y / Definitions.Grid_Cell_Pixel_Size));

                _smashedBlockWorldPositions.Add(blockWorldPosition);

                SurvivalModeItemSmashBlock block = GetTile(blockMapPosition.X, blockMapPosition.Y) as SurvivalModeItemSmashBlock;
                if (block != null)
                {
                    block.ReadyForDisposal = true;
                    block.Visible = false;
                    block.Collidable = false;
                }
            }
        }
    }
}
