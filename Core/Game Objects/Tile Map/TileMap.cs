using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core.Game_Objects.Behaviours;

namespace Leda.Core.Game_Objects.Tile_Map
{
    public class TileMap : ICameraLinked, ISimpleRenderable
    {
        private ITile[] _tileObjects;
        private bool[] _cellIsOccupied;
        private int _mapWidth;
        private int _mapHeight;
        private int _tileCount;
        private Point _tileDimensions;
        private Vector2 _cameraPosition;
        private Point _viewportDimensionsInTiles;
        private int _renderLayer;

        public Point MapDimensions { get { return new Point(_mapWidth, _mapHeight); } }
        public int Width { get { return _mapWidth; } }
        public int Height { get { return _mapHeight; } }
        public Point TileDimensions { get { return _tileDimensions; } }
        public Point ViewportDimensionsInTiles { set { _viewportDimensionsInTiles = value; } }
        public Point MapWorldDimensions { get { return new Point(_mapWidth * _tileDimensions.X, _mapHeight * _tileDimensions.Y); } }
        public Vector2 CameraPosition { set { _cameraPosition = value; } }

        public bool Visible { get; set; }
        public int RenderLayer { get { return _renderLayer; } set { _renderLayer = value; SetRenderLayerForTiles(); } }

        public TileMap(int mapWidth, int mapHeight, int tileWidth, int tileHeight, int renderLayer) 
            : this()
        {
            InitializeMap(new Point(mapWidth, mapHeight), new Point(tileWidth, tileHeight));
            RenderLayer = renderLayer;
        }

        public TileMap()
        {
            _tileDimensions = Point.Zero;
            _mapWidth = 0;
            _mapHeight = 0;
            _tileCount = 0;

            Visible = true;
            _renderLayer = -1;
        }

        public void SetRenderLayerForTiles()
        {
            for (int i = 0; i < _tileCount; i++)
            {
                if (_cellIsOccupied[i]) { _tileObjects[i].RenderLayer = _renderLayer; }
            }
        }

        public void Initialize()
        {
        }

        public virtual void Reset()
        {
        }

        public void InitializeMap(Point mapDimensions, Point tileDimensions)
        {
            _tileDimensions = tileDimensions;
            _mapWidth = mapDimensions.X;
            _mapHeight = mapDimensions.Y;
            _tileCount = _mapWidth * _mapHeight;

            _tileObjects = new ITile[_tileCount];
            _cellIsOccupied = new bool[_tileCount];
            for (int i = 0; i < _tileCount; i++) { _cellIsOccupied[i] = false; }
        }

        public void SetTile(int x, int y, ITile tileObject)
        {
            if (_tileObjects == null) { throw new Exception("Map must be initialized before adding tiles. Call InitializeMap first"); }
            if (!BlockLocationIsWithinBounds(x, y)) { throw new Exception("Block coordinates are outside the boundaries of the map"); }

            if (tileObject.WorldPosition == Vector2.Zero) { tileObject.WorldPosition = new Vector2(x * _tileDimensions.X, y * _tileDimensions.Y); }
            tileObject.RenderLayer = _renderLayer;

            _tileObjects[(y * _mapWidth) + x] = tileObject;
            _cellIsOccupied[(y * _mapWidth) + x] = (tileObject != null);
        }

        public bool BlockLocationIsWithinBounds(int x, int y)
        {
            return ((x > -1) && (x < _mapWidth) && (y > -1) && (y < _mapHeight));
        }

        public bool CellIsOccupied(int x, int y)
        {
            if (_tileObjects == null) { throw new Exception("Map must be initialized before adding tiles. Call InitializeMap first"); }
            if (!BlockLocationIsWithinBounds(x, y)) { return false; }

            return _cellIsOccupied[(y * _mapWidth) + x];
        }

        public ITile GetTile(int x, int y)
        {
            if (_tileObjects == null) { throw new Exception("Map must be initialized before adding tiles. Call InitializeMap first"); }
            if (!BlockLocationIsWithinBounds(x, y)) { return null; }

            return _tileObjects[(y * _mapWidth) + x];
        }

        public Type GetTileType(int x, int y)
        {
            ITile tile = GetTile(x, y);

            if (tile != null) { return tile.GetType(); }

            return null;
        }

        public void Draw(SpriteBatch spritebatch)
        {
            int left = Convert.ToInt32(Math.Floor(_cameraPosition.X / _tileDimensions.X)) - 1;
            int top = Convert.ToInt32(Math.Floor(_cameraPosition.Y / _tileDimensions.Y)) - 1;
            int right = left + _viewportDimensionsInTiles.X + 2;
            int bottom = top + _viewportDimensionsInTiles.Y + 2;

            for (int x = left; x < right; x++)
            {
                if ((x > -1) && (x < _mapWidth))
                {
                    for (int y = top; y < bottom; y++)
                    {
                        if ((y > -1) && (y < _mapHeight))
                        {
                            int cellIndex = (y * _mapWidth) + x;
                            if ((_cellIsOccupied[cellIndex]) && (_tileObjects[cellIndex].Visible))
                            {
                                _tileObjects[cellIndex].CameraPosition = _cameraPosition;
                                _tileObjects[cellIndex].Draw(spritebatch);
                            }
                        }
                    }
                }
            }
        }
    }
}
