using UnityEngine;

public static class ShapeGenerator 
{
    public static RoomShape GenerateRandomShape(RectInt leafBounds, bool useRandomSize) 
    {
        return new RectangleShape(leafBounds, useRandomSize);
    }
}