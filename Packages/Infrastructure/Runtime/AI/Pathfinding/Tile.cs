using Priority_Queue;

namespace Origine.Pathfinding
{
    public sealed class Tile : FastPriorityQueueNode
    {
        public Tile Parent { get; private set; }
        public int PositionX { get; private set; }
        public int PositionY { get; private set; }

        public float GCost { get; set; }
        public float HCost { get; set; }

        public float FCost => GCost + HCost;

        public TileType TileType { get; private set; }
        public int Index { get; }

        public Tile(int targetTileIndex, int targetPositionX, int targetPositionY)
        {
            Index = targetTileIndex;
            SetTilePostion(targetPositionX, targetPositionY);
        }

        public void SetParent(Tile targetTile) => Parent = targetTile;

        private void SetTilePostion(int targetPositionX, int targetPOsitionY)
        {
            PositionX = targetPositionX;
            PositionY = targetPOsitionY;
        }

        public void SetType(TileType targetType)
        {
            if (TileType == targetType)
                return;

            TileType = targetType;
        }

        public override bool Equals(object obj)
        {
            Tile otherTile = obj as Tile;
            if (otherTile == null)
                return false;

            return Index == otherTile.Index;
        }

        public override int GetHashCode() => base.GetHashCode();
    }
}
