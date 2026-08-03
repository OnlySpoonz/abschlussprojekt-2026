using UnityEngine;
using System.Collections.Generic;

public static class RoomFactory 
{
    public static List<RoomInstance> CreateRooms(List<BSPNode> leaves, bool useRandomSize) 
    {
        List<RoomInstance> rooms = new List<RoomInstance>(leaves.Count);

        for (int i = 0; i < leaves.Count; i++) 
        {
            RectInt leafBounds = leaves[i].Bounds;

            RoomShape shape = ShapeGenerator.GenerateRandomShape(leafBounds, useRandomSize);

            rooms.Add(new RoomInstance(i, shape));
        }
        return rooms;
    }
}