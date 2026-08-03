using UnityEngine;
using System.Collections.Generic;

public class DungeonData 
{
    public List<RoomInstance> Rooms { get; }
    public List<Vector2Int> CorridorTiles { get; }
    public List<(RoomInstance a, RoomInstance b)> Connections { get; }

    public DungeonData(List<RoomInstance> rooms, List<Vector2Int> corridorTiles, List<(RoomInstance a, RoomInstance b)> connections) 
    {
        Rooms = rooms;
        CorridorTiles = corridorTiles;
        Connections = connections;
    }
}