using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CorridorResult
{
    public List<Vector2Int> Tiles { get; } = new List<Vector2Int>();
    public List<(RoomInstance a, RoomInstance b)> Connections { get; } = new List<(RoomInstance a, RoomInstance b)>();
}

public static class CorridorGenerator
{
    public static CorridorResult Connect(BSPNode root, List<RoomInstance> rooms, int corridorWidth)
    {
        CorridorResult result = new CorridorResult();
        int leafIndex = 0;
        BuildConnections(root, rooms, ref leafIndex, corridorWidth, result, rooms);
        return result;
    }

    private static List<RoomInstance> BuildConnections(BSPNode node, List<RoomInstance> rooms, ref int leafIndex, int corridorWidth, CorridorResult result, List<RoomInstance> allRooms)
    {
        if (node.IsLeaf)
        {
            RoomInstance room = rooms[leafIndex];
            leafIndex++;
            return new List<RoomInstance> { room };
        }

        List<RoomInstance> leftRooms = BuildConnections(node.Left, rooms, ref leafIndex, corridorWidth, result, allRooms);
        List<RoomInstance> rightRooms = BuildConnections(node.Right, rooms, ref leafIndex, corridorWidth, result, allRooms);

        (RoomInstance a, RoomInstance b) = FindClosestPair(leftRooms, rightRooms);
        ConnectRooms(a, b, corridorWidth, result, allRooms);

        List<RoomInstance> merged = new List<RoomInstance>(leftRooms.Count + rightRooms.Count);
        merged.AddRange(leftRooms);
        merged.AddRange(rightRooms);
        return merged;
    }

    private static (RoomInstance, RoomInstance) FindClosestPair(List<RoomInstance> left, List<RoomInstance> right)
    {
        RoomInstance bestA = left[0];
        RoomInstance bestB = right[0];
        float bestDistance = float.MaxValue;

        foreach (RoomInstance a in left)
        {
            foreach (RoomInstance b in right)
            {
                float distance = Vector2Int.Distance(a.Center, b.Center);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestA = a;
                    bestB = b;
                }
            }
        }
        return (bestA, bestB);
    }

    private static void ConnectRooms(RoomInstance a, RoomInstance b, int corridorWidth, CorridorResult result, List<RoomInstance> allRooms)
    {
        RectInt rectA = a.Shape.GetBounds();
        RectInt rectB = b.Shape.GetBounds();

        int minX = Mathf.Max(rectA.xMin, rectB.xMin);
        int maxX = Mathf.Min(rectA.xMax - 1, rectB.xMax - 1);

        if (minX <= maxX)
        {
            int targetX = (minX + maxX) / 2;
            int startY, endY;

            if (rectA.yMax <= rectB.yMin)
            {
                startY = rectA.yMax - 1;
                endY = rectB.yMin;
            }
            else
            {
                startY = rectA.yMin;
                endY = rectB.yMax - 1;
            }

            AddThickLine(new Vector2Int(targetX, startY), new Vector2Int(targetX, endY), corridorWidth, result.Tiles);
            result.Connections.Add((a, b));
            return;
        }

        int minY = Mathf.Max(rectA.yMin, rectB.yMin);
        int maxY = Mathf.Min(rectA.yMax - 1, rectB.yMax - 1);

        if (minY <= maxY)
        {
            int targetY = (minY + maxY) / 2;
            int startX, endX;

            if (rectA.xMax <= rectB.xMin)
            {
                startX = rectA.xMax - 1;
                endX = rectB.xMin;
            }
            else
            {
                startX = rectA.xMin;
                endX = rectB.xMax - 1;
            }

            AddThickLine(new Vector2Int(startX, targetY), new Vector2Int(endX, targetY), corridorWidth, result.Tiles);
            result.Connections.Add((a, b));
            return;
        }

        List<RectInt> otherRoomBounds = allRooms.Where(r => r != a && r != b).Select(r => r.Shape.GetBounds()).ToList();

        int startX1 = (rectB.center.x > rectA.center.x) ? rectA.xMax - 1 : rectA.xMin;
        int startY1 = (rectA.yMin + rectA.yMax - 1) / 2;
        Vector2Int start1 = new Vector2Int(startX1, startY1);

        int cornerX1 = (rectB.xMin + rectB.xMax - 1) / 2;
        int cornerY1 = startY1;
        Vector2Int corner1 = new Vector2Int(cornerX1, cornerY1);

        int endX1 = cornerX1;
        int endY1 = (cornerY1 > (rectB.yMin + rectB.yMax - 1) / 2) ? rectB.yMax - 1 : rectB.yMin;
        Vector2Int end1 = new Vector2Int(endX1, endY1);

        int startY2 = (rectB.center.y > rectA.center.y) ? rectA.yMax - 1 : rectA.yMin;
        int startX2 = (rectA.xMin + rectA.xMax - 1) / 2;
        Vector2Int start2 = new Vector2Int(startX2, startY2);

        int cornerX2 = startX2;
        int cornerY2 = (rectB.yMin + rectB.yMax - 1) / 2;
        Vector2Int corner2 = new Vector2Int(cornerX2, cornerY2);

        int endY2 = cornerY2;
        int endX2 = (cornerX2 > (rectB.xMin + rectB.xMax - 1) / 2) ? rectB.xMax - 1 : rectB.xMin;
        Vector2Int end2 = new Vector2Int(endX2, endY2);

        Vector2Int chosenStart = start1;
        Vector2Int chosenCorner = corner1;
        Vector2Int chosenEnd = end1;

        if (LineHitsAnyRoom(start1, corner1, corridorWidth, otherRoomBounds) || LineHitsAnyRoom(corner1, end1, corridorWidth, otherRoomBounds))
        {
            if (!LineHitsAnyRoom(start2, corner2, corridorWidth, otherRoomBounds) && !LineHitsAnyRoom(corner2, end2, corridorWidth, otherRoomBounds))
            {
                chosenStart = start2;
                chosenCorner = corner2;
                chosenEnd = end2;
            }
        }

        AddThickLine(chosenStart, chosenCorner, corridorWidth, result.Tiles);
        AddThickLine(chosenCorner, chosenEnd, corridorWidth, result.Tiles);

        result.Connections.Add((a, b));
    }

    private static bool LineHitsAnyRoom(Vector2Int from, Vector2Int to, int width, List<RectInt> roomBounds)
    {
        foreach (RectInt bounds in roomBounds)
        {
            if (SegmentIntersectsBounds(from, to, width, bounds))
                return true;
        }
        return false;
    }

    private static bool SegmentIntersectsBounds(Vector2Int from, Vector2Int to, int width, RectInt bounds)
    {
        if (width < 1) width = 1;
        int startOffset = -(width / 2);
        int endOffset = startOffset + width - 1;

        int maxX = bounds.xMax - 1;
        int maxY = bounds.yMax - 1;

        if (from.y == to.y)
        {
            int segMinY = from.y + startOffset;
            int segMaxY = from.y + endOffset;

            if (segMaxY < bounds.yMin || segMinY > maxY)
                return false;

            int minX = Mathf.Min(from.x, to.x);
            int segMaxX = Mathf.Max(from.x, to.x);
            return minX <= maxX && segMaxX >= bounds.xMin;
        }
        else
        {
            int segMinX = from.x + startOffset;
            int segMaxX = from.x + endOffset;

            if (segMaxX < bounds.xMin || segMinX > maxX)
                return false;

            int minY = Mathf.Min(from.y, to.y);
            int segMaxY = Mathf.Max(from.y, to.y);
            return minY <= maxY && segMaxY >= bounds.yMin;
        }
    }

    private static void AddThickLine(Vector2Int from, Vector2Int to, int width, List<Vector2Int> tiles)
    {
        if (width < 1) width = 1;

        int startOffset = -(width / 2);
        int endOffset = startOffset + width;

        if (from.y == to.y)
        {
            int minX = Mathf.Min(from.x, to.x);
            int maxX = Mathf.Max(from.x, to.x);

            for (int x = minX; x <= maxX; x++)
            {
                for (int offset = startOffset; offset < endOffset; offset++)
                {
                    tiles.Add(new Vector2Int(x, from.y + offset));
                }
            }
        }
        else
        {
            int minY = Mathf.Min(from.y, to.y);
            int maxY = Mathf.Max(from.y, to.y);

            for (int y = minY; y <= maxY; y++)
            {
                for (int offset = startOffset; offset < endOffset; offset++)
                {
                    tiles.Add(new Vector2Int(from.x + offset, y));
                }
            }
        }
    }
}