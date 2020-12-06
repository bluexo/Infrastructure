using System;

namespace Origine.Pathfinding
{
    public enum TileType : byte
    {
        EMPTY,
        ROAD,
        BLOCK
    }

    public enum NeighborDirection
    {
        LEFT = 0,
        TOP = 1,
        RIGHT = 2,
        DOWN = 3
    }

    public sealed class Grid
    {
        public int GridSizeX { get; private set; } = 100;
        public int GridSizeY { get; private set; } = 100;

        public Tile[] Tiles { get; private set; }

        public int TilePosToIndex(int x, int y) => x + y * GridSizeX;

        public void IndexToTilePos(int index, out int x, out int y)
        {
            x = index % GridSizeX;
            y = (int)Math.Floor(index / (float)GridSizeX);
        }

        public void SetTileType(int index, TileType type) => Tiles[index].SetType(type);

        public void SetTileType(int x, int y, TileType type) => SetTileType(TilePosToIndex(x, y), type);

        public TileType GetTileType(int index) => Tiles[index].TileType;

        public TileType GetTileType(int x, int y) => GetTileType(TilePosToIndex(x, y));

        public void SetTileBlocked(int index, bool blocked) => SetTileType(index, blocked ? TileType.BLOCK : TileType.EMPTY);

        public void SetTileBlocked(int x, int y, bool blocked) => SetTileBlocked(TilePosToIndex(x, y), blocked);

        public bool IsTileBlocked(int index) => Tiles[index].TileType == TileType.BLOCK;

        public bool IsTileBlocked(int x, int y) => IsTileBlocked(TilePosToIndex(x, y));

        public bool IsValidTilePosition(int targetPositionX, int targetPositionY)
        {
            if (targetPositionX < 0 || targetPositionX > GridSizeX - 1)
                return false;

            if (targetPositionY < 0 || targetPositionY > GridSizeY - 1)
                return false;

            int tilePosToIndex = TilePosToIndex(targetPositionX, targetPositionY);

            if (Tiles[tilePosToIndex].TileType == TileType.BLOCK)
                return false;

            return true;
        }

        public void GenerateTiles()
        {
            Tiles = new Tile[GridSizeX * GridSizeY];
            for (int i = Tiles.Length - 1; i >= 0; i--)
            {
                IndexToTilePos(i, out int positionX, out int positionY);
                Tiles[i] = new Tile(i, positionX, positionY);
            }
        }

        public void GenerateTiles(int gridX, int gridY)
        {
            GridSizeX = gridX;
            GridSizeY = gridY;

            GenerateTiles();
        }

        public void Clear()
        {
            for (int i = Tiles.Length - 1; i >= 0; i--)
                SetTileType(i, TileType.EMPTY);
        }

    }
}
