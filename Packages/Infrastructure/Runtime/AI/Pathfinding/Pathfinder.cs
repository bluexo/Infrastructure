using System.Collections.Generic;
using System.Linq;
using Priority_Queue;
using System;

namespace Origine.Pathfinding
{
    public static class Pathfinder
    {
        private static Grid grid;

        private static FastPriorityQueue<Tile> openListPriorityQueue;
        private static Tile[] neighbors = new Tile[4];
        private static List<Tile> finalPath;
        private static Dictionary<int, Tile> closeDictionary;

        public static void Initialize(Grid targetGrid)
        {
            grid = targetGrid;
            openListPriorityQueue = new FastPriorityQueue<Tile>(grid.GridSizeX * grid.GridSizeY);
            finalPath = new List<Tile>((int)Math.Round(grid.Tiles.Length * 0.1f));
            closeDictionary = new Dictionary<int, Tile>((int)Math.Round(grid.Tiles.Length * 0.1f));
        }

        public static List<Tile> GetPath(int fromX, int fromY, int toX, int toY)
        {
            finalPath.Clear();

            int fromIndex = grid.TilePosToIndex(fromX, fromY);
            int toIndex = grid.TilePosToIndex(toX, toY);

            Tile initialTile = grid.Tiles[fromIndex];
            Tile destinationTile = grid.Tiles[toIndex];

            openListPriorityQueue.Enqueue(initialTile, 0);

            Tile currentTile = null;

            while (openListPriorityQueue.Count > 0)
            {
                currentTile = openListPriorityQueue.Dequeue();

                closeDictionary.Add(currentTile.Index, currentTile);

                if (Equals(currentTile, destinationTile))
                    break;

                UpdateNeighbors(currentTile);

                for (int i = neighbors.Length - 1; i >= 0; --i)
                {
                    Tile neighbourPathTile = neighbors[i];
                    if (neighbourPathTile == null)
                        continue;

                    if (closeDictionary.ContainsKey(neighbourPathTile.Index))
                        continue;

                    bool isAtOpenList = openListPriorityQueue.Contains(neighbourPathTile);

                    float movementCostToNeighbour = currentTile.GCost + GetDistance(currentTile, neighbourPathTile);
                    if (movementCostToNeighbour < neighbourPathTile.GCost || !isAtOpenList)
                    {
                        neighbourPathTile.GCost = movementCostToNeighbour;
                        neighbourPathTile.HCost = GetDistance(neighbourPathTile, destinationTile);
                        neighbourPathTile.SetParent(currentTile);

                        if (!isAtOpenList)
                        {
                            openListPriorityQueue.Enqueue(neighbourPathTile, neighbourPathTile.FCost);
                        }
                        else
                        {
                            openListPriorityQueue.UpdatePriority(neighbourPathTile, neighbourPathTile.FCost);
                        }
                    }
                }
            }

            while (currentTile.Parent != null && !Equals(currentTile, initialTile))
            {
                finalPath.Add(currentTile);
                currentTile = currentTile.Parent;
            }

            finalPath.Add(initialTile);

            openListPriorityQueue.Clear();
            closeDictionary.Clear();
            return finalPath;
        }

        private static float GetDistance(Tile targetFromTile, Tile targetToTile)
        {
            int fromPositionX = targetFromTile.PositionX;
            int toPositionX = targetToTile.PositionX;
            int fromPositionY = targetFromTile.PositionY;
            int toPositionY = targetToTile.PositionY;
            return (fromPositionX - toPositionX) *
                   (fromPositionX - toPositionX) +
                   (fromPositionY - toPositionY) *
                   (fromPositionY - toPositionY);
        }

        private static void UpdateNeighbors(Tile targetTile)
        {
            neighbors[0] = GetNeighborAtDirection(targetTile, NeighborDirection.LEFT);
            neighbors[1] = GetNeighborAtDirection(targetTile, NeighborDirection.TOP);
            neighbors[2] = GetNeighborAtDirection(targetTile, NeighborDirection.RIGHT);
            neighbors[3] = GetNeighborAtDirection(targetTile, NeighborDirection.DOWN);
        }

        private static Tile GetNeighborAtDirection(Tile targetTile, NeighborDirection targetDirection)
        {
            int positionX;
            int positionY;

            GetNeighbourPosition(targetTile, targetDirection, out positionX, out positionY);
            if (!grid.IsValidTilePosition(positionX, positionY))
                return null;

            int neighborIndex = grid.TilePosToIndex(positionX, positionY);

            Tile neighborAtDirection = grid.Tiles[neighborIndex];
            return neighborAtDirection;
        }

        private static void GetNeighbourPosition(Tile targetTile, NeighborDirection targetDirection, out int targetPositionX, out int targetPositionY)
        {
            targetPositionX = targetTile.PositionX;
            targetPositionY = targetTile.PositionY;
            switch (targetDirection)
            {
                case NeighborDirection.LEFT:
                    targetPositionX -= 1;
                    break;
                case NeighborDirection.TOP:
                    targetPositionY += 1;
                    break;
                case NeighborDirection.RIGHT:
                    targetPositionX += 1;
                    break;
                case NeighborDirection.DOWN:
                    targetPositionY -= 1;
                    break;
            }
        }
    }
}
